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

        public void TryWhile(bool cond, Action action)
        {
            try
            {
                while (cond)
                {
                    this.mainWindow.token.ThrowIfCancellationRequested();
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
                    // Ligger här för att också stoppa denna task för att köra om och om igen, och sedan spotta ut en jäkla massa patrons
                    action();
                });
            }, mainWindow.token);
        }
    }
}