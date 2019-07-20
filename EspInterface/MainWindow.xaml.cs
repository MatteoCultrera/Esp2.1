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
        public MainWindow()
        {
            InitializeComponent();
            setup = new SetupModel();
            monitor = new MonitorModel();
            statistics = new StatisticsModel();

            listItems = new List<menuItem>();
            listItems.Add(new menuItem() { enabled = true, text = "Border Setup" });
            listItems.Add(new menuItem() { enabled = false, text = "Room Monitor" });
            listItems.Add(new menuItem() { enabled = false, text = "Statistics" });
            listItems.Add(new menuItem() { enabled = true, text = "Quit App" });
            

            lbMenu.ItemsSource = listItems;
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
