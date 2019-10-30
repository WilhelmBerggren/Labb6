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
        public PubOptions Options;
        public int TimeUntilClosing { get; set; }

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
            Task.Run(() => new Bouncer(this), mainWindow.token);
            Task.Run(() => new Bartender(this), mainWindow.token);
            Task.Run(() => new Waitress(this), mainWindow.token);
        }

        public void Close()
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

                    Thread.Sleep(1000);
                    Log($"Taken chairs: {TakenChairs.Count}, Waiting Patrons: {WaitingPatrons.Count}, Glasses: {Shelf.Count}", LogBox.Event);
                };
            }, mainWindow.token);
        }

        private void CountDown()
        {
            Task.Run(() => 
            {
                while(this.IsOpen && TimeUntilClosing > 0)
                {
                    if (mainWindow.token.IsCancellationRequested)
                        return;

                    Thread.Sleep((int)(1000/Options.Speed));
                    this.TimeUntilClosing--;
                    string time = $"{TimeUntilClosing / 60}:{TimeUntilClosing % 60}";
                    mainWindow.PrintTime(time);
                    Console.WriteLine(TimeUntilClosing + ", " + time);
                }
                Close();
            }, mainWindow.token);
        }
    }
}