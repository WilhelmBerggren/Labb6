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

            Pub.WhileOpen(pub, () =>
            {
                int wait = random.Next((int) pub.Params["BouncerMinTiming"], (int) pub.Params["BouncerMaxTiming"]);
                Pub.Sleep(wait, pub.mainWindow.pauseBouncerAndPatrons);
                 pub.RunAsTask(() => _ = new Patron(pub));
            });
        }
    }
}