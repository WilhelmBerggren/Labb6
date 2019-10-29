using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Labb6
{
    public class Pub
    {
        public bool IsOpen { get; set; } = false;
        internal MainWindow mainWindow;

        internal ConcurrentQueue<Patron> WaitingPatrons { get; set; }
        internal ConcurrentStack<Glass> Shelf { get; set; }
        internal Dictionary<Patron, Glass> BarDisk { get; set; }
        internal ConcurrentStack<Glass> Table { get; set; }
        internal List<Patron> TakenChairs { get; set; }
        public Dictionary<string, double> Params { get; set; }

        private TaskFactory taskFactory;
        private bool badGuyBouncer;
        public bool BadGuyBouncer { get => badGuyBouncer; set => badGuyBouncer = value; }

        public Pub(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));

            this.Params = new Dictionary<string, double>()
            {
                { "BartenderGlassTiming", 3 },
                { "BartenderPourTiming", 3 },
                { "WaitressClearTiming", 10 },
                { "WaitressPlaceTiming", 15 },
                { "BouncerMinTiming", 3 },
                { "BouncerMaxTiming", 10 },
                { "PatronArriveTiming", 1 },
                { "PatronTableTiming", 4 },
                { "PatronMinDrinkTiming", 20 },
                { "PatronMaxDrinkTiming", 30 },
                { "NumberOfGlasses", 8 },
                { "NumberOfChairs", 9 }
            };

            IsOpen = false;
            Table = new ConcurrentStack<Glass>();
            WaitingPatrons = new ConcurrentQueue<Patron>();
            TakenChairs = new List<Patron>();
            BarDisk = new Dictionary<Patron, Glass>();
            Shelf = new ConcurrentStack<Glass>(Enumerable.Range(0, (int)Params["NumberOfGlasses"]).Select(i => new Glass()));
        }

        public void OpenTheBar()
        {
            IsOpen = true;
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
            mainWindow.LogEvent(text, listbox);
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