using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Labb6
{
    static class NameGiver
    {
        static Random random = new Random();
        private static Queue<string> names = new Queue<string>(
            new[] { "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda", "William",
                "Elizabeth", "David", "Barbara", "Richard", "Susan", "Joseph", "Jessica", "Thomas",
                "Sarah", "Charles", "Karen", "Christopher", "Nancy", "Daniel", "Margaret", "Lisa" }
            .OrderBy(x => random.Next()).ToArray());

        public static string GetName()
        {
            string selectedName;
            lock (names)
            {
                selectedName = names.Dequeue();
                names.Enqueue(selectedName);
            }
            return selectedName;
        }
    }

    public class Patron
    {
        private Pub pub;
        private Glass glass;
        public string Name { get; internal set; }
        public bool Ready { get; internal set; }

        public Patron(Pub pub)
        {
            this.pub = pub;
            this.Name = NameGiver.GetName();
            PrintPatronInfo();
        }
        public void Run() {
            Console.WriteLine($"{Name} enqueued");
            Task.Run(() =>
            {
                Thread.Sleep((int)(pub.Options.PatronArriveTiming / pub.Options.Speed));
                pub.WaitingPatrons.Enqueue(this);

                pub.mainWindow.pauseBouncerAndPatrons.WaitOne();
                Ready = true;
                Console.WriteLine($"{Name} ready");
                pub.Log($"{Name} is waiting to be served", LogBox.Patron);
                
                WaitForGlass();
                WaitForTable();
                DrinkAndLeave();
            }, pub.mainWindow.token);
        }

        private void PrintPatronInfo()
        {
            if (this.Name == "Karen")
                pub.Log($"{Name} enters the pub.\nShe wants to speak to the manager!\n", LogBox.Patron);
            else
                pub.Log($"{Name} enters the pub", LogBox.Patron);
        }

        private void WaitForGlass()
        {
            while (true)
            {
                pub.mainWindow.pauseBouncerAndPatrons.WaitOne();
                if (pub.BarDisk.ContainsKey(this))
                {
                    lock (pub.PatronLock)
                    {
                        this.glass = pub.BarDisk[this];
                        pub.BarDisk.Remove(this);
                    }
                    pub.Log($"{Name} got a glass", LogBox.Patron);
                    return;
                }
                Thread.Sleep(50);
            }
        }

        private void WaitForTable()
        {
            pub.Log($"{Name} is waiting to be seated", LogBox.Patron);

            while (true)
            {
                if (pub.TakenChairs.Count < pub.Options.NumberOfChairs)
                {
                    lock (pub.PatronLock)
                    {
                        Thread.Sleep((int)(pub.Options.PatronTableTiming / pub.Options.Speed));
                        pub.mainWindow.pauseBouncerAndPatrons.WaitOne();
                        if(pub.TakenChairs.Count < pub.Options.NumberOfChairs)
                            pub.TakenChairs.Add(this);
                    }
                    pub.mainWindow.pauseBouncerAndPatrons.WaitOne();

                    pub.Log($"{Name} found a chair", LogBox.Patron);
                    return;
                }
                Thread.Sleep(50);
            }
        }

        private void DrinkAndLeave()
        {
            pub.Log($"{Name} enjoys the drink...", LogBox.Patron);
            int wait = new Random().Next((int)pub.Options.PatronMinDrinkTiming, (int)pub.Options.PatronMaxDrinkTiming);
            Thread.Sleep((int)(wait / pub.Options.Speed));
            pub.mainWindow.pauseBouncerAndPatrons.WaitOne();

            lock (pub.PatronLock)
            {
                pub.TakenChairs.Remove(this);
                pub.Table.Push(glass);
                pub.Log($"{Name} left", LogBox.Patron);
            }
        }
    }
}