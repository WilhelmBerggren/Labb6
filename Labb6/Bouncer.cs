using System;
using System.Threading;
using System.Threading.Tasks;

namespace Labb6
{
    public class Bouncer
    {
        private readonly Pub pub;
        private Random random;
        public Bouncer(Pub pub)
        {
            this.pub = pub ?? throw new ArgumentNullException(nameof(pub));
            this.random = new Random();
        }

        public void Run() {
            Task.Run(() =>
            {
                while (pub.IsOpen)
                {
                    if (pub.mainWindow.token.IsCancellationRequested)
                        return;

                    int wait = random.Next((int)pub.Options.BouncerMinTiming, (int)pub.Options.BouncerMaxTiming);
                    Thread.Sleep(wait);
                    pub.mainWindow.pauseBouncerAndPatrons.WaitOne();
                    CreatePatron();
                }
            });
        }

        private void CreatePatron()
        {
            if(pub.Options.BadGuyBouncer)
            {
                if (pub.TimeUntilClosing <= 100)
                {
                    pub.Options.BadGuyBouncer = false;
                    pub.Log("Oh shit, a bus full of tourists!", LogBox.Event);

                    for (int i = 0; i < 15; i++)
                    {
                        new Patron(pub).Run();
                    }
                }
                else
                {
                    new Patron(pub).Run();
                }
            }
            else if(pub.Options.CouplesNight)
            {
                new Patron(pub).Run();
                new Patron(pub).Run();
            }
            else
            {
                new Patron(pub).Run();
            }
        }
    }
}