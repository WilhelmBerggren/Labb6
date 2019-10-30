using System;
using System.Threading;
using System.Threading.Tasks;
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

        //DispatcherTimer timer;
        private bool SelectionIsMade = false;
        public int BarOpenForDuration { get; set; } = 120; // given in seconds. default value == 120 sec (2min)

        private Pub pub;

        public MainWindow()
        {
            InitializeComponent();
            pauseBouncerAndPatrons = new ManualResetEvent(true);
            pauseBartender = new ManualResetEvent(true);
            pauseWaitress = new ManualResetEvent(true);
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
                        pub.Options.NumberOfGlasses = 20;
                        pub.Options.NumberOfChairs = 3;
                        SetBarState(BarState.Open);
                        break;
                    case "20 Chairs, 5 Glasses":
                        pub = new Pub(this);
                        pub.Options.NumberOfChairs = 20;
                        pub.Options.NumberOfGlasses = 5;
                        SetBarState(BarState.Open);
                        break;
                    case "Double Stay (Patrons)":
                        pub = new Pub(this);
                        pub.Options.PatronArriveTiming = 2000;
                        pub.Options.PatronTableTiming = 8000;
                        pub.Options.PatronMinDrinkTiming = 20000;
                        pub.Options.PatronMaxDrinkTiming = 40000;
                        SetBarState(BarState.Open);
                        break;
                    case "Double Speed Waitress":
                        pub = new Pub(this);
                        pub.Options.WaitressClearTiming = 5000;
                        pub.Options.WaitressPlaceTiming = 7500;
                        SetBarState(BarState.Open);
                        break;
                    case "5 Minutes open":
                        pub.TimeUntilClosing = 300;
                        break;
                    case "Couples night":
                        pub.Options.CouplesNight = true;
                        break;
                    case "Bouncer is a jerk":
                        pub = new Pub(this);
                        pub.Options.BouncerMinTiming = 6000;
                        pub.Options.BouncerMaxTiming = 20000;
                        pub.Options.BadGuyBouncer = true;
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
                BarOpenForDuration = 120;
                pub.Close();

                Task.Run(() =>
                {
                    while (pub.TotalPresentPatrons > 0)
                        Dispatcher.Invoke(() => { ToggleBarOpenButton.IsEnabled = false; });

                });

                ToggleBarOpenButton.IsEnabled = true;
                SpeedSlider.Value = 1;
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
                SpeedSlider.Value = 1;
                PatronListBox.Items.Clear();
                BartenderListBox.Items.Clear();
                WaitressListBox.Items.Clear();
                EventListBox.Items.Clear();
                tokenSource = new CancellationTokenSource();
                token = tokenSource.Token;
                pub.Open();
                SelectionIsMade = true;
                ToggleBarOpenButton.Content = "Close bar";
                LogEvent("TestCase: " + TestCase.SelectedValue.ToString().Substring(38), LogBox.Event);
                Pause_GuestsButton.IsEnabled = true;
                Pause_BartenderButton.IsEnabled = true;
                Pause_WaitressButton.IsEnabled = true;
                TestCase.IsEnabled = false;
            }
        }

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
                this.pub.Options.Speed = SpeedSlider.Value;
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
        public void PrintTime(string text)
        {
            this.Dispatcher.Invoke(() => timerLabel.Content = text);
        }
    }

    public class Glass { }
}