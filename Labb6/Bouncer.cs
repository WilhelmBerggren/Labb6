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


            if (pub.BadGuyBouncer)
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
            Pub.WhileOpen(pub, () =>
            {
                int wait = random.Next((int)pub.Params["BouncerMinTiming"], (int)pub.Params["BouncerMaxTiming"]);
                Pub.Sleep(wait, pub.mainWindow.pauseBouncerAndPatrons);
                pub.RunAsTask(() => _ = new Patron(pub));
            });
        }

        private void BadGuyBouncer(Random random)
        {
            Pub.WhileOpen(pub, () =>
            {
                // IF timer is after the first 20 seconds, instantiate 15 new patrons all at once.
                // This occurs only once. After that, he continues as per usual...
                int wait = random.Next((int)pub.Params["BouncerMinTiming"], (int)pub.Params["BouncerMaxTiming"]);
                Pub.Sleep(wait, pub.mainWindow.pauseBouncerAndPatrons);
                pub.RunAsTask(() => _ = new Patron(pub));
            });
        }
    }
}