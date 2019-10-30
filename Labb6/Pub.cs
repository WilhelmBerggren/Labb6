﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Labb6
{
    public class PubOptions
    {
        private double _BartenderGlassTiming;
        private double _BartenderPourTiming;
        private double _WaitressClearTiming;
        private double _WaitressPlaceTiming;
        private double _BouncerMinTiming;
        private double _BouncerMaxTiming;
        private double _PatronArriveTiming;
        private double _PatronTableTiming;
        private double _PatronMinDrinkTiming;
        private double _PatronMaxDrinkTiming;

        public double BartenderGlassTiming { get { return _BartenderGlassTiming/Speed; } set { _BartenderGlassTiming = value; } }
        public double BartenderPourTiming { get { return _BartenderPourTiming/Speed; } set { _BartenderPourTiming = value; } }
        public double WaitressClearTiming { get { return _WaitressClearTiming/Speed; } set { _WaitressClearTiming = value; } }
        public double WaitressPlaceTiming { get { return _WaitressPlaceTiming/Speed; } set { _WaitressPlaceTiming = value; } }
        public double BouncerMinTiming { get { return _BouncerMinTiming/Speed; } set { _BouncerMinTiming = value; } }
        public double BouncerMaxTiming { get { return _BouncerMaxTiming/Speed; } set { _BouncerMaxTiming = value; } }
        public double PatronArriveTiming { get { return _PatronArriveTiming/Speed; } set { _PatronArriveTiming = value; } }
        public double PatronTableTiming { get { return _PatronTableTiming/Speed; } set { _PatronTableTiming = value; } }
        public double PatronMinDrinkTiming { get { return _PatronMinDrinkTiming/Speed; } set { _PatronMinDrinkTiming = value; } }
        public double PatronMaxDrinkTiming { get { return _PatronMaxDrinkTiming/Speed; } set { _PatronMaxDrinkTiming = value; } }
        public double NumberOfGlasses { get; internal set; }
        public double NumberOfChairs { get; internal set; }

        public int BadGuyBouncer { get; internal set; }
        public double Speed { get; internal set; }
    }

    public class Pub
    {
        public bool IsOpen { get; set; } = false;
        internal MainWindow mainWindow;

        internal ConcurrentQueue<Patron> WaitingPatrons { get; set; }
        internal ConcurrentStack<Glass> Shelf { get; set; }
        internal Dictionary<Patron, Glass> BarDisk { get; set; }
        internal ConcurrentStack<Glass> Table { get; set; }
        internal List<Patron> TakenChairs { get; set; }
        public PubOptions PubOptions;

        public Pub(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));

            this.PubOptions = new PubOptions()
            {
                BartenderGlassTiming = 3,
                BartenderPourTiming = 3,
                WaitressClearTiming = 10,
                WaitressPlaceTiming = 15,
                BouncerMinTiming = 3,
                BouncerMaxTiming = 10,
                PatronArriveTiming = 1,
                PatronTableTiming = 4,
                PatronMinDrinkTiming = 20,
                PatronMaxDrinkTiming = 30,
                NumberOfGlasses = 8,
                NumberOfChairs = 9,
                BadGuyBouncer = 0,
                Speed = 1
            };

            IsOpen = false;
        }

        public void OpenTheBar()
        {
            IsOpen = true;
            Table = new ConcurrentStack<Glass>();
            WaitingPatrons = new ConcurrentQueue<Patron>();
            TakenChairs = new List<Patron>();
            BarDisk = new Dictionary<Patron, Glass>();
            Shelf = new ConcurrentStack<Glass>(Enumerable.Range(0, (int)PubOptions.NumberOfGlasses).Select(i => new Glass()));
            InfoPrinter();
            Task.Run(() => new Bouncer(this), mainWindow.token);
            Task.Run(() => new Bartender(this), mainWindow.token);
            Task.Run(() => new Waitress(this), mainWindow.token);
        }

        public void CloseTheBar()
        {
            IsOpen = false;
            mainWindow.tokenSource.Cancel();
        }

        public void Log(string text, LogBox listbox)
        {
            string timeStamp = DateTime.Now.ToString("T");
            string addSpace = " ";
            mainWindow.LogEvent(timeStamp + addSpace + text, listbox);
        }

        private void InfoPrinter()
        {
            Task.Run(() =>
            {
                while(IsOpen)
                {
                    if (mainWindow.token.IsCancellationRequested)
                        return;

                    Log($"Taken chairs: {TakenChairs.Count}, Waiting Patrons: {WaitingPatrons.Count}, Glasses: {Shelf.Count}", LogBox.Event);
                    Thread.Sleep(1000);
                };
            }, mainWindow.token);
        }

        public void RunAsTask(Action action)
        {
            Task.Run(() => action(), mainWindow.token);
        }
    }
}