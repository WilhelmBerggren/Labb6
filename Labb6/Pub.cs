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
        internal MainWindow mainWindow { get; set; }
        internal ConcurrentQueue<Patron> WaitingPatrons { get; set; }
        internal ConcurrentStack<Glass> Shelf { get; set; }
        internal Dictionary<Patron, Glass> BarDisk { get; set; }
        internal ConcurrentStack<Glass> Table { get; set; }
        internal List<Patron> TakenChairs { get; set; }
        public bool WaitressIsPresent { get; set; } = true;
        public bool BartenderIsPresent { get; set; } = true;
        public PubOptions Options;
        public int TimeUntilClosing { get; set; }
        internal object PatronLock = new object();

        public Pub(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));

            Options = new PubOptions();
            TimeUntilClosing = 120;

            IsOpen = false;
        }

        public void Open()
        {
            IsOpen = true;
            Table = new ConcurrentStack<Glass>();
            WaitingPatrons = new ConcurrentQueue<Patron>();
            TakenChairs = new List<Patron>();
            BarDisk = new Dictionary<Patron, Glass>();
            Shelf = new ConcurrentStack<Glass>(Enumerable.Range(0, (int)Options.NumberOfGlasses).Select(i => new Glass()));
            InfoPrinter();
            CountDown();

            new Bouncer(this).Run();
            new Bartender(this).Run();
            new Waitress(this).Run();
        }

        public void Close()
        {
            IsOpen = false;
            mainWindow.tokenSource.Cancel();
        }

        public void Log(string text, LogBox listbox)
        {
            string timeStamp = DateTime.Now.ToString("T");
            mainWindow.LogEvent($"{timeStamp} {text}", listbox);
        }

        private void InfoPrinter()
        {
            Task.Run(() =>
            {
                while (BartenderIsPresent && WaitressIsPresent)
                {
                    Thread.Sleep((int)(1000 / Options.Speed));
                    Log($"Patrons present: {(WaitingPatrons.Count + TakenChairs.Count)}. Drinking patrons: {TakenChairs.Count}. Waiting Patrons: {WaitingPatrons.Count}\n" +
                        $"\tAvailable chairs: {Options.NumberOfChairs - TakenChairs.Count}. Available Glasses: {Shelf.Count}\n", LogBox.Event);
                };
            }, mainWindow.token);
        }

        private void CountDown()
        {
            Task.Run(() => 
            {
                while(this.IsOpen && TimeUntilClosing > 0)
                {
                    Thread.Sleep((int)(1000 / Options.Speed));
                    this.TimeUntilClosing--;
                    string minutes = $"{TimeUntilClosing / 60}".PadLeft(2, '0');
                    string seconds = $"{TimeUntilClosing % 60}".PadLeft(2, '0');
                    string time = $"{minutes}:{seconds}";
                    mainWindow.PrintTime(time);
                }
                mainWindow.Dispatcher.Invoke(() =>
                {
                    mainWindow.SetBarState(BarState.Close);
                });
            }, mainWindow.token);
        }
    }
}