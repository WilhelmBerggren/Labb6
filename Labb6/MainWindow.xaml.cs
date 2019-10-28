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
        internal ManualResetEvent pauseBouncerAndPatrons; // starts out in a signaled state, meaning it does not block by default.
        internal ManualResetEvent pauseBartender;
        internal ManualResetEvent pauseWaitress;
        internal CancellationTokenSource tokenSource;
        internal CancellationToken token;

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
                PanicButton.Content = "Phew! Crisis averted... :-)";
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
        internal MainWindow mainWindow;

        internal ConcurrentQueue<Patron> WaitingPatrons { get; set; }
        private Bartender bartender;
        private Waitress waitress;
        private Bouncer bouncer;
        internal ConcurrentStack<Glass> Shelf { get; set; }
        internal Dictionary<Patron, Glass> BarDisk { get; set; }
        internal ConcurrentStack<Glass> Table { get; set; }
        internal List<Patron> TakenChairs { get; set; }
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
            RunAsTask(() =>
            {
                mainWindow.token.ThrowIfCancellationRequested();
                Log($"Taken chairs: {TakenChairs.Count}, Waiting Patrons: {WaitingPatrons.Count}, Glasses: {Shelf.Count}", LogBox.Event);
                Thread.Sleep(1000);
            });
        }

        public void RunAsTask(Action action)
        {
            Task.Run(() => 
            {
                try
                {
                    while (this.IsOpen)
                    {
                        this.mainWindow.token.ThrowIfCancellationRequested();
                        action();
                    }
                }
                catch (OperationCanceledException) { }
            }, mainWindow.token);
        }
    }

    public class Glass { }

    public abstract class Agent
    {
        public static void Run(Pub pub, Action Loop)
        {
            pub.RunAsTask(() =>
            {
                // Ligger här för att också stoppa denna task för att köra om och om igen, och sedan spotta ut en jäkla massa patrons
                pub.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);
                Loop();
            });
        }
    }

    public class Bouncer
    {
        public Bouncer(Pub pub)
        {
            pub.RunAsTask(() =>
            {
                Thread.Sleep(new Random().Next(pub.BouncerMinTiming, pub.BouncerMaxTiming));
                Task.Run(() =>
                {
                    // Ligger här för att stoppa denna tasken att skapa en ny patron hela tiden
                    pub.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);
                    _ = new Patron(pub);
                }, pub.mainWindow.token);
            });
        }
    }

    public class Bartender
    {
        Pub pub;
        Glass currentGlass;
        Patron currentPatron;
        public Bartender(Pub pub)
        {
            this.pub = pub;
            pub.RunAsTask(() =>
            {
                pub.mainWindow.token.ThrowIfCancellationRequested();
                pub.mainWindow.pauseBartender.WaitOne(Timeout.Infinite);

                currentPatron = WaitForPatron();
                currentGlass = WaitForGlass();
                pub.mainWindow.pauseBartender.WaitOne(Timeout.Infinite);

                pub.Log("Pouring beer...", LogBox.Bartender);
                Thread.Sleep(pub.BartenderPourTiming);

                lock (pub.BarDisk)
                {
                    pub.BarDisk.Add(currentPatron, currentGlass);
                }
            });
        }
        Patron WaitForPatron()
        {
            while (true)
            {
                pub.mainWindow.pauseBartender.WaitOne(Timeout.Infinite);

                if (pub.WaitingPatrons.TryDequeue(out Patron patron))
                {
                    return patron;
                }
            }
        }

        Glass WaitForGlass()
        {
            while (true)
            {
                pub.mainWindow.pauseBartender.WaitOne(Timeout.Infinite);
                if (pub.Shelf.TryPeek(out _))
                {

                    pub.Log("Collecting glass...", LogBox.Bartender);
                    Thread.Sleep(pub.BartenderGlassTiming);
                    pub.mainWindow.pauseBartender.WaitOne(Timeout.Infinite);

                    pub.Shelf.TryPop(out Glass glass);
                    return glass;
                }
            }
        }
    }

    public class Waitress
    {
        readonly Pub pub;
        readonly Stack<Glass> glasses;
        public Waitress(Pub pub)
        {
            glasses = new Stack<Glass>();
            this.pub = pub;
            pub.RunAsTask(() =>
            {
                TakeEmptyGlasses();
                PlaceGlass();
            });
        }

        private void TakeEmptyGlasses()
        {
            while (!pub.Table.IsEmpty)
            {
                pub.mainWindow.pauseWaitress.WaitOne(Timeout.Infinite);

                if (pub.Table.TryPop(out Glass currentGlass))
                {
                    glasses.Push(currentGlass);
                }
            }
            if (glasses.Count != 0)
            {
                pub.mainWindow.pauseWaitress.WaitOne(Timeout.Infinite);

                pub.Log("Goes to collect glasses...", LogBox.Waitress);
                Thread.Sleep(pub.WaitressClearTiming);

            }
        }

        private void PlaceGlass()
        {
            if (glasses.Count > 0)
            {
                pub.mainWindow.pauseWaitress.WaitOne(Timeout.Infinite);

                pub.Log("Does dishes...", LogBox.Waitress);
                Thread.Sleep(pub.WaitressPlaceTiming);
                while (glasses.Count > 0)
                {
                    pub.mainWindow.pauseWaitress.WaitOne(Timeout.Infinite);

                    pub.Shelf.Push(glasses.Pop());
                }
                pub.Log("Placed clean glasses in shelf...", LogBox.Waitress);
            }
        }
    }

    public class Patron
    {
        private Pub pub;
        private Glass glass;
        private string patronName;
        private string[] nameArray;
        public Patron(Pub pub)
        {
            this.pub = pub;
            nameArray = new string[25]
            {
                "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda","William",
                "Elizabeth", "David", "Barbara", "Richard", "Susan","Joseph", "Jessica", "Thomas", "Sarah",
                "Charles", "Karen", "Christopher", "Nancy", "Daniel", "Margaret", "Lisa"
            };
            SetName();

            PrintPatronInfo();
            Thread.Sleep(pub.PatronArriveTiming);
            pub.WaitingPatrons.Enqueue(this);
            pub.Log("Number of Waiting Patrons: " + pub.WaitingPatrons.Count, LogBox.Waitress);

            WaitForGlass();
            WaitForTable();
            DrinkAndLeave();

        }

        private void PrintPatronInfo()
        {
            if (this.patronName == "Karen")
                pub.Log($"{patronName} enters the pub. She wants to speak to the manager!", LogBox.Patron);
            else
                pub.Log($"{patronName} enters the pub", LogBox.Patron);
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
                    pub.mainWindow.token.ThrowIfCancellationRequested();
                    pub.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);

                    if (pub.BarDisk.ContainsKey(this))
                    {
                        lock (pub.BarDisk)
                        {
                            this.glass = pub.BarDisk[this];
                            pub.BarDisk.Remove(this);
                        }
                        pub.Log($"{patronName} got a glass", LogBox.Patron);
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
                    pub.mainWindow.token.ThrowIfCancellationRequested();

                    if (pub.TakenChairs.Count < pub.NumberOfChairs)
                    {
                        pub.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);

                        Thread.Sleep(pub.PatronTableTiming);
                        lock (pub.TakenChairs)
                        {
                            pub.TakenChairs.Add(this);
                        }
                        pub.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);

                        pub.Log($"{patronName} found a chair", LogBox.Patron);
                        return;
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        private void DrinkAndLeave()
        {
            pub.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);

            Thread.Sleep(new Random().Next(pub.PatronMinDrinkTiming, pub.PatronMaxDrinkTiming));
            lock (pub.TakenChairs)
            {
                pub.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);

                pub.TakenChairs.Remove(this);
                pub.Table.Push(glass);
                pub.Log($"{patronName} left", LogBox.Patron);
            }
        }
    }
}