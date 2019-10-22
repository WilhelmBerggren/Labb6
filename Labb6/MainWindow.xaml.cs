using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Labb6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Pause_Bartender_Click(object sender, RoutedEventArgs e) { }
        private void Pause_Waitress_Click(object sender, RoutedEventArgs e) { }
        private void Pause_Guests_Click(object sender, RoutedEventArgs e) { }
        private void ToggleBarOpen_Click(object sender, RoutedEventArgs e) { }
        private void Panic_Click(object sender, RoutedEventArgs e) { }
    }

    delegate void BartenderGetGlass();
    delegate void BartenderPourBeer();
    delegate void BartenderGoHome();
    delegate void WaitressCollectGlasses();
    delegate void WaitressWashGlasses();
    delegate void WaitressPlaceGlasses();
    delegate void BouncerGoHome();
    delegate void PatronArrive(Patron patron);
    delegate void PatronSitDown(Patron patron);
    delegate void PatronFinishBeer(Patron patron);
    delegate void PatronLeave(Patron patron);

    delegate void AddPatron(Patron patron);

    class Bar
    {
        public bool Open { get; set; }
        public BlockingCollection<int> Shelf { get; set; }
        public BlockingCollection<int> Chairs { get; set; }
        public ConcurrentQueue<Patron> Patrons { get; set; }
        private Bartender bartender;
        private Waitress waitress;
        private Bouncer bouncer;

        AddPatron addPatron;

        public Bar()
        {
            Shelf = new BlockingCollection<int>();
            Chairs = new BlockingCollection<int>();
            Patrons = new ConcurrentQueue<Patron>();
            bartender = new Bartender();
            waitress = new Waitress();
            bouncer = new Bouncer(addPatron);

            addPatron += DoAddPatron;
        }

        void DoAddPatron(Patron patron)
        {
            Patrons.Enqueue(patron);
        }

        public void NextPatron()
        {
            Patrons.TryDequeue(out Patron patron);
            //if (patron != default)
                //PatronServed(patron);
        }
    }

    class Bartender
    {
        public event Action GetGlass, PourBeer, GoHome;
        public Bartender()
        {
            DoGetGlass();
            DoPourBeer();
            DoGoHome();
        }
        public void DoGetGlass() { GetGlass(); }
        public void DoPourBeer() { PourBeer();  }
        public void DoGoHome() { GoHome(); }
    }

    class Waitress
    {
        public event Action CollectGlasses, WashGlasses, PlaceGlasses;
        public Waitress() => Task.Run(() =>
            {
                DoCollectGlasses();
                DoWashGlasses();
                DoPlaceGlasses();
            });

        public void DoCollectGlasses() { CollectGlasses(); }
        public void DoWashGlasses() { WashGlasses(); }
        public void DoPlaceGlasses() { PlaceGlasses(); }
    }
        
    class Bouncer
    {
        public Bouncer(AddPatron addPatron) => Task.Run(() =>
            {
                Thread.Sleep(1000);
                addPatron(new Patron());
            });

        public event Action GoHome;
        public void DoGoHome() { GoHome(); }
    }

    class Patron
    {
        public event Action Arrive, SitDown, FinishBeer, Leave;
        public Patron() => Task.Run(() =>
            {
                DoArrive();
                DoSitDown();
                DoFinishBeer();
                DoLeave();
            });
        public void DoArrive() { Arrive(); }
        public void DoSitDown() { SitDown(); }
        public void DoFinishBeer() { FinishBeer(); }
        public void DoLeave() { Leave(); }
    }
}
