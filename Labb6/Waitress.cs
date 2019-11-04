using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Labb6
{
    public class Waitress
    {
        readonly Pub pub;
        readonly Stack<Glass> glasses;

        public Waitress(Pub pub)
        {
            glasses = new Stack<Glass>();
            this.pub = pub;
        }

        public void Run() {
            pub.Log("Arrived", LogBox.Waitress);
            Task.Run(() =>
            {
                while (pub.IsOpen || pub.Shelf.Count != pub.Options.NumberOfGlasses || pub.TotalPresentPatrons > 0)
                {
                    TakeEmptyGlasses();
                    PlaceGlass();
                }
                pub.Log("Left the bar with her best friend, the bartender.\n", LogBox.Waitress);
                pub.WaitressIsPresent = false;
            }, pub.mainWindow.token);
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
                Thread.Sleep((int)(pub.Options.WaitressClearTiming / pub.Options.Speed));
                pub.mainWindow.pauseWaitress.WaitOne();
            }
        }

        private void PlaceGlass()
        {
            if (glasses.Count > 0)
            {
                pub.Log("Does dishes...", LogBox.Waitress);

                Thread.Sleep((int)(pub.Options.WaitressPlaceTiming / pub.Options.Speed));
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