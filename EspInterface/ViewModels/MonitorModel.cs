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
namespace EspInterface.ViewModels
{
    public class MonitorModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //Private attributes
        private ObservableCollection<Board> _boards;
        private String _title;
        private String _subtitle;
        private double _maxRoomSize;
        private List<Device>[][] devicesInGrid = new List<Device>[10][];
        private ObservableCollection<Device> _currentDevicesList;
        private List<Device> totalDevicesList;
        private int currentX, currentY;
        private Timer roomCheckTimer;
        private int counter = 60;

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
                    _boards = value;
                setBoardsPositions();
                this.NotifyPropertyChanged("boards");
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

        public double maxRoomSize
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
        public void setBoardsPositions() {
            
        }

        public void newData(List<Device> newDevices)
        {
            totalDevicesList.Clear();
            clearMatrix();
            totalDevicesList = newDevices;
            createMatrix(newDevices);

            newDataAvailable?.Invoke(this, null);

            if (currentX == -1 || currentY == -1)
                return;

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

    //Classes For Data Conversion
}
