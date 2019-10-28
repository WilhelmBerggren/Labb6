using System;
using System.Threading;

namespace Labb6
{
    public class Bartender
    {
        Pub pub;
        Glass currentGlass;
        Patron currentPatron;
        public Bartender(Pub pub)
        {
            this.pub = pub ?? throw new ArgumentNullException(nameof(pub));
            pub.RunAsTask(() =>
            {
                pub.mainWindow.token.ThrowIfCancellationRequested();
                pub.mainWindow.pauseBartender.WaitOne(Timeout.Infinite);

                currentPatron = WaitForPatron();
                currentGlass = WaitForGlass();
                pub.mainWindow.pauseBartender.WaitOne(Timeout.Infinite);

                pub.Log("Pouring beer...", LogBox.Bartender);
                Thread.Sleep(pub.Params["BartenderPourTiming"]);

                lock (pub.BarDisk)
                {
                    pub.BarDisk.Add(currentPatron, currentGlass);
                }
            });
        }
        Patron WaitForPatron()
        {
            while (true)
            {
                pub.mainWindow.pauseBartender.WaitOne(Timeout.Infinite);

                if (pub.WaitingPatrons.TryDequeue(out Patron patron))
                {
                    return patron;
                }
            }
        }

        Glass WaitForGlass()
        {
            while (true)
            {
                pub.mainWindow.pauseBartender.WaitOne(Timeout.Infinite);
                if (pub.Shelf.TryPeek(out _))
                {

                    pub.Log("Collecting glass...", LogBox.Bartender);
                    Thread.Sleep(pub.Params["BartenderGlassTiming"]);
                    pub.mainWindow.pauseBartender.WaitOne(Timeout.Infinite);

                    pub.Shelf.TryPop(out Glass glass);
                    return glass;
                }
            }
        }
    }
}