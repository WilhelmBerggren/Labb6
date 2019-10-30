using System;
using System.Collections.Generic;
using System.Threading;

namespace Labb6
{
    public class Waitress
    {
        readonly Pub pub;
        readonly Stack<Glass> glasses;

        public Waitress(Pub pub)
        {
            glasses = new Stack<Glass>();
            this.pub = pub ?? throw new ArgumentNullException(nameof(pub));

            while (pub.Shelf.Count != pub.PubOptions.NumberOfGlasses || pub.TotalPresentPatrons > 0)
            {
                TakeEmptyGlasses();
                PlaceGlass();
            }
            WaitForPatronsToLeave();
        }

        private void WaitForPatronsToLeave()
        {
            //while (pub.WaitingPatrons.Count + pub.TakenChairs.Count > 0) { /*block*/ }
            pub.Log("Went home", LogBox.Waitress);
        }

        private void TakeEmptyGlasses()
        {
            while (!pub.Table.IsEmpty)
            {
                pub.mainWindow.pauseWaitress.WaitOne(Timeout.Infinite);

                if (pub.Table.TryPop(out Glass currentGlass))
                {
                    glasses.Push(currentGlass);
                }
            }
            if (glasses.Count != 0)
            {
                pub.Log("Goes to collect glasses...", LogBox.Waitress);
                Thread.Sleep((int)pub.PubOptions.WaitressClearTiming);
                pub.mainWindow.pauseWaitress.WaitOne();
            }
        }
        
        private void PlaceGlass()
        {
            if (glasses.Count > 0)
            {
                pub.Log("Does dishes...", LogBox.Waitress);

                Thread.Sleep((int)pub.PubOptions.WaitressPlaceTiming);
                pub.mainWindow.pauseWaitress.WaitOne();
                while (glasses.Count > 0)
                {
                    pub.Shelf.Push(glasses.Pop());
                }
                pub.Log("Placed clean glasses in shelf...", LogBox.Waitress);
            }
        }
    }
}