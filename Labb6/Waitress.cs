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

            while (pub.IsOpen || pub.Shelf.Count != pub.Options.NumberOfGlasses && pub.TotalPresentPatrons > 0)
            {
                TakeEmptyGlasses();
                PlaceGlass();
            }
            WaitForPatronsAndFriendzonedBartender();
        }

        private void WaitForPatronsAndFriendzonedBartender()
        {
            while (pub.TotalPresentPatrons > 0) { }
            pub.Log("Left the bar with her best friend, the bartender.\n", LogBox.Waitress);
            pub.WaitressIsPresent = false;
            pub.CloseTheBar();
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
                Thread.Sleep((int)pub.Options.WaitressClearTiming);
                pub.mainWindow.pauseWaitress.WaitOne();
            }
        }

        private void PlaceGlass()
        {
            if (glasses.Count > 0)
            {
                pub.Log("Does dishes...", LogBox.Waitress);

                Thread.Sleep((int)pub.Options.WaitressPlaceTiming);
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