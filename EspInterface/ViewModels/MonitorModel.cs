using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EspInterface.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Timers;
using System.Text.RegularExpressions;
using System.Windows;

namespace EspInterface.ViewModels
{
    public class MonitorModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //Private attributes
        private ObservableCollection<Board> _boards;
        private String _title;
        private String _subtitle;
        private int _maxRoomSize;
        private List<Device>[][] devicesInGrid = new List<Device>[10][];
        private ObservableCollection<Device> _currentDevicesList;
        private List<Device> totalDevicesList;
        private int currentX, currentY;
        private Timer roomCheckTimer;
        private int counter = 60;
        private bool[,] mask = new bool[10, 10];
        private bool hasData = false;

        //Public attributes
        public ObservableCollection<Device> currentDevicesList
        {
            get { return _currentDevicesList; }
            set
            {
                if(_currentDevicesList != value)
                {
                    this._currentDevicesList = value;
                    NotifyPropertyChanged("currentDevicesList");
                }
            }
        }
        public ObservableCollection<Board> boards
        {
            get { return _boards; }
            set
            {
                if (value != this._boards)
                {
                    _boards = value;

                    this.NotifyPropertyChanged("boards");

                    generateMatrix();
                }
                
            }
        }

        public string title
        {
            get { return this._title; }
            set
            {
                if(this._title != value)
                {
                    this._title = value;
                    NotifyPropertyChanged("title");
                }
            }
        }

        public string subtitle
        {
            get { return this._subtitle; }
            set
            {
                if (this._subtitle != value)
                {
                    this._subtitle = value;
                    NotifyPropertyChanged("subtitle");
                }
            }
        }

        public int maxRoomSize
        {
            get { return this._maxRoomSize; }
            set
            {
                if(this._maxRoomSize != value)
                {
                    this._maxRoomSize = value;
                    NotifyPropertyChanged("maxRoomSize");
                }
            }
        }

        public event EventHandler<EventArgs> newDataAvailable;

        //Methods

        public List<Board> getBoards()
        {
            return boards.ToList();
        }

        public void newData(List<Device> newDevices)
        {
            totalDevicesList.Clear();
            clearMatrix();
           

            Point[] pol = new Point[boards.Count];

            for (int i = 0; i < boards.Count; i++)   
                pol[i] = new Point(boards[i].posX, boards[i].posY);


            foreach (Device d in newDevices)
                if (PointInPolygon(pol, new Point(d.x, d.y)))
                    totalDevicesList.Add(d);

            createMatrix(totalDevicesList);


            hasData = true;
            newDataAvailable?.Invoke(this, null);

            if (currentX == -1 || currentY == -1)
                return;

        }

        public ObservableCollection<Device> getGridDevices(int x, int y)
        {
            ObservableCollection<Device> toReturn = new ObservableCollection<Device>(devicesInGrid[x][y]);
            return toReturn;
        }

        public List<Device> getAllDevices()
        {
            return totalDevicesList;
        }

        private void clearMatrix()
        {
            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                    devicesInGrid[i][j].Clear();
        }
        private void createMatrix(List<Device> newDevices)
        {
            foreach(Device d in newDevices)
            {
                devicesInGrid[d.xInt][d.yInt].Add(d);
            }

        }

        public void generateMatrix()
        {
            string s = "";

            Point[] pol = new Point[boards.Count];

            for (int i = 0; i < boards.Count; i++)
            {
                pol[i] = new Point(boards[i].posX, boards[i].posY);
            }


            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                {
                    Point test = new Point(i + 0.5, j + 0.5);
                    mask[i, j] = PointInPolygon(pol, test);
                }
        }

        public static bool PointInPolygon(Point[] polygon, Point testPoint)
        {
            bool result = false;
            int j = polygon.Count() - 1;
            for (int i = 0; i < polygon.Count(); i++)
            {
                if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y || polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
                {
                    if (polygon[i].X + (testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < testPoint.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public void timerElapsed(object source, ElapsedEventArgs e)
        {
            counter--;
            if(counter == 0)
            {
                roomCheckTimer.Stop();
                subtitle = "Waiting new data...";
                return;
            }
            subtitle = "Next Data in 0." + counter + " s";
        }

        public void startedScanning()
        {
            roomCheckTimer.Stop();
            counter = 60;
            roomCheckTimer.Start();
        }

        public int numDevices(int x, int y)
        {
            if(devicesInGrid[x][y].Count == 0)
            {
                return -1;
            }
            else
            {
                return devicesInGrid[x][y].Count;
            }
        }

        public Device findDevice(string MAC)
        {
            Device toRet = null;

            foreach(Device d in totalDevicesList)
            {
                if (d.mac.Equals(MAC))
                    toRet = d;
            }

            return toRet;

        }

        //Constructor
        public MonitorModel()
        {
            currentX = -1;
            currentY = -1;

            totalDevicesList = new List<Device>();

            for (int i = 0; i < 10; i++)
                devicesInGrid[i] = new List<Device>[10];


            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                    devicesInGrid[i][j] = new List<Device>();

            roomCheckTimer = new Timer(1000);
            roomCheckTimer.Elapsed += timerElapsed;
            roomCheckTimer.AutoReset = true;

        }

    }

    //Classes For Data Passing through Events

}
