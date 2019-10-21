using System;
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
using EspInterface.ViewModels;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using EspInterface.Models;

namespace EspInterface
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SetupModel setup;
        public MonitorModel monitor;
        public StatisticsModel statistics;

        List<menuItem> listItems;
        List<Board> boards;
        private DebugPhase phase = DebugPhase.monitor;



        public enum DebugPhase
        {
            none, setup, monitor, statistics
        };

        public MainWindow()
        {
            InitializeComponent();
            setup = new SetupModel(this);
            monitor = new MonitorModel();
            statistics = new StatisticsModel();

            listItems = new List<menuItem>();

            switch (phase)
            {
                case DebugPhase.none:
                case DebugPhase.setup:
                    listItems.Add(new menuItem() { enabled = true, text = "Border Setup" });
                    listItems.Add(new menuItem() { enabled = false, text = "Room Monitor" });
                    listItems.Add(new menuItem() { enabled = false, text = "Statistics" });
                    listItems.Add(new menuItem() { enabled = true, text = "Quit App" });
                    break;

                case DebugPhase.monitor:
                    //Here we generate a stub of boards to be used on the monitor phase
                    listItems.Add(new menuItem() { enabled = false, text = "Border Setup" });
                    listItems.Add(new menuItem() { enabled = true, text = "Room Monitor" });
                    listItems.Add(new menuItem() { enabled = true, text = "Statistics" });
                    listItems.Add(new menuItem() { enabled = true, text = "Quit App" });
                    boards = new List<Board>();
                    for (int i = 0; i < 4; i++)
                    {
                        boards.Add(new Board("/Resources/Icons/Boards/Board" + i + "N.png", "Board " + i.ToString(), false, i));
                        boards[i].HasMac = true;
                        boards[i].MAC = "AA:AA:AA:AA:AA:AA:A" + i;
                    }

                    boards[0].posX = 0; boards[0].posY = 0;
                    boards[0].posX = 10; boards[0].posY = 0;
                    boards[0].posX = 0; boards[0].posY = 10;
                    boards[0].posX = 10; boards[0].posY = 10;
                    ObservableCollection<Board> obsBoards = new ObservableCollection<Board>(boards);
                    monitor.boards = obsBoards;
                    monitor.maxRoomSize = 10;
                    Thread t = new Thread(debugForceMonitor);
                    t.Start();
                    break;


            }
            
            

            lbMenu.ItemsSource = listItems;

            
            if(phase == DebugPhase.monitor)
                lbMenu.SelectedIndex = 1;

            if (phase == DebugPhase.statistics)
                lbMenu.SelectedIndex = 2;
        
            //Debug code to force a single phase
        }

        public void setupEnded(List<Board> boards)
        {
            this.boards = boards;

            listItems[0].enabled = false;
            listItems[1].enabled = true;
            listItems[2].enabled = true;

            ObservableCollection<Board> obsBoards = new ObservableCollection<Board>(boards);
            monitor.boards = obsBoards;
        
        }

        //Debug code to force Monitor
        private void debugForceMonitor()
        {

            for(int i = 0; i < 20; i++)
            {
                //Can be called from a secondary thread
                monitor.startedScanning();
                //Simulate scanning room
                Thread.Sleep(60000);
                List<Device> newDevices = new List<Device>();
                Random random = new Random();
                for(int num = 0; num < 300; num++)
                {
                    Device d = new Device(GetRandomMacAddress(), random.NextDouble()*9, random.NextDouble()*9, "00,00,00", "date", "time" );
                    //MessageBox.Show(d.mac + " " + d.x + " " + d.xInt + " " + d.y + " " + d.yInt);
                    newDevices.Add(d);
                }
                //Simulate Trilateration Calculation
                Thread.Sleep(100);

                //Must be called from the main thread
                Application.Current.Dispatcher.Invoke(new Action(() => {
                    //This will be executed in the main thread
                    monitor.newData(newDevices);
                }));

            }
        }

        public static string GetRandomMacAddress()
        {
            var random = new Random();
            var buffer = new byte[6];
            random.NextBytes(buffer);
            var result = String.Concat(buffer.Select(x => string.Format("{0}:", x.ToString("X2"))).ToArray());
            return result.TrimEnd(':');
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void lbMenu_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            if (lbMenu.SelectedItem == null)
                return;
            menuItem selected = lbMenu.SelectedItem as menuItem;
            if (selected.enabled == false) //Shouldn't Happen But just in case
                return;

            switch (selected.text) {
                case "Border Setup":
                    DataContext = setup;

                break;

                case "Room Monitor":
                    DataContext = monitor;
                break;

                case "Statistics":
                    DataContext = statistics;
                break;

                case "Quit App":
                    listItems[1].enabled = true;
                    listItems[2].enabled = true;
                break;
            }
           

        }

        private void Quit_Clicked(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }
    }
    public class menuItem : INotifyPropertyChanged
    {
        private bool _enabled;
        public bool enabled {
            get {
                return this._enabled;
            } set {
                if (this._enabled != value) {
                    _enabled = value;
                    NotifyPropertyChanged("enabled");
                }
            }
        }
        public string text { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }


    }

    
}
