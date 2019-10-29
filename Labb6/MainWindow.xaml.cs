﻿using System.Threading;
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
                        pub.Params["PatronArriveTiming"] = 2;
                        pub.Params["PatronTableTiming"] = 8;
                        pub.Params["PatronMinDrinkTiming"] = 40;
                        pub.Params["PatronMaxDrinkTiming"] = 60;
                        OpenOrCloseBar();
                        break;
                    case "Double Speed Waitress":
                        pub = new Pub(this);
                        pub.Params["WaitressClearTiming"] = 5;
                        pub.Params["WaitressPlaceTiming"] = 7.5;
                        OpenOrCloseBar();
                        break;
                    //case "5 Minutes open":
                    //  Not yet implemented countdown.
                    //   break;
                    //case "Couples night":
                    // Not yet implemented. Bouncer creates two instances of patrons per turn.
                    //   break;
                    case "Bouncer is a jerk":
                        pub = new Pub(this);
                        pub.Params["BouncerMinTiming"] = 6;
                        pub.Params["BouncerMaxTiming"] = 20;
                        pub.BadGuyBouncer = true;
                        OpenOrCloseBar();
                    break;
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
                //if (PatronsAreStillPresentInBar)
                //{
                //    Disable"OpenBar" button until bar is empty.
                //}
                //else (if bar is empty...)
                //{
                //    Let it roll...
                //}
                pub.CloseTheBar();
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
                PatronListBox.Items.Clear();
                BartenderListBox.Items.Clear();
                WaitressListBox.Items.Clear();
                EventListBox.Items.Clear();
                tokenSource = new CancellationTokenSource();
                token = tokenSource.Token;
                pub.OpenTheBar();
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

    public class Glass { }
}