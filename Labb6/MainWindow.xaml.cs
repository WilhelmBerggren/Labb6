using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Labb6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public enum LogBox { Event, Bartender, Waitress, Patron }
    public enum BarState { Open, Close }
    public partial class MainWindow : Window
    {
        internal ManualResetEvent pauseBouncerAndPatrons; // starts out in a signaled state, meaning it does not block by default.
        internal ManualResetEvent pauseBartender;
        internal ManualResetEvent pauseWaitress;
        internal CancellationTokenSource tokenSource;
        internal CancellationToken token;

        DispatcherTimer timer;
        private bool SelectionIsMade = false;
        public int BarOpenForDuration { get; set; } = 120; // given in seconds. default value == 120 sec (2min)

        private Pub pub;

        public MainWindow()
        {
            timer = new DispatcherTimer();
            InitializeComponent();
            pauseBouncerAndPatrons = new ManualResetEvent(true);
            pauseBartender = new ManualResetEvent(true);
            pauseWaitress = new ManualResetEvent(true);
        }

        private void TimerInitialization()
        {
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += OnTimer_Tick;
        }

        private void OnTimer_Tick(object sender, EventArgs e)
        {
            if (BarOpenForDuration == 0)
            {
                timer.Stop();
                pub.IsOpen = false;
                timerLabel.Content = "00:00";
            }
            else
            {
                DateTime TimerStartTime = DateTime.Now;
                timerLabel.Content = (TimeSpan.FromSeconds(BarOpenForDuration) - (DateTime.Now - TimerStartTime)).ToString("mm\\:ss");
                BarOpenForDuration--;
            }
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
                        SetBarState(BarState.Open);
                        break;
                    case "20 Glasses, 3 chairs":
                        pub = new Pub(this);
                        pub.PubOptions.NumberOfGlasses = 20;
                        pub.PubOptions.NumberOfChairs = 3;
                        SetBarState(BarState.Open);
                        break;
                    case "20 Chairs, 5 Glasses":
                        pub = new Pub(this);
                        pub.PubOptions.NumberOfChairs = 20;
                        pub.PubOptions.NumberOfGlasses = 5;
                        SetBarState(BarState.Open);
                        break;
                    case "Double Stay (Patrons)":
                        pub = new Pub(this);
                        pub.PubOptions.PatronArriveTiming = 2000;
                        pub.PubOptions.PatronTableTiming = 8000;
                        pub.PubOptions.PatronMinDrinkTiming = 20000;
                        pub.PubOptions.PatronMaxDrinkTiming = 40000;
                        SetBarState(BarState.Open);
                        break;
                    case "Double Speed Waitress":
                        pub = new Pub(this);
                        pub.PubOptions.WaitressClearTiming = 5000;
                        pub.PubOptions.WaitressPlaceTiming = 7500;
                        SetBarState(BarState.Open);
                        break;
                    //case "5 Minutes open":
                    //  Not yet implemented countdown.
                    //   break;
                    //case "Couples night":
                    // Not yet implemented. Bouncer creates two instances of patrons per turn.
                    //   break;
                    case "Bouncer is a jerk":
                        pub = new Pub(this);
                        pub.PubOptions.BouncerMinTiming = 6000;
                        pub.PubOptions.BouncerMaxTiming = 20000;
                        pub.PubOptions.BadGuyBouncer = true;
                        SetBarState(BarState.Open);
                    break;
                    default:
                        break;
                }
            }
            else
                SetBarState(BarState.Close);
        }

        private void SetBarState(BarState newState)
        {
            if (pub == null)
                return;

            if (newState == BarState.Close)
            {
                //if (PatronsAreStillPresentInBar)
                //{
                //    Disable"OpenBar" button until bar is empty.
                //}
                //else (if bar is empty...)
                //{
                //    Let it roll...
                //}
                timer.Stop();
                pub.CloseTheBar();
                timer = new DispatcherTimer();
                SelectionIsMade = false;
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
                TimerInitialization();
                PatronListBox.Items.Clear();
                BartenderListBox.Items.Clear();
                WaitressListBox.Items.Clear();
                EventListBox.Items.Clear();
                tokenSource = new CancellationTokenSource();
                token = tokenSource.Token;
                pub.OpenTheBar();
                timer.Start();
                SelectionIsMade = true;
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
                SetBarState(BarState.Close);
                PanicButton.Content = "Phew! Crisis averted... :-)";
            }
            else
            {
                SetBarState(BarState.Open);
                PanicButton.Content = "Panic! Pause all threads!";
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (pub != null)
            {
                this.pub.PubOptions.Speed = SpeedSlider.Value;
                SpeedLabel.Content = Math.Round(SpeedSlider.Value, 1);
            }
        }

        public void LogEvent(string text, LogBox textblock)
        {
            Console.WriteLine(textblock + ": " + text);
            switch (textblock)
            {
                case LogBox.Event:
                    this.Dispatcher.Invoke(() => EventListBox.Items.Insert(0, text));
                    break;
                case LogBox.Bartender:
                    this.Dispatcher.Invoke(() => BartenderListBox.Items.Insert(0, text ));
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

    public class Glass { }
}