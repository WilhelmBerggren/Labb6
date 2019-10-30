using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Labb6
{
    static class Name
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
        private string patronName;

        public Patron(Pub pub)
        {
            this.pub = pub;
            this.patronName = Name.GetName();

            PrintPatronInfo();
            //Pub.Sleep(pub.PubOptions.PatronArriveTiming, pub.mainWindow.pauseBouncerAndPatrons);
            Thread.Sleep((int)pub.PubOptions.PatronArriveTiming);
            pub.mainWindow.pauseBouncerAndPatrons.WaitOne();
            pub.WaitingPatrons.Enqueue(this);
            pub.Log("Number of Waiting Patrons: " + pub.WaitingPatrons.Count, LogBox.Waitress);

            WaitForGlass();
            WaitForTable();
            DrinkAndLeave();
        }

        private void PrintPatronInfo()
        {
            if (this.patronName == "Karen")
                pub.Log($"{patronName} enters the pub.\nShe wants to speak to the manager!", LogBox.Patron);
            else
                pub.Log($"{patronName} enters the pub", LogBox.Patron);
        }

        private void WaitForGlass()
        {
            while (true)
            {
                pub.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);
                if (pub.BarDisk.ContainsKey(this))
                {
                    lock (pub.BarDisk)
                    {
                        this.glass = pub.BarDisk[this];
                        pub.BarDisk.Remove(this);
                    }
                    pub.Log($"{patronName} got a glass", LogBox.Patron);
                    return;
                }
            }
        }

        private void WaitForTable()
        {
            while (true)
            {
                if (pub.TakenChairs.Count < pub.PubOptions.NumberOfChairs)
                {
                    lock (pub.TakenChairs)
                    {
                        Thread.Sleep((int)pub.PubOptions.PatronTableTiming);
                        pub.mainWindow.pauseBouncerAndPatrons.WaitOne();
                        if(pub.TakenChairs.Count < pub.PubOptions.NumberOfChairs)
                            pub.TakenChairs.Add(this);
                    }
                    pub.mainWindow.pauseBouncerAndPatrons.WaitOne();

                    pub.Log($"{patronName} found a chair", LogBox.Patron);
                    return;
                }
            }
        }

        private void DrinkAndLeave()
        {
            Thread.Sleep(new Random().Next((int)pub.PubOptions.PatronMinDrinkTiming, (int)pub.PubOptions.PatronMaxDrinkTiming));
            pub.mainWindow.pauseBouncerAndPatrons.WaitOne();

            lock (pub.TakenChairs)
            {
                pub.TakenChairs.Remove(this);
                pub.Table.Push(glass);
                pub.Log($"{patronName} left", LogBox.Patron);
            }
        }
    }
}