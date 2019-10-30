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
            while (pub.IsOpen || pub.WaitingPatrons.Count > 0)
            {
                if (currentPatron == null)
                    currentPatron = WaitForPatron();
                if (currentPatron != null)
                {
                    currentGlass = WaitForGlass();
                    if (currentGlass != null)
                    {
                        pub.Log("Got glass and pouring beer...", LogBox.Bartender);
                        Thread.Sleep((int)pub.PubOptions.BartenderPourTiming);
                        pub.mainWindow.pauseBartender.WaitOne();
                        lock (pub.BarDisk)
                        {
                            pub.BarDisk.Add(currentPatron, currentGlass);
                            currentPatron = null;
                            currentGlass = null;
                        }
                    }
                }

            }
            WaitForPatronsAndLoveInterestWaitress();
        }

        private void WaitForPatronsAndLoveInterestWaitress()
        {
            while (pub.TotalPresentPatrons > 0 || pub.WaitressIsPresent) { }
            pub.Log("Left the bar together with the woman of his dreams, the Waitress...\n", LogBox.Bartender);
            pub.BartenderIsPresent = false;
        }

        private Patron WaitForPatron()
        {
            while (pub.WaitingPatrons.Count > 0)
            {
                pub.mainWindow.pauseBartender.WaitOne();

                if (pub.WaitingPatrons.TryDequeue(out Patron patron))
                {
                    return patron;
                }
            }
            return null;
        }

        private Glass WaitForGlass()
        {
            while (pub.WaitingPatrons.Count >= 0)
            {
                pub.mainWindow.pauseBartender.WaitOne();
                if (pub.Shelf.TryPeek(out _))
                {
                    pub.Log("Collecting glass...", LogBox.Bartender);
                    Thread.Sleep((int)pub.PubOptions.BartenderGlassTiming);
                    pub.mainWindow.pauseBartender.WaitOne();

                    pub.Shelf.TryPop(out Glass glass);
                    return glass;
                }
            }
            return null;
        }
    }
}