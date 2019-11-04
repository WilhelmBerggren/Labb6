using System;
using System.Threading;
using System.Threading.Tasks;

namespace Labb6
{
    public class Bartender
    {
        Pub pub;
        Glass currentGlass;
        Patron currentPatron;
        public Bartender(Pub pub)
        {
            this.pub = pub;
        }

        public void Run() {
            pub.Log("Arrived", LogBox.Bartender);

            Task.Run(() => 
            { 
                while (pub.IsOpen || pub.TotalPresentPatrons > 0)
                {
                    if (currentPatron == null)
                        currentPatron = WaitForPatron();
                    else
                    {
                        currentGlass = WaitForGlass();
                        if (currentGlass != null)
                        {
                            pub.Log("Got glass and pouring beer...", LogBox.Bartender);
                            Thread.Sleep((int)(pub.Options.BartenderPourTiming / pub.Options.Speed));
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
            }, pub.mainWindow.token);
        }

        private void WaitForPatronsAndLoveInterestWaitress()
        {
            pub.Log("Waiting for patrons to leave", LogBox.Waitress);
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
                    Thread.Sleep((int)(pub.Options.BartenderGlassTiming / pub.Options.Speed));
                    pub.mainWindow.pauseBartender.WaitOne();

                    pub.Shelf.TryPop(out Glass glass);
                    return glass;
                }
            }
            return null;
        }
    }
}