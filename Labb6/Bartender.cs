﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Labb6
{
    public class Bartender
    {
        readonly Pub pub;
        private Glass currentGlass;
        private Patron currentPatron;
        public Bartender(Pub pub)
        {
            this.pub = pub;
        }

        public void Run() {
            pub.Log("Arrived", LogBox.Bartender);

            Task.Run(() => 
            { 
                while (pub.IsOpen || (pub.WaitingPatrons.Count + pub.BarDisk.Count) > 0)
                {
                    if (currentPatron == null)
                    {
                        TryGetPatron();
                    }
                    else
                    {
                        if (currentGlass == null)
                            TryGetGlass();
                        else
                        {
                            pub.Log($"Pouring beer for {currentPatron.Name}", LogBox.Bartender);
                            Thread.Sleep((int)(pub.Options.BartenderPourTiming / pub.Options.Speed));
                            pub.mainWindow.pauseBartender.WaitOne();
                            lock (pub.BarDisk)
                            {
                                pub.BarDisk.Add(currentPatron, currentGlass);
                            }
                            currentPatron = null;
                            currentGlass = null;
                        }
                    }

                }
                WaitForPatronsAndLoveInterestWaitress();
            }, pub.mainWindow.token);
        }

        private void WaitForPatronsAndLoveInterestWaitress()
        {
            pub.Log("Waiting for patrons to leave", LogBox.Waitress);
            while ((pub.WaitingPatrons.Count + pub.BarDisk.Count) > 0 || pub.WaitressIsPresent) { }
            pub.Log("Left the bar together with the woman of his dreams, the Waitress...\n", LogBox.Bartender);
            pub.BartenderIsPresent = false;
        }

        private void TryGetPatron()
        {
            pub.mainWindow.pauseBartender.WaitOne();

            if (pub.WaitingPatrons.TryDequeue(out Patron patron))
            {
                currentPatron = patron;
                pub.Log($"Serving {currentPatron.Name}", LogBox.Bartender);
                return;
            }
        }

        private void TryGetGlass()
        {
            pub.mainWindow.pauseBartender.WaitOne();

            if (pub.Shelf.TryPeek(out _))
            {
                pub.Log("Getting glass", LogBox.Bartender);
                Thread.Sleep((int)(pub.Options.BartenderGlassTiming / pub.Options.Speed));
                pub.mainWindow.pauseBartender.WaitOne();

                pub.Shelf.TryPop(out Glass glass);
                currentGlass = glass;
                pub.Log("Got glass", LogBox.Bartender);
                return;
            }
        }
    }
}