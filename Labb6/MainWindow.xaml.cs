using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Labb6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum TextBlocks { Event, Bartender, Waitress, Patron}
        public MainWindow()
        {
            InitializeComponent();
            LogEvent("asdffdsfdasasdf", TextBlocks.Event);
            Bar bar = new Bar(this);
        }

        private void Pause_Bartender_Click(object sender, RoutedEventArgs e) { }
        private void Pause_Waitress_Click(object sender, RoutedEventArgs e) { }
        private void Pause_Guests_Click(object sender, RoutedEventArgs e) { }
        private void ToggleBarOpen_Click(object sender, RoutedEventArgs e) 
        {
            //EventTextBlock.Text += "lkadfklfad";
        }
        private void Panic_Click(object sender, RoutedEventArgs e) { }

        public void LogEvent(string text, TextBlocks textblock)
        {
            switch (textblock)
            {
                case TextBlocks.Event:
                    this.Dispatcher.Invoke(() => EventTextBlock.Text += text);
                    break;
                case TextBlocks.Bartender:
                    this.Dispatcher.Invoke(() => BartenderTextBlock.Text += text);
                    break;
                case TextBlocks.Patron:
                    this.Dispatcher.Invoke(() => PatronTextBlock.Text += text);
                    break;
                case TextBlocks.Waitress:
                    this.Dispatcher.Invoke(() => WaitressTextBlock.Text += text);
                    break;
            }
        }
    }

    class Bar
    {
        public bool IsOpen { get; set; }
        public MainWindow mainWindow;
        public ConcurrentQueue<Patron> Patrons { get; set; }
        private Bartender bartender;
        private Waitress waitress;
        private Bouncer bouncer;
        public ConcurrentStack<Glass> Shelf { get; set; }
        public ConcurrentStack<Glass> Table { get; set; }
        public List<Patron> Chairs { get; set; }
        public int NumberOfChairs;

        public Bar(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            IsOpen = true;
            InitShelf(5);
            Table = new ConcurrentStack<Glass>();
            Patrons = new ConcurrentQueue<Patron>();
            Chairs = new List<Patron>();
            NumberOfChairs = 5;

            bouncer = new Bouncer(this);
            bartender = new Bartender(this);
            waitress = new Waitress(this);
        }

        public void Log(string text)
        {
            mainWindow.Dispatcher.Invoke(() => mainWindow.EventTextBlock.Text.Insert(0, text));
        }

        public void InitShelf(int numberOfGlasses)
        {
            Shelf = new ConcurrentStack<Glass>();
            for (int i = 0; i < numberOfGlasses; i++)
            {
                Shelf.Push(new Glass());
            }
        }
    }

    public class Glass { }

    class Bouncer
    {
        public Bouncer(Bar bar)
        {
            Task.Run(() =>
            {
                while (bar.IsOpen)
                {
                    Thread.Sleep(1000);
                    bar.Patrons.Enqueue(new Patron(bar));
                    System.Console.WriteLine("Bouncer: let in patron");
                }
            });
        }
    }

    class Bartender
    {
        Bar bar;
        Glass currentGlass;
        Patron currentPatron;
        public Bartender(Bar bar)
        {
            this.bar = bar;
            Task.Run(() =>
            {
                while (bar.IsOpen)
                {
                    currentGlass = WaitForGlass();
                    currentPatron = WaitForPatron();
                    Thread.Sleep(3000);
                    currentPatron.Serve(currentGlass);
                }
            });
        }

        Glass WaitForGlass()
        {
            bool gotGlass = false;
            while(!gotGlass)
            {
                if (bar.Shelf.TryPop(out Glass glass))
                {
                    Console.WriteLine("Bartender: got glass");
                    Console.WriteLine("Shelf count: "+bar.Shelf.Count);
                    Thread.Sleep(3000);
                    return glass;
                }
            }
            return default;
        }
        Patron WaitForPatron()
        {
            bool foundPerson = false;
            while(!foundPerson)
            {
                if (bar.Patrons.TryDequeue(out Patron patron))
                {
                    Console.WriteLine("Bartender: got patron");
                    Console.WriteLine("Patron count: "+bar.Patrons.Count);
                    return patron;
                }
            }
            return default;
        }
    }

    class Waitress
    {
        Bar bar;
        Glass currentGlass;
        public Waitress(Bar bar)
        {
            this.bar = bar;
            Task.Run(() =>
            {
                while (bar.IsOpen)
                {
                    Thread.Sleep(1000);
                    currentGlass = CheckForGlass();
                    Thread.Sleep(1000);
                    PlaceGlass();
                }
            });
        }

        private void PlaceGlass()
        {
            bar.Shelf.Push(currentGlass);
            Console.WriteLine("Waitress: Placed glass");
            Console.WriteLine("Shelf glass count: " + bar.Shelf.Count);
        }

        private Glass CheckForGlass()
        {
            bool foundPerson = false;
            while (!foundPerson)
            {
                if (bar.Table.TryPop(out Glass currentGlass))
                {
                    Console.WriteLine("Waitress: got glass");
                    Console.WriteLine("Table glass count: " + bar.Table.Count);
                    return currentGlass;
                }
            }
            return default;
        }

        private void Log(string text)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.Dispatcher.Invoke(() =>
            {
                mainWindow.WaitressTextBlock.Text += text;
            });
        }
    }

    class Patron
    {
        private Bar bar;
        public bool Finished;
        public Patron(Bar bar)
        {
            this.bar = bar;
        }
        public void Serve(Glass glass)
        {
            Finished = false;
            Task.Run(() =>
            {
                CheckForTable();
                Thread.Sleep(1000);
                Leave();
                Thread.Sleep(1000);
            });
        }

        private void Leave()
        {
            lock(bar.Chairs)
            {
                bar.Chairs.Remove(this);
            }
        }

        private void CheckForTable()
        {
            bool foundTable = false;
            while (!foundTable)
            {
                if (bar.Chairs.Count < bar.NumberOfChairs)
                {
                    lock(bar.Chairs)
                    {
                        bar.Chairs.Add(this);
                    }
                    Console.WriteLine("Patron: got chair");
                    Console.WriteLine("Chair count: " + bar.Chairs.Count);
                    return;
                }
            }
        }
    }
}