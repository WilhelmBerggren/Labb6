using System;
using System.Linq;
using System.Threading;

namespace Labb6
{

    static class Name {
        private static string[] nameArray = new string[25]
        {
            "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda", "William",
            "Elizabeth", "David", "Barbara", "Richard", "Susan","Joseph", "Jessica", "Thomas", "Sarah",
            "Charles", "Karen", "Christopher", "Nancy", "Daniel", "Margaret", "Lisa"
        };
        public static string GetName()
        {
            int selectedName = new Random().Next(0, nameArray.Length);
            return nameArray[selectedName];
        }
    }
    public class Patron
    {
        private Pub pub;
        private Glass glass;
        private string patronName;
        public Patron(Pub pub)
        {
            this.pub = pub ?? throw new ArgumentNullException(nameof(pub));
            this.patronName = Name.GetName();

            PrintPatronInfo();
            Thread.Sleep(pub.Params["PatronArriveTiming"]);
            pub.WaitingPatrons.Enqueue(this);
            pub.Log("Number of Waiting Patrons: " + pub.WaitingPatrons.Count, LogBox.Waitress);

            WaitForGlass();
            WaitForTable();
            DrinkAndLeave();
        }

        private void PrintPatronInfo()
        {
            if (this.patronName == "Karen")
                pub.Log($"{patronName} enters the pub. She wants to speak to the manager!", LogBox.Patron);
            else
                pub.Log($"{patronName} enters the pub", LogBox.Patron);
        }

        private void WaitForGlass()
        {
            try
            {
                while (true)
                {
                    pub.mainWindow.token.ThrowIfCancellationRequested();
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
            catch (OperationCanceledException) { }
        }

        private void WaitForTable()
        {
            bool foundTable = false;
            try
            {
                while (!foundTable)
                {
                    pub.mainWindow.token.ThrowIfCancellationRequested();

                    if (pub.TakenChairs.Count < pub.Params["NumberOfChairs"])
                    {
                        pub.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);

                        Thread.Sleep(pub.Params["PatronTableTiming"]);
                        lock (pub.TakenChairs)
                        {
                            pub.TakenChairs.Add(this);
                        }
                        pub.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);

                        pub.Log($"{patronName} found a chair", LogBox.Patron);
                        return;
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        private void DrinkAndLeave()
        {
            pub.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);

            Thread.Sleep(new Random().Next(pub.Params["PatronMinDrinkTiming"], pub.Params["PatronMaxDrinkTiming"]));
            lock (pub.TakenChairs)
            {
                pub.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);

                pub.TakenChairs.Remove(this);
                pub.Table.Push(glass);
                pub.Log($"{patronName} left", LogBox.Patron);
            }
        }
    }
}