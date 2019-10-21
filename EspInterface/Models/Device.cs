using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace EspInterface.Models
{
    public class Device: INotifyPropertyChanged
    {
        //Event for property changed
        public event PropertyChangedEventHandler PropertyChanged;

        //Private Fields
        private string _mac;
        private double _x;
        private double _y;
        private int _xInt;
        private int _yInt;
        private string _timestamp;
        private string _date;
        private string _time;

        //Public Fields
        public string mac
        {
            get { return this._mac; }
            set
            {
                if (this._mac != value)
                {
                    this._mac = value;
                    NotifyPropertyChanged("mac");
                }
            }
        }

        public double x
        {
            get { return this._x; }
            set
            {
                if (this._x != value)
                {
                    this._x = value;
                    NotifyPropertyChanged("x");
                }
            }
        }

        public double y
        {
            get { return this._y; }
            set
            {
                if (this._y != value)
                {
                    this._y = value;
                    NotifyPropertyChanged("x");
                }
            }
        }

        public int xInt
        {
            get { return this._xInt; }
            set
            {
                if(this._xInt != value)
                {
                    this._xInt = value;
                    NotifyPropertyChanged("xInt");
                }

            }
        }

        public int yInt
        {
            get { return this._yInt; }
            set
            {
                if (this._yInt != value)
                {
                    this._yInt = value;
                    NotifyPropertyChanged("yInt");
                }

            }
        }

        public string timestamp
        {
            get { return this._timestamp; }
            set
            {
                if (this._timestamp != value)
                {
                    this._timestamp = value;
                    NotifyPropertyChanged("timestamp");
                }
            }
        }

        public string date
        {
            get { return this._date; }
            set
            {
                if (this._date != value)
                {
                    this._date = value;
                    NotifyPropertyChanged("date");
                }
            }
        }

        public string time
        {
            get { return this._time; }
            set
            {
                if (this._time != value)
                {
                    this._time = value;
                    NotifyPropertyChanged("time");
                }
            }
        }

        //Methods


        //Constructor
        public Device(string mac, double x, double y, string timestamp, string date, string time)
        {
            this._mac = mac;
            this._x = x;
            this._y = y;
            this._timestamp = timestamp;
            this._date = date;
            this._time = time;

            this._xInt = Convert.ToInt32(Math.Round(x));
            this._yInt = Convert.ToInt32(Math.Round(y));

        }

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
