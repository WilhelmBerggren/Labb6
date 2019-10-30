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
        public double BartenderGlassTiming;
        public double BartenderPourTiming;
        public double WaitressClearTiming;
        public double WaitressPlaceTiming;
        public double BouncerMinTiming;
        public double BouncerMaxTiming;
        public double PatronArriveTiming;
        public double PatronTableTiming;
        public double PatronMinDrinkTiming;
        public double PatronMaxDrinkTiming;
        public double NumberOfGlasses;
        public double NumberOfChairs;

        public int BadGuyBouncer { get; internal set; }
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
                BadGuyBouncer = 0
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
            RunAsTask(() => _ = new Bouncer(this));
            RunAsTask(() => _ = new Bartender(this));
            RunAsTask(() => _ = new Waitress(this));
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
            RunAsTask(() =>
            {
                Pub.WhileOpen(this, () =>
                {
                    if (mainWindow.token.IsCancellationRequested)
                        return;

                    Log($"Taken chairs: {TakenChairs.Count}, Waiting Patrons: {WaitingPatrons.Count}, Glasses: {Shelf.Count}", LogBox.Event);
                    Thread.Sleep(1000);
                });
            });
        }

        public static void WhileOpen(Pub pub, Action action)
        {
            while (pub.IsOpen)
            {
                if (pub.mainWindow.token.IsCancellationRequested)
                    return;

                action();
            }
        }

        public void RunAsTask(Action action)
        {
            Task.Run(() => action(), mainWindow.token);
        }

        public static void Sleep(double seconds, ManualResetEvent manualResetEvent)
        {
            if (manualResetEvent == null)
                throw new ArgumentNullException(nameof(manualResetEvent));

            for (int i = 0; i < seconds; i++)
            {
                manualResetEvent.WaitOne();
                Thread.Sleep(1000);
            }
        }
    }
}