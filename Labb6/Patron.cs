﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Labb6
{
    static class Name {
        private static Queue<string> names = new Queue<string>(new[] { "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda", "William", "Elizabeth", "David", "Barbara", "Richard", "Susan", "Joseph", "Jessica", "Thomas", "Sarah", "Charles", "Karen", "Christopher", "Nancy", "Daniel", "Margaret", "Lisa" });
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
            this.pub = pub ?? throw new ArgumentNullException(nameof(pub));
            this.patronName = Name.GetName();

            PrintPatronInfo();
            Pub.Sleep(pub.Params["PatronArriveTiming"], pub.mainWindow.pauseBouncerAndPatrons);
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
                if (pub.TakenChairs.Count < pub.Params["NumberOfChairs"])
                {

                    Pub.Sleep(pub.Params["PatronTableTiming"], pub.mainWindow.pauseBouncerAndPatrons);

                    lock (pub.TakenChairs)
                    {
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

            Pub.Sleep(new Random().Next(pub.Params["PatronMinDrinkTiming"], pub.Params["PatronMaxDrinkTiming"]), 
                pub.mainWindow.pauseBouncerAndPatrons);

            lock (pub.TakenChairs)
            {
                pub.TakenChairs.Remove(this);
                pub.Table.Push(glass);
                pub.Log($"{patronName} left", LogBox.Patron);
            }
        }
    }
}