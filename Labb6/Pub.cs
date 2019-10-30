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
        public int TotalPresentPatrons { get; internal set; }
        public bool WaitressIsPresent { get; set; } = true;
        public bool BartenderIsPresent { get; set; } = true;

        public PubOptions PubOptions;

        public Pub(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));

            this.PubOptions = new PubOptions();

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
                while (BartenderIsPresent && WaitressIsPresent)
                {
                    Thread.Sleep(1000);
                    Log($"Patrons present: {TotalPresentPatrons}. Drinking patrons: {TakenChairs.Count}. Waiting Patrons: {TotalPresentPatrons - TakenChairs.Count}\n" +
                        $"\tAvailable chairs: {PubOptions.MaxNumberOfChairs - TakenChairs.Count}. Available Glasses: {Shelf.Count}\n", LogBox.Event);
                };
            }, mainWindow.token);
        }
    }
}