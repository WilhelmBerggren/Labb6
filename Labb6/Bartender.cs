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
            Pub.WhileOpen(pub, () =>
            {
                currentPatron = WaitForPatron();
                currentGlass = WaitForGlass();

                pub.Log("Pouring beer...", LogBox.Bartender);
                Pub.Sleep(pub.Params["BartenderPourTiming"], pub.mainWindow.pauseBartender);

                lock (pub.BarDisk)
                {
                    pub.BarDisk.Add(currentPatron, currentGlass);
                }
            });
        }

        private Patron WaitForPatron()
        {
            while (true)
            {
                pub.mainWindow.pauseBartender.WaitOne();

                if (pub.WaitingPatrons.TryDequeue(out Patron patron))
                {
                    return patron;
                }
            }
        }

        private Glass WaitForGlass()
        {
            while (true)
            {
                pub.mainWindow.pauseBartender.WaitOne();
                if (pub.Shelf.TryPeek(out _))
                {
                    pub.Log("Collecting glass...", LogBox.Bartender);
                    Pub.Sleep(pub.Params["BartenderGlassTiming"], pub.mainWindow.pauseBartender);

                    pub.Shelf.TryPop(out Glass glass);
                    return glass;
                }
            }
        }
    }
}