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
    public enum BarState { WantsToOpen, WantsToClose }
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
                        pub = new Pub(this);
                        pub.Params["NumberOfGlasses"] = 20;
                        pub.Params["NumberOfChairs"] = 3;
                        OpenOrCloseBar();
                        break;
                    case "20 Chairs, 5 Glasses":
                        pub = new Pub(this);
                        pub.Params["NumberOfChairs"] = 20;
                        pub.Params["NumberOfGlasses"] = 5;
                        OpenOrCloseBar();
                        break;
                    case "Double Stay (Patrons)":
                        pub = new Pub(this);
                        pub.Params["PatronArriveTiming"] = 2000;
                        pub.Params["PatronTableTiming"] = 8000;
                        pub.Params["PatronMinDrinkTiming"] = 20000;
                        pub.Params["PatronMaxDrinkTiming"] = 40000;
                        OpenOrCloseBar();
                        break;
                    case "Double Speed Waitress":
                        pub = new Pub(this);
                        pub.Params["WaitressClearTiming"] = 5000;
                        pub.Params["WaitressPlaceTiming"] = 7500;
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

        private void OpenOrCloseBar()
        {
            if (pub.IsOpen)
                OpenOrCloseBar(BarState.WantsToClose);
            else
                OpenOrCloseBar(BarState.WantsToOpen);
        }

        private void OpenOrCloseBar(BarState desiredState)
        {
            if (pub == null)
                return;

            if (desiredState == BarState.WantsToClose)
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
                OpenOrCloseBar(BarState.WantsToClose);
                PanicButton.Content = "Phew! Crisis averted... :-)";
            }
            else
            {
                OpenOrCloseBar(BarState.WantsToOpen);
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
        public Dictionary<string, int> Params { get; }

        public Pub(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            this.Params = new Dictionary<string, int>()
            {
                { "BartenderGlassTiming", 3000 },
                { "BartenderPourTiming", 3000 },
                { "WaitressClearTiming", 10000 },
                { "WaitressPlaceTiming", 15000 },
                { "BouncerMinTiming", 3000 },
                { "BouncerMaxTiming", 10000 },
                { "PatronArriveTiming", 1000 },
                { "PatronTableTiming", 4000 },
                { "PatronMinDrinkTiming", 10000 },
                { "PatronMaxDrinkTiming", 20000 },
                { "NumberOfGlasses", 8 },
                { "NumberOfChairs", 9 }
            };

            IsOpen = false;
            Table = new ConcurrentStack<Glass>();
            WaitingPatrons = new ConcurrentQueue<Patron>();
            TakenChairs = new List<Patron>();
            BarDisk = new Dictionary<Patron, Glass>();
            Shelf = new ConcurrentStack<Glass>(Enumerable.Range(0, Params["NumberOfGlasses"]).Select(i => new Glass()));
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

        public static void TryWhile(bool cond, Action action)
        {
            try
            {
                while(cond)
                {
                    action();
                }
            }
            catch (OperationCanceledException) { }
        }

        public void RunAsTask(Action action)
        {
            Task.Run(() => 
            {
                TryWhile(this.IsOpen, () =>
                {
                    this.mainWindow.token.ThrowIfCancellationRequested();
                    // Ligger här för att också stoppa denna task för att köra om och om igen, och sedan spotta ut en jäkla massa patrons
                    this.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);
                    action();
                });
            }, mainWindow.token);
        }
    }

    public class Glass { }
}