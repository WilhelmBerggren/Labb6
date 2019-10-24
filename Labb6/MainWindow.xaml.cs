using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Labb6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public enum LogBox { Event, Bartender, Waitress, Patron }
    public partial class MainWindow : Window
    {
        Pub pub;
        public MainWindow()
        {
            InitializeComponent();
            pub = new Pub(this);
        }

        private void Pause_Bartender_Click(object sender, RoutedEventArgs e) { }
        private void Pause_Waitress_Click(object sender, RoutedEventArgs e) { }
        private void Pause_Guests_Click(object sender, RoutedEventArgs e) { }
        private void ToggleBarOpen_Click(object sender, RoutedEventArgs e)
        {
            if (pub.IsOpen)
            {
                pub.Stop();
            }
            else
            {
                LogEvent(TestCase.SelectedIndex.ToString(), LogBox.Event);
                pub.Start();
            }
        }
        private void Panic_Click(object sender, RoutedEventArgs e) { pub.IsOpen = false; }

        public void LogEvent(string text, LogBox textblock)
        {
            switch (textblock)
            {
                case LogBox.Event:
                    this.Dispatcher.Invoke(() => EventListBox.Items.Insert(0, text));
                    break;
                case LogBox.Bartender:
                    this.Dispatcher.Invoke(() => BartenderListBox.Items.Insert(0, text));
                    break;
                case LogBox.Patron:
                    this.Dispatcher.Invoke(() => PatronListBox.Items.Insert(0, text));
                    break;
                case LogBox.Waitress:
                    this.Dispatcher.Invoke(() => WaitressListBox.Items.Insert(0, text));
                    break;
            }
        }
    }

    public class Pub
    {
        public bool IsOpen { get; set; }
        public MainWindow mainWindow;

        public ConcurrentQueue<Patron> WaitingPatrons { get; set; }
        private Bartender bartender;
        private Waitress waitress;
        private Bouncer bouncer;
        public ConcurrentStack<Glass> Shelf { get; set; }
        public Dictionary<Patron, Glass> BarDisk { get; set; }
        public ConcurrentStack<Glass> Table { get; set; }
        public List<Patron> TakenChairs { get; set; }
        public int BartenderGlassTiming { get; }
        public int BartenderPourTiming { get; }
        public int WaitressClearTiming { get; }
        public int WaitressPlaceTiming { get; }
        public int BouncerMinTiming { get; }
        public int BouncerMaxTiming { get; }
        public int PatronArriveTiming { get; }
        public int PatronTableTiming { get; }
        public int PatronMinDrinkTiming { get; }
        public int PatronMaxDrinkTiming { get; }
        public int NumberOfGlasses { get; }
        public int NumberOfChairs { get; }

        public Pub(MainWindow mainWindow,
            int BartenderGlassTiming = 3000,
            int BartenderPourTiming = 3000,
            int WaitressClearTiming = 10000,
            int WaitressPlaceTiming = 15000,
            int BouncerMinTiming = 3000,
            int BouncerMaxTiming = 10000,
            int PatronArriveTiming = 1000,
            int PatronTableTiming = 4000,
            int PatronMinDrinkTiming = 10000,
            int PatronMaxDrinkTiming = 20000,
            int NumberOfGlasses = 8,
            int NumberOfChairs = 9)
        {
            this.mainWindow = mainWindow;
            this.BartenderGlassTiming = BartenderGlassTiming;
            this.BartenderPourTiming = BartenderPourTiming;
            this.WaitressClearTiming = WaitressClearTiming;
            this.WaitressPlaceTiming = WaitressPlaceTiming;
            this.BouncerMinTiming = BouncerMinTiming;
            this.BouncerMaxTiming = BouncerMaxTiming;
            this.PatronArriveTiming = PatronArriveTiming;
            this.PatronTableTiming = PatronTableTiming;
            this.PatronMinDrinkTiming = PatronMinDrinkTiming;
            this.PatronMaxDrinkTiming = PatronMaxDrinkTiming;
            this.NumberOfGlasses = NumberOfGlasses;
            this.NumberOfChairs = NumberOfChairs;

            IsOpen = false;
            Table = new ConcurrentStack<Glass>();
            WaitingPatrons = new ConcurrentQueue<Patron>();
            TakenChairs = new List<Patron>();
            BarDisk = new Dictionary<Patron, Glass>();
            Shelf = new ConcurrentStack<Glass>(Enumerable.Range(0, NumberOfGlasses).Select(i => new Glass()));
        }

        public void Start()
        {
            IsOpen = true;
            InfoPrinter();

            bouncer = new Bouncer(this);
            bartender = new Bartender(this);
            waitress = new Waitress(this);
        }

        public void Stop()
        {
            IsOpen = false;
        }

        public void Log(string text, LogBox listbox)
        {
            mainWindow.LogEvent(text, listbox);
        }

        private void InfoPrinter()
        {
            Task.Run(() =>
            {
                while (IsOpen)
                {
                    Log($"Taken chairs: {TakenChairs.Count}, Waiting Patrons: {WaitingPatrons.Count}, Glasses: {Shelf.Count}", LogBox.Event);
                    Thread.Sleep(1000);
                }
            });
        }
    }

    public class Glass { }

    public class Bouncer
    {
        public Bouncer(Pub bar)
        {
            Task.Run(() =>
            {
                while (bar.IsOpen)
                {
                    Thread.Sleep(new Random().Next(bar.BouncerMinTiming, bar.BouncerMaxTiming));
                    new Patron(bar);
                }
            });
        }
    }

    public class Bartender
    {
        Pub bar;
        Glass currentGlass;
        Patron currentPatron;
        public Bartender(Pub bar)
        {
            this.bar = bar;
            Task.Run(() =>
            {
                while (bar.IsOpen)
                {
                    currentPatron = WaitForPatron();
                    currentGlass = WaitForGlass();

                    bar.Log("Pouring beer...", LogBox.Bartender);
                    Thread.Sleep(bar.BartenderPourTiming);

                    lock (bar.BarDisk)
                    {
                        bar.BarDisk.Add(currentPatron, currentGlass);
                    }
                }
            });
        }

        Glass WaitForGlass()
        {
            while (true)
            {
                if (bar.Shelf.TryPeek(out _))
                {
                    bar.Log("Collecting glass...", LogBox.Bartender);
                    Thread.Sleep(bar.BartenderGlassTiming);
                    bar.Shelf.TryPop(out Glass glass);
                    return glass;
                }
            }
        }
        Patron WaitForPatron()
        {
            while (true)
            {
                if (bar.WaitingPatrons.TryDequeue(out Patron patron))
                {
                    return patron;
                }
            }
        }
    }

    public class Waitress
    {
        Pub bar;
        Stack<Glass> glasses;
        public Waitress(Pub bar)
        {
            glasses = new Stack<Glass>();
            this.bar = bar;
            Task.Run(() =>
            {
                while (bar.IsOpen)
                {
                    TakeEmptyGlasses();
                    PlaceGlass();
                }
            });
        }

        private void TakeEmptyGlasses()
        {
            while (!bar.Table.IsEmpty)
            {
                if (bar.Table.TryPop(out Glass currentGlass))
                {
                    glasses.Push(currentGlass);
                }
            }
            if (glasses.Count != 0)
            {
                bar.Log("Goes to collect glasses...", LogBox.Waitress);
                Thread.Sleep(bar.WaitressClearTiming);

            }
        }

        private void PlaceGlass()
        {
            if (glasses.Count > 0)
            {
                bar.Log("Does dishes...", LogBox.Waitress);
                Thread.Sleep(bar.WaitressPlaceTiming);
                while (glasses.Count > 0)
                {
                    bar.Shelf.Push(glasses.Pop());
                }
                bar.Log("Placed clean glasses in shelf...", LogBox.Waitress);
            }
        }
    }

    public class Patron
    {
        private Pub bar;
        private Glass glass;
        public Patron(Pub bar)
        {
            this.bar = bar;

            Task.Run(() =>
            {
                bar.Log("Patron enters the bar", LogBox.Patron);
                Thread.Sleep(bar.PatronArriveTiming);
                bar.WaitingPatrons.Enqueue(this);
                bar.Log("Number of Waiting Patrons: " + bar.WaitingPatrons.Count, LogBox.Waitress);
                WaitForGlass();
                WaitForTable();
                DrinkAndLeave();
            });
        }
        private void WaitForGlass()
        {
            while (true)
            {
                if (bar.BarDisk.ContainsKey(this))
                {
                    lock (bar.BarDisk)
                    {
                        this.glass = bar.BarDisk[this];
                        bar.BarDisk.Remove(this);
                    }
                    bar.Log("Got glass", LogBox.Patron);
                    return;
                }
            }
        }

        private void WaitForTable()
        {
            bool foundTable = false;
            while (!foundTable)
            {
                if (bar.TakenChairs.Count < bar.NumberOfChairs)
                {
                    Thread.Sleep(bar.PatronTableTiming);
                    lock (bar.TakenChairs)
                    {
                        bar.TakenChairs.Add(this);
                    }
                    bar.Log("Got chair", LogBox.Patron);
                    return;
                }
            }
        }

        private void DrinkAndLeave()
        {
            Thread.Sleep(new Random().Next(bar.PatronMinDrinkTiming, bar.PatronMaxDrinkTiming));
            lock (bar.TakenChairs)
            {
                bar.TakenChairs.Remove(this);
                bar.Table.Push(glass);
                bar.Log("Patron left", LogBox.Patron);
            }
        }
    }
}