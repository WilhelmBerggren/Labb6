using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private void Pause_Bartender_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Pause_Waitress_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Pause_Guests_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ToggleBarOpen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Panic_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    delegate void CustomerServed();

    class Bar
    {
        private CustomerServed CustomerServed { get; set; }
        public bool Open { get; set; }
        public BlockingCollection<int> Shelf { get; set; }
        public BlockingCollection<int> Chairs { get; set; }
        public ConcurrentQueue<Patron> Patrons { get; set; }
        private Bartender bartender;

        public void NextPatron()
        {
            Patrons.TryDequeue(out Patron patron);
            if (patron != default)
                CustomerServed();
        }
    }
    class Bartender
    {
        public void ServeClient() { }
    }
    class Waitress { }
    class Patron { }
}
