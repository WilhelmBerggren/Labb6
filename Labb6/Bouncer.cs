using System;
using System.Threading;
using System.Threading.Tasks;

namespace Labb6
{
    public class Bouncer
    {
        private readonly Pub pub;
        public Bouncer(Pub pub)
        {
            this.pub = pub ?? throw new ArgumentNullException(nameof(pub));
            Random random = new Random();

            if (pub.PubOptions.BadGuyBouncer == 1)
            {
                BadGuyBouncer(random);
                pub.Log("The bouncer bangs his chest...\n" +
                    "No more Mr. Niceguy!", LogBox.Event);
            }
            else
            {
                NiceGuyBouncer(random);
            }
        }

        private void NiceGuyBouncer(Random random)
        {
            while (pub.IsOpen)
            {
                if (pub.mainWindow.token.IsCancellationRequested)
                    return;

                int wait = random.Next((int)pub.PubOptions.BouncerMinTiming, (int)pub.PubOptions.BouncerMaxTiming);
                Thread.Sleep(wait);
                pub.mainWindow.pauseBouncerAndPatrons.WaitOne();
                pub.RunAsTask(() => _ = new Patron(pub));
            }
        }

        private void BadGuyBouncer(Random random)
        {
            while (pub.IsOpen)
            {
                if (pub.mainWindow.token.IsCancellationRequested)
                    return;

                if (pub.mainWindow.BarOpenForDuration <= 100 && pub.mainWindow.BarOpenForDuration > 90)
                {
                    for (int i = 0; i < 15; i++)
                    {
                        pub.RunAsTask(() => _ = new Patron(pub));
                    }
                }
                else
                {
                    int wait = random.Next((int)pub.PubOptions.BouncerMinTiming, (int)pub.PubOptions.BouncerMaxTiming);
                    Thread.Sleep(wait);
                    pub.mainWindow.pauseBouncerAndPatrons.WaitOne();
                    pub.RunAsTask(() => _ = new Patron(pub));
                }
            }
        }
    }
}