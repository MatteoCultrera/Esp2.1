using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EspInterface.ViewModels;

namespace EspInterface.Views
{
    /// <summary>
    /// Logica di interazione per Monitor.xaml
    /// </summary>
    public partial class Monitor : UserControl
    {
        //Private Fields
        private devicesInGrid[][] devicesMatrix = new devicesInGrid[10][];
        private static double initialPosX = 80, initialPosY = 100.3;
        private static double offset = 26.45;

        public Monitor()
        {
            InitializeComponent();
            for(int i = 0; i < 10; i++)
                devicesMatrix[i] = new devicesInGrid[10];

            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                {
                    devicesMatrix[i][j] = new devicesInGrid();
                    devicesMatrix[i][j].numDevices = -1;
                    devicesMatrix[i][j].x = i; devicesMatrix[i][j].y = j;
                }
        }



        //Methods to subscribe events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MonitorModel mm = (MonitorModel)(this.DataContext);

            mm.newDataAvailable += newData;

            Style style = canvas.FindResource("deviceZoom") as Style;

            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                {
                    devicesMatrix[i][j].deviceCheckbox = new CheckBox();
                    devicesMatrix[i][j].deviceCheckbox.Content = devicesMatrix[i][j].numDevices+"";
                    canvas.Children.Add(devicesMatrix[i][j].deviceCheckbox);
                    devicesMatrix[i][j].deviceCheckbox.Style = style;
                    setPositionsInGrid(devicesMatrix[i][j]);
                    devicesMatrix[i][j].deviceCheckbox.Click += setChecked;
                    devicesMatrix[i][j].deviceCheckbox.Visibility = Visibility.Collapsed;
                }

        }

        public void setChecked(object o, RoutedEventArgs e)
        {
            MonitorModel mm = (MonitorModel)(this.DataContext);
            CheckBox curr = (CheckBox)o;
            int x= -1, y = -1;

            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                {
                    devicesMatrix[i][j].deviceCheckbox.IsChecked = false;
                    if (devicesMatrix[i][j].deviceCheckbox.Equals(curr))
                    {
                        x = devicesMatrix[i][j].x; y = devicesMatrix[i][j].y;
                    }

                }


            curr.IsChecked = true;
            DevicesLB.ItemsSource = mm.getGridDevices(x, y);
            

        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            MonitorModel mm = (MonitorModel)(this.DataContext);

            mm.newDataAvailable -= newData;
        }

        private void newData(object sender, EventArgs e)
        {
            TimeSpan timing = new TimeSpan(0, 0, 0, 0, 400);
            TimeSpan secondAnim = new TimeSpan(0, 0, 4);

            DoubleAnimation fadeOut1 = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(400)
            };
            DoubleAnimation fadeIn1 = new DoubleAnimation
            {
                To = 1,
                Duration = timing
            };
            DoubleAnimation fadeOut2 = new DoubleAnimation
            {
                BeginTime = secondAnim,
                To = 0,
                Duration = timing
            };
            DoubleAnimation fadeIn2 = new DoubleAnimation
            { 
                To = 1,
                Duration = timing
            };

            Storyboard firstFading = new Storyboard();
            Storyboard secondFading = new Storyboard();
            Storyboard thirdFading = new Storyboard();

            firstFading.Children.Add(fadeOut1);
            secondFading.Children.Add(fadeIn1);
            secondFading.Children.Add(fadeOut2);
            thirdFading.Children.Add(fadeIn2);

            Storyboard.SetTarget(firstFading, title);
            Storyboard.SetTargetProperty(firstFading, new PropertyPath(Control.OpacityProperty));

            Storyboard.SetTarget(secondFading, title);
            Storyboard.SetTargetProperty(secondFading, new PropertyPath(Control.OpacityProperty));

            Storyboard.SetTarget(thirdFading, title);
            Storyboard.SetTargetProperty(thirdFading, new PropertyPath(Control.OpacityProperty));

            firstFading.Completed += (s, a) => {
                title.Text = "New Data Available";
                secondFading.Begin();
            };

            secondFading.Completed += (s, a) => {
                title.Text = "Scanning Room";
                thirdFading.Begin();
            };

            firstFading.Begin();

            updateBoarGrid();


        }

        private void updateBoarGrid()
        {

            MonitorModel mm = (MonitorModel)(this.DataContext);

            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                {
                    devicesMatrix[i][j].numDevices = mm.numDevices(i, j);
                    if (devicesMatrix[i][j].numDevices != -1)
                    {
                        devicesMatrix[i][j].deviceCheckbox.Visibility = Visibility.Visible;
                        devicesMatrix[i][j].deviceCheckbox.Content = devicesMatrix[i][j].numDevices + "";
                    }
                    else
                        devicesMatrix[i][j].deviceCheckbox.Visibility = Visibility.Collapsed;
                }


        }

        private void setPositionsInGrid(devicesInGrid device)
        {
            Canvas.SetLeft(device.deviceCheckbox, initialPosX + offset * device.x);
            Canvas.SetBottom(device.deviceCheckbox, initialPosY + offset * device.y);
        }


    }

    //Helper Class to Handle Devices in Grid
    public class devicesInGrid
    {
        public int x, y;
        public int numDevices;
        public CheckBox deviceCheckbox;
    }

    //Code for Value Converters
    public class roomSize_StringFloatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return String.Format("{0:##} m", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stringType = (string)value;
            string stringValue = stringType.Split(' ')[0];
            int returnVal;
            Int32.TryParse(stringValue, out returnVal);
            return returnVal;
        }
    }

    public class xDevicePosition_StringFloatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return String.Format("x: {0:##.##}", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stringType = (string)value;
            string stringValue = stringType.Split(' ')[0];
            double returnVal;
            Double.TryParse(stringValue, out returnVal);
            return returnVal;
        }
    }

    public class yDevicePosition_StringFloatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return String.Format("y: {0:##.##}", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stringType = (string)value;
            string stringValue = stringType.Split(' ')[0];
            double returnVal;
            Double.TryParse(stringValue, out returnVal);
            return returnVal;
        }
    }

}
