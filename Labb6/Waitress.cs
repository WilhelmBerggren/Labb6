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

            Pub.WhileOpen(pub, () =>
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
                pub.Log("Goes to collect glasses...", LogBox.Waitress);
                Pub.Sleep(pub.Params["WaitressClearTiming"], pub.mainWindow.pauseWaitress);
            }
        }
        
        private void PlaceGlass()
        {
            if (glasses.Count > 0)
            {
                pub.Log("Does dishes...", LogBox.Waitress);

                Pub.Sleep(pub.Params["WaitressPlaceTiming"], pub.mainWindow.pauseWaitress);
                while (glasses.Count > 0)
                {
                    pub.Shelf.Push(glasses.Pop());
                }
                pub.Log("Placed clean glasses in shelf...", LogBox.Waitress);
            }
        }
    }
}