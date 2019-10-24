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
        public ManualResetEvent pauseBouncerAndPatrons = new ManualResetEvent(true); // starts out in a signaled state, meaning it does not block by default.
        public ManualResetEvent pauseBartender = new ManualResetEvent(true); 
        public ManualResetEvent pauseWaitress = new ManualResetEvent(true); 
        Pub pub;
        public MainWindow()
        {
            InitializeComponent();
            pub = new Pub(this);
        }

        private void HandlePauseAndResume(LogBox logbox)
        {

        }

        private void Pause_Bartender_Click(object sender, RoutedEventArgs e)
        {
            if (Pause_Bartender.Content.ToString() == "Pause")
            {
                pauseBartender.Reset();
                Pause_Bartender.Content = "Resume";
            }
            else
            {
                pauseBartender.Set();
                Pause_Bartender.Content = "Pause";
            }
        }
        private void Pause_Waitress_Click(object sender, RoutedEventArgs e)
        {
            if (Pause_Waitress.Content.ToString() == "Pause")
            {
                pauseWaitress.Reset();
                Pause_Waitress.Content = "Resume";
            }
            else
            {
                pauseWaitress.Set();
                Pause_Waitress.Content = "Pause";
            }
        }
        private void Pause_Guests_Click(object sender, RoutedEventArgs e)
        {
            if (Pause_Guests.Content.ToString() == "Pause")
            {
                pauseBouncerAndPatrons.Reset();
                Pause_Guests.Content = "Resume";
            }
            else
            {
                pauseBouncerAndPatrons.Set();
                Pause_Guests.Content = "Pause";
            }
        }
        private void ToggleBarOpen_Click(object sender, RoutedEventArgs e)
        {
            if (pub.IsOpen)
            {
                ToggleBarOpen.Content = "Open bar";
                pub.Stop();
            }
            else
            {
                ToggleBarOpen.Content = "Close bar";
                LogEvent(TestCase.SelectedIndex.ToString(), LogBox.Event);
                pub.Start();
            }
        }
        
        // Medveten om att den kanske bör "stoppa" alla trådar perma och inte pausa,
        // men detta kan funka tills det är löst.
        private void Panic_Click(object sender, RoutedEventArgs e)
        {
            if (Panic.Content.ToString() == "Panic! Pause all threads!")
            {
                pauseBouncerAndPatrons.Reset();
                pauseBartender.Reset();
                pauseWaitress.Reset();
                Panic.Content = "Phew! Crysis averted... :-)";
            }
            else
            {
                pauseBouncerAndPatrons.Set();
                pauseBartender.Set();
                pauseWaitress.Set();
                Panic.Content = "Panic! Pause all threads!";
            }
        }

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
        private Task bartender;
        private Task waitress;
        private Task bouncer;
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

            bouncer = Task.Run(() => 
            {
                new Bouncer(this);
            });

            bartender = Task.Run(() =>
            {
                new Bartender(this);
            });


            waitress = Task.Run(() => 
            {
                new Waitress(this);
            });
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

            while (bar.IsOpen)
            {
                // Ligger här för att också stoppa denna task för att köra om och om igen, och sedan spotta ut en jäkla massa patrons
                bar.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);
                Thread.Sleep(new Random().Next(bar.BouncerMinTiming, bar.BouncerMaxTiming));
                Task.Run(() =>
                {
                    // Ligger här för att stoppa denna tasken att skapa en ny patron hela tiden
                    bar.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);
                    new Patron(bar);
                });
            }

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

            while (bar.IsOpen)
            {
                bar.mainWindow.pauseBartender.WaitOne(Timeout.Infinite);

                currentPatron = WaitForPatron();
                currentGlass = WaitForGlass();
                bar.mainWindow.pauseBartender.WaitOne(Timeout.Infinite);

                bar.Log("Pouring beer...", LogBox.Bartender);
                Thread.Sleep(bar.BartenderPourTiming);

                lock (bar.BarDisk)
                {
                    bar.BarDisk.Add(currentPatron, currentGlass);
                }
            }
        }
        Patron WaitForPatron()
        {
            while (true)
            {
                bar.mainWindow.pauseBartender.WaitOne(Timeout.Infinite);

                if (bar.WaitingPatrons.TryDequeue(out Patron patron))
                {
                    return patron;
                }
            }
        }

        Glass  WaitForGlass()
        {
            while (true)
            {

                if (bar.Shelf.TryPeek(out _))
                {
                    bar.mainWindow.pauseBartender.WaitOne(Timeout.Infinite);

                    bar.Log("Collecting glass...", LogBox.Bartender);
                    Thread.Sleep(bar.BartenderGlassTiming);
                    bar.mainWindow.pauseBartender.WaitOne(Timeout.Infinite);

                    bar.Shelf.TryPop(out Glass glass);
                    return glass;
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

            while (bar.IsOpen)
            {
                bar.mainWindow.pauseWaitress.WaitOne(Timeout.Infinite);

                TakeEmptyGlasses();
                PlaceGlass();
            }

        }

        private void TakeEmptyGlasses()
        {
            while (!bar.Table.IsEmpty)
            {
                bar.mainWindow.pauseWaitress.WaitOne(Timeout.Infinite);

                if (bar.Table.TryPop(out Glass currentGlass))
                {
                    glasses.Push(currentGlass);
                }
            }
            if (glasses.Count != 0)
            {
                bar.mainWindow.pauseWaitress.WaitOne(Timeout.Infinite);

                bar.Log("Goes to collect glasses...", LogBox.Waitress);
                Thread.Sleep(bar.WaitressClearTiming);

            }
        }

        private void PlaceGlass()
        {
            if (glasses.Count > 0)
            {
                bar.mainWindow.pauseWaitress.WaitOne(Timeout.Infinite);

                bar.Log("Does dishes...", LogBox.Waitress);
                Thread.Sleep(bar.WaitressPlaceTiming);
                while (glasses.Count > 0)
                {
                    bar.mainWindow.pauseWaitress.WaitOne(Timeout.Infinite);

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

            bar.Log("Patron enters the bar", LogBox.Patron);
            Thread.Sleep(bar.PatronArriveTiming);
            bar.WaitingPatrons.Enqueue(this);
            bar.Log("Number of Waiting Patrons: " + bar.WaitingPatrons.Count, LogBox.Waitress);
            WaitForGlass();
            WaitForTable();
            DrinkAndLeave();

        }
        private void WaitForGlass()
        {
            while (true)
            {
                bar.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);

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
                    bar.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);

                    Thread.Sleep(bar.PatronTableTiming);
                    lock (bar.TakenChairs)
                    {
                        bar.TakenChairs.Add(this);
                    }
                    bar.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);

                    bar.Log("Got chair", LogBox.Patron);
                    return;
                }
            }
        }

        private void DrinkAndLeave()
        {
            bar.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);

            Thread.Sleep(new Random().Next(bar.PatronMinDrinkTiming, bar.PatronMaxDrinkTiming));
            lock (bar.TakenChairs)
            {
                bar.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);

                bar.TakenChairs.Remove(this);
                bar.Table.Push(glass);
                bar.Log("Patron left", LogBox.Patron);
            }
        }
    }
}