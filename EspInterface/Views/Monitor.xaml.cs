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
using System.Text.RegularExpressions;
using EspInterface.ViewModels;
using EspInterface.Models;

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
        private List<boardsInGrid> boards = new List<boardsInGrid>();
        private List<devicesInGrid> deviceSearched = new List<devicesInGrid>();

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

            mm.newDataAvailable += newData_D;

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

            List<Board> modelBoards = mm.getBoards();

            boardsInGrid last = null;

            for(int i = 0; i < modelBoards.Count; i++)
            {
                boardsInGrid bing = new boardsInGrid();
                bing.b = modelBoards[i];
                bing.boardImage = new Image();
                bing.boardImage.Source = new BitmapImage(new Uri("/Resources/Icons/boardDevice.png", UriKind.Relative));
                bing.boardImage.Width = 12;
                bing.boardImage.Height = 12;
              
                canvas.Children.Add(bing.boardImage);
                Panel.SetZIndex(bing.boardImage, 25);

                //initialPosX = 55.2, initialPosY = 110.3;

                Canvas.SetLeft(bing.boardImage, 66.6 + offset * bing.b.posX);
                Canvas.SetBottom(bing.boardImage, 86.6 + offset * bing.b.posY);

                SolidColorBrush white = new SolidColorBrush();
                white.Color = Colors.White;

                bing.connectLine = new Line();

                bing.connectLine.StrokeThickness = 3;
                bing.connectLine.Stroke = white;
                bing.connectLine.Visibility = Visibility.Collapsed;
                

                if (i + 1 < modelBoards.Count && modelBoards.Count != 1)
                {
                    bing.connectLine.X1 = 66.6 + offset * bing.b.posX + Measures.smalloffsetBoard;
                    bing.connectLine.Y1 = 575 - (86.6 + offset * bing.b.posY + Measures.smalloffsetBoard);
                    bing.connectLine.X2 = 66.6 + offset * modelBoards[i + 1].posX + Measures.smalloffsetBoard;
                    bing.connectLine.Y2 = 575 - (86.6 + offset * modelBoards[i+1].posY + Measures.smalloffsetBoard);
                    bing.connectLine.Visibility = Visibility.Visible;
                }
                else if(i+1 == modelBoards.Count && modelBoards.Count > 2)
                {
                    bing.connectLine.X1 = 66.6 + offset * bing.b.posX + Measures.smalloffsetBoard;
                    bing.connectLine.Y1 = 575 - (86.6 + offset * bing.b.posY + Measures.smalloffsetBoard);
                    bing.connectLine.X2 = 66.6 + offset * modelBoards[0].posX + Measures.smalloffsetBoard;
                    bing.connectLine.Y2 = 575 - (86.6 + offset * modelBoards[0].posY + Measures.smalloffsetBoard);
                    bing.connectLine.Visibility = Visibility.Visible;
                }
                boards.Add(bing);
                canvas.Children.Add(bing.connectLine);
                Panel.SetZIndex(bing.connectLine, 23);
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
            //DevicesLB.ItemsSource = mm.getGridDevices(x, y); 
            changeData_D(x, y, true);

        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            MonitorModel mm = (MonitorModel)(this.DataContext);

            mm.newDataAvailable -= newData_D;
        }

        

            private void newData_D(object sender, EventArgs e)
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

            updateBoardGrid_D();
            
        }

        private void updateBoardGrid_D()
        {

            MonitorModel mm = (MonitorModel)(this.DataContext);

            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                {
                    devicesMatrix[i][j].numDevices = mm.numDevices(i, j);
                    if(devicesMatrix[i][j].deviceCheckbox.IsChecked == true)
                    {
                        if (devicesMatrix[i][j].numDevices != -1)
                        {
                            devicesMatrix[i][j].deviceCheckbox.Visibility = Visibility.Visible;
                            devicesMatrix[i][j].deviceCheckbox.Content = devicesMatrix[i][j].numDevices + "";
                            //DevicesLB.ItemsSource = mm.getGridDevices(i, j);
                            changeData_D(i, j, true);
                        }
                        else
                        {
                            devicesMatrix[i][j].deviceCheckbox.Visibility = Visibility.Collapsed;
                            //DevicesLB.ItemsSource = null;
                            changeData_D(0, 0, false);
                         }
                        break;
                    }
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

        private void changeData_D(int x, int y, bool hasData)
        {
            MonitorModel mm = (MonitorModel)this.DataContext;
            DoubleAnimation fadeOut = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(400)
            };
            Storyboard.SetTarget(fadeOut, DevicesLB);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(Control.OpacityProperty));
            DoubleAnimation fadeIn = new DoubleAnimation()
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(400)
            };
            Storyboard.SetTarget(fadeIn, DevicesLB);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(Control.OpacityProperty));

            Storyboard fadeChange = new Storyboard();
            Storyboard changed = new Storyboard();
            fadeChange.Children.Add(fadeOut);
            changed.Children.Add(fadeIn);

            fadeChange.Completed += (s, a) => {
                if (hasData == true)
                    DevicesLB.ItemsSource = mm.getGridDevices(x, y);
                else
                    DevicesLB.ItemsSource = null;
                changed.Begin();
            };
            fadeChange.Begin();
        }

        private void shrinkListBox(int x, int y) {

            DoubleAnimation fadeOut = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(400)
            };
            Storyboard.SetTarget(fadeOut, DevicesLB);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(Control.OpacityProperty));
            DoubleAnimation shrinkBorder = new DoubleAnimation()
            {
                To = 77.5,
                Duration = TimeSpan.FromMilliseconds(400)
            };
            Storyboard.SetTarget(shrinkBorder, Border1);
            Storyboard.SetTargetProperty(shrinkBorder, new PropertyPath(Control.HeightProperty));
            DoubleAnimation shrinkList = new DoubleAnimation()
            {
                To = 77.5,
                Duration = TimeSpan.FromMilliseconds(400)
            };
            Storyboard.SetTarget(shrinkList, DevicesLB);
            Storyboard.SetTargetProperty(shrinkList, new PropertyPath(Control.HeightProperty));
            DoubleAnimation fadeIn = new DoubleAnimation()
            {
                BeginTime = TimeSpan.FromMilliseconds(400),
                To = 1,
                Duration = TimeSpan.FromMilliseconds(400)
            };
            Storyboard.SetTarget(fadeIn, DevicesLB);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(Control.OpacityProperty));

            Storyboard fadeChange = new Storyboard();
            Storyboard changed = new Storyboard();
            fadeChange.Children.Add(fadeOut);
            changed.Children.Add(shrinkBorder);
            changed.Children.Add(shrinkList);
            changed.Children.Add(fadeIn);

            fadeChange.Completed += (s, a) => {
                //Here place the data inside
                
                changed.Begin();
            };

            fadeChange.Begin();

        }

        private void enlargeListBox() {

        }

        private void handleTextMAC(object sender, TextCompositionEventArgs e)
        {
            TextBox box = sender as TextBox;
            Regex regex = new Regex("^[a-fA-F0-9:]*$");

            string text = box.GetLineText(0);

            if (box.GetLineLength(0) > 16)
            {
                e.Handled = true;
                return;
            }

            

            e.Handled = !regex.IsMatch(e.Text);
        }

        private void handleKeyMAC(object sender, KeyEventArgs e)
        {
            TextBox box = sender as TextBox;

            if (e.Key == Key.Enter)
            {
               
                ClearFocus.Focus();
            }

        }

        private void mac_LostFocus(object sender, RoutedEventArgs e)
        {
            MonitorModel mm = (MonitorModel)this.DataContext;
            TextBox box = sender as TextBox;
            Regex regex = new Regex("^[0-9A-F]{2}:[0-9A-F]{2}:[0-9A-F]{2}:[0-9A-F]{2}:[0-9A-F]{2}:[0-9A-F]{2}$");

         

            if (regex.IsMatch(box.Text)){
                //TODO: add code to search MAC


            }
            else
            {
                
                showErrorMessage("Wrong MAC");
            }

        }

        private void mac_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).Text = "";
        }

        private void showErrorMessage(string message)
        {
            MacTextBox.Focusable = false;
            TimeSpan timing = new TimeSpan(0, 0, 0, 0, 200);
            TimeSpan secondAnim = new TimeSpan(0, 0, 2);

            DoubleAnimation fadeOut1 = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200)
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

            Storyboard.SetTarget(firstFading, MacTextBox);
            Storyboard.SetTargetProperty(firstFading, new PropertyPath(Control.OpacityProperty));

            Storyboard.SetTarget(secondFading, MacTextBox);
            Storyboard.SetTargetProperty(secondFading, new PropertyPath(Control.OpacityProperty));

            Storyboard.SetTarget(thirdFading, MacTextBox);
            Storyboard.SetTargetProperty(thirdFading, new PropertyPath(Control.OpacityProperty));

            firstFading.Completed += (s, a) => {
               
                MacTextBox.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF623F"));
                MacTextBox.Text = message;
                secondFading.Begin();
            };

            secondFading.Completed += (s, a) => {
                MacTextBox.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#707070"));
                MacTextBox.Text = "Search MAC";
                thirdFading.Begin();
            };

            thirdFading.Completed += (s, a) => {
                MacTextBox.Focusable = true;
            };

            firstFading.Begin();
        }


    }

    //Helper Classes
    public class devicesInGrid
    {
        public int x, y;
        public int numDevices;
        public CheckBox deviceCheckbox;
    }

    public class boardsInGrid
    {
        public Board b;
        public Image boardImage;
        public Line connectLine;
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
