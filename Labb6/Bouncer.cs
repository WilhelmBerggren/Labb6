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

            while (pub.IsOpen)
            {
                if (pub.mainWindow.token.IsCancellationRequested)
                    return;

                int wait = random.Next((int)pub.Options.BouncerMinTiming, (int)pub.Options.BouncerMaxTiming);
                Thread.Sleep(wait);
                pub.mainWindow.pauseBouncerAndPatrons.WaitOne();
                CreatePatron();
            }
        }

        private void CreatePatron()
        {
            if(pub.Options.BadGuyBouncer)
            {
                if (pub.mainWindow.BarOpenForDuration <= 100)
                {
                    pub.Options.BadGuyBouncer = false;
                    pub.Log("Oh shit, a bus full of tourists", LogBox.Event);
                    for (int i = 0; i < 15; i++)
                    {
                        Task.Run(() => new Patron(pub), pub.mainWindow.token);
                    }
                }
                else
                {
                    Task.Run(() => new Patron(pub), pub.mainWindow.token);
                }
            }
            else if(pub.Options.CouplesNight)
            {
                Task.Run(() => new Patron(pub), pub.mainWindow.token);
                Task.Run(() => new Patron(pub), pub.mainWindow.token);
            }
            else
            {
                Task.Run(() => new Patron(pub), pub.mainWindow.token);
            }
        }
    }
}