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
    public enum SetBarState { WantsToOpen, WantsToClose }
    public partial class MainWindow : Window
    {
        public ManualResetEvent pauseBouncerAndPatrons; // starts out in a signaled state, meaning it does not block by default.
        public ManualResetEvent pauseBartender;
        public ManualResetEvent pauseWaitress;
        public CancellationTokenSource tokenSource;
        public CancellationToken token;

        private bool SelectionIsMade = false;

        private Pub pub;

        public MainWindow()
        {
            InitializeComponent();
            pauseBouncerAndPatrons = new ManualResetEvent(true);
            pauseBartender = new ManualResetEvent(true);
            pauseWaitress = new ManualResetEvent(true);
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
        }

        private void Pause_Bartender_Click(object sender, RoutedEventArgs e)
        {
            if (Pause_BartenderButton.Content.ToString() == "Pause")
            {
                pauseBartender.Reset();
                Pause_BartenderButton.Content = "Resume";
            }
            else
            {
                pauseBartender.Set();
                Pause_BartenderButton.Content = "Pause";
            }
        }

        private void Pause_Waitress_Click(object sender, RoutedEventArgs e)
        {
            if (Pause_WaitressButton.Content.ToString() == "Pause")
            {
                pauseWaitress.Reset();
                Pause_WaitressButton.Content = "Resume";
            }
            else
            {
                pauseWaitress.Set();
                Pause_WaitressButton.Content = "Pause";
            }
        }
        private void Pause_Guests_Click(object sender, RoutedEventArgs e)
        {
            if (Pause_GuestsButton.Content.ToString() == "Pause")
            {
                pauseBouncerAndPatrons.Reset();
                Pause_GuestsButton.Content = "Resume";
            }
            else
            {
                pauseBouncerAndPatrons.Set();
                Pause_GuestsButton.Content = "Pause";
            }
        }
        private void ToggleBarOpen_Click(object sender, RoutedEventArgs e)
        {
            if (SelectionIsMade == false)
            {
                switch (TestCase.SelectedValue.ToString().Substring(38))
                {
                    case "Default":
                        pub = new Pub(this);
                        OpenOrCloseBar();
                        break;
                    case "20 Glasses, 3 chairs":
                        pub = new Pub(this, NumberOfGlasses: 20, NumberOfChairs: 3);
                        OpenOrCloseBar();
                        break;
                    case "20 Chairs, 5 Glasses":
                        pub = new Pub(this, NumberOfChairs: 20, NumberOfGlasses: 5);
                        OpenOrCloseBar();
                        break;
                    case "Double Stay (Patrons)":
                        pub = new Pub(this, PatronArriveTiming: 2000, PatronTableTiming: 8000, PatronMinDrinkTiming: 20000, PatronMaxDrinkTiming: 40000);
                        OpenOrCloseBar();
                        break;
                    case "Double Speed Waitress":
                        pub = new Pub(this, WaitressClearTiming: 5000, WaitressPlaceTiming: 7500);
                        OpenOrCloseBar();
                        break;
                    //case "5 Minutes open":
                    //  Not yet implemented countdown.
                    //   break;
                    //case "Couples night":
                    // Not yet implemented. Bouncer creates two instances of patrons per turn.
                    //   break;
                    //case "Bouncer is a jerk":
                    // Double time to bounce patrons, but after the first 20 sec (ONLY the first 20 sec), let in 15 guests at the same time.
                    //break;
                    default:
                        break;
                }
            }
            else
                OpenOrCloseBar();
        }

        private void Countdown(int count)
        {

        }

        private void OpenOrCloseBar()
        {
            if (pub.IsOpen)
                OpenOrCloseBar(SetBarState.WantsToClose);
            else
                OpenOrCloseBar(SetBarState.WantsToOpen);
        }

        private void OpenOrCloseBar(SetBarState desiredState)
        {
            if (pub == null)
                return;

            if (desiredState == SetBarState.WantsToClose)
            {
                pub.CloseTheBar();
                SelectionIsMade = false;
                PatronListBox.Items.Clear();
                BartenderListBox.Items.Clear();
                WaitressListBox.Items.Clear();
                EventListBox.Items.Clear();
                Pause_GuestsButton.Content = "Pause";
                Pause_GuestsButton.IsEnabled = false;
                Pause_BartenderButton.Content = "Pause";
                Pause_BartenderButton.IsEnabled = false;
                Pause_WaitressButton.Content = "Pause";
                Pause_WaitressButton.IsEnabled = false;
                TestCase.IsEnabled = true;
                ToggleBarOpenButton.Content = "Open bar";
                PanicButton.Content = "Panic! Pause all threads!";
            }
            else
            {
                pub.OpenTheBar();
                SelectionIsMade = true;
                tokenSource = new CancellationTokenSource();
                token = tokenSource.Token;
                ToggleBarOpenButton.Content = "Close bar";
                LogEvent("TestCase: " + TestCase.SelectedValue.ToString().Substring(38), LogBox.Event);
                Pause_GuestsButton.IsEnabled = true;
                Pause_BartenderButton.IsEnabled = true;
                Pause_WaitressButton.IsEnabled = true;
                TestCase.IsEnabled = false;
            }
        }

        // Medveten om att den kanske bör "stoppa" alla trådar perma och inte pausa,
        // men detta kan funka tills det är löst.
        private void Panic_Click(object sender, RoutedEventArgs e)
        {
            if (pub == null)
                return;

            if (PanicButton.Content.ToString() == "Panic! Pause all threads!")
            {
                OpenOrCloseBar(SetBarState.WantsToClose);
                PanicButton.Content = "Phew! Crysis averted... :-)";
            }
            else
            {
                OpenOrCloseBar(SetBarState.WantsToOpen);
                PanicButton.Content = "Panic! Pause all threads!";
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
        public bool IsOpen { get; set; } = false;
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

        public void OpenTheBar()
        {
            IsOpen = true;
            InfoPrinter();
            bouncer = new Bouncer(this);
            bartender = new Bartender(this);
            waitress = new Waitress(this);
        }

        public void CloseTheBar()
        {
            IsOpen = false;
            mainWindow.tokenSource.Cancel();
        }

        public void Log(string text, LogBox listbox)
        {
            mainWindow.LogEvent(text, listbox);
        }

        private void InfoPrinter()
        {
            Task.Run(() =>
            {
                try
                {
                    while (IsOpen)
                    {
                        mainWindow.token.ThrowIfCancellationRequested();
                        Log($"Taken chairs: {TakenChairs.Count}, Waiting Patrons: {WaitingPatrons.Count}, Glasses: {Shelf.Count}", LogBox.Event);
                        Thread.Sleep(1000);
                    }
                }
                catch (OperationCanceledException) { }
            }, mainWindow.token);
        }
    }

    public class Glass { }

    public class Bouncer
    {
        public Bouncer(Pub bar)
        {
            Task.Run(() =>
            {
                try
                {
                    while (bar.IsOpen)
                    {
                        bar.mainWindow.token.ThrowIfCancellationRequested();
                        // Ligger här för att också stoppa denna task för att köra om och om igen, och sedan spotta ut en jäkla massa patrons
                        bar.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);
                        Thread.Sleep(new Random().Next(bar.BouncerMinTiming, bar.BouncerMaxTiming));
                        Task.Run(() =>
                        {
                            // Ligger här för att stoppa denna tasken att skapa en ny patron hela tiden
                            bar.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);
                            new Patron(bar);
                        }, bar.mainWindow.token);
                    }
                }
                catch (OperationCanceledException) { }
            }, bar.mainWindow.token);
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
                try
                {
                    while (bar.IsOpen)
                    {
                        bar.mainWindow.token.ThrowIfCancellationRequested();
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
                catch (OperationCanceledException) { }
            }, bar.mainWindow.token);
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

        Glass WaitForGlass()
        {
            while (true)
            {
                bar.mainWindow.pauseBartender.WaitOne(Timeout.Infinite);
                if (bar.Shelf.TryPeek(out _))
                {

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
            Task.Run(() =>
            {
                try
                {
                    while (bar.IsOpen)
                    {
                        bar.mainWindow.pauseWaitress.WaitOne(Timeout.Infinite);

                        TakeEmptyGlasses();
                        PlaceGlass();
                    }
                }
                catch (OperationCanceledException) { }
            }, bar.mainWindow.token);
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
        private string patronName;
        private string[] nameArray;
        public Patron(Pub bar)
        {
            this.bar = bar;
            nameArray = new string[25]
            {
                "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda","William",
                "Elizabeth", "David", "Barbara", "Richard", "Susan","Joseph", "Jessica", "Thomas", "Sarah",
                "Charles", "Karen", "Christopher", "Nancy", "Daniel", "Margaret", "Lisa"
            };
            SetName();

            PrintPatronInfo();
            Thread.Sleep(bar.PatronArriveTiming);
            bar.WaitingPatrons.Enqueue(this);
            bar.Log("Number of Waiting Patrons: " + bar.WaitingPatrons.Count, LogBox.Waitress);

            WaitForGlass();
            WaitForTable();
            DrinkAndLeave();

        }

        private void PrintPatronInfo()
        {
            if (this.patronName == "Karen")
                bar.Log($"{patronName} enters the bar. She wants to speak to the manager!", LogBox.Patron);
            else
                bar.Log($"{patronName} enters the bar", LogBox.Patron);
        }
        private void SetName()
        {
            int selectedName = new Random().Next(0, nameArray.Length);
            this.patronName = nameArray[selectedName];
        }

        private void WaitForGlass()
        {
            try
            {
                while (true)
                {
                    bar.mainWindow.token.ThrowIfCancellationRequested();
                    bar.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);

                    if (bar.BarDisk.ContainsKey(this))
                    {
                        lock (bar.BarDisk)
                        {
                            this.glass = bar.BarDisk[this];
                            bar.BarDisk.Remove(this);
                        }
                        bar.Log($"{patronName} got a glass", LogBox.Patron);
                        return;
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        private void WaitForTable()
        {
            bool foundTable = false;
            try
            {
                while (!foundTable)
                {
                    bar.mainWindow.token.ThrowIfCancellationRequested();

                    if (bar.TakenChairs.Count < bar.NumberOfChairs)
                    {
                        bar.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);

                        Thread.Sleep(bar.PatronTableTiming);
                        lock (bar.TakenChairs)
                        {
                            bar.TakenChairs.Add(this);
                        }
                        bar.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);

                        bar.Log($"{patronName} found a chair", LogBox.Patron);
                        return;
                    }
                }
            }
            catch (OperationCanceledException) { }
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
                bar.Log($"{patronName} left", LogBox.Patron);
            }
        }
    }
}