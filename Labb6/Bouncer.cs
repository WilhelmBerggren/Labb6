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
            pub.RunAsTask(() =>
            {
                Thread.Sleep(new Random().Next(pub.Params["BouncerMinTiming"], pub.Params["BouncerMaxTiming"]));
                Task.Run(() =>
                {
                    // Ligger här för att stoppa denna tasken att skapa en ny patron hela tiden
                    pub.mainWindow.pauseBouncerAndPatrons.WaitOne(Timeout.Infinite);
                    _ = new Patron(pub);
                }, pub.mainWindow.token);
            });
        }
    }
}