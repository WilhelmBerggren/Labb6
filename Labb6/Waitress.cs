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
            pub.RunAsTask(() =>
            {
                TakeEmptyGlasses();
                PlaceGlass();
            });
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
                pub.mainWindow.pauseWaitress.WaitOne(Timeout.Infinite);

                pub.Log("Goes to collect glasses...", LogBox.Waitress);
                Thread.Sleep(pub.Params["WaitressClearTiming"]);

            }
        }
        
        private void PlaceGlass()
        {
            if (glasses.Count > 0)
            {
                pub.mainWindow.pauseWaitress.WaitOne(Timeout.Infinite);

                pub.Log("Does dishes...", LogBox.Waitress);
                Thread.Sleep(pub.Params["WaitressPlaceTiming"]);
                while (glasses.Count > 0)
                {
                    pub.mainWindow.pauseWaitress.WaitOne(Timeout.Infinite);

                    pub.Shelf.Push(glasses.Pop());
                }
                pub.Log("Placed clean glasses in shelf...", LogBox.Waitress);
            }
        }
    }
}