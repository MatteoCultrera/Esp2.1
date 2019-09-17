using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace EspInterface.Models
{
    public class Board : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _boardImgSrc;
        private string _boardName;
        private string _mac;
        private int _boardNum;
        private bool _hasMac;
        private bool _macEditable;
        private bool _connected;
        private string _subtitle;
        private bool _dragging;
        public bool positioned;
        public int posX, posY;
        private bool _unloaded;
        private int _indexLoaded;
        private double _xMeters;
        private double _yMeters;

        public double xMeters
        {
            get { return this._xMeters; }
            set { this._xMeters = value; }
        }

        public double yMeters
        {
            get { return this._yMeters; }
            set { this._yMeters = value; }
        }

        public int indexLoaded
        {
            get { return _indexLoaded; }
            set { this._indexLoaded = value; }
        }


        public string subtitle {
            get {
                return this._subtitle;
            }
            set {

                    this._subtitle = value;
                    NotifyPropertyChanged("subtitle");
                
            }
        }

        public bool unloaded {
            get
            {
                return this._unloaded;
            }
            set {
                this._unloaded = value;
                NotifyPropertyChanged("unloaded");
            }
        }

        public bool dragging {
            get {
                return this._dragging;
            }
            set {
                if (this._dragging != value) {
                    this._dragging = value;
                    NotifyPropertyChanged("dragging");
                    NotifyPropertyChanged("imageVisible");
                }
            }

        }

        public string imageVisible {
            get {
                if (this._dragging == false)
                    return "Visible";
                else
                    return "Collapsed";
            }

        }

        public bool connected {
            get {
                return this._connected;
            }
            set {
                if (this._connected != value) {
                    this._connected = value;
                    NotifyPropertyChanged("connected");
                    if (this._connected == false)
                    {
                        BoardImgSrc = "/Resources/Icons/Boards/Board"+this._boardNum+"N.png";
                    }
                    else {
                        BoardImgSrc = "/Resources/Icons/Boards/Board"+this._boardNum+".png";
                    }

                }
            }

        }

        public string macEditableVisibility
        {
            get
            {
                if (this.macEditable)
                    return "Visible";
                else
                    return "Collapsed";
            }

        }
        public bool macEditable {
            get {
                return this._macEditable;
            }
            set {
                if (this._macEditable != value)
                {
                    this._macEditable = value;
                    NotifyPropertyChanged("macEditable");
                    NotifyPropertyChanged("macEditableVisibility");
                }
               
            }
        }

        public string BoardImgSrc
        {
            get
            {
                return this._boardImgSrc;
            }
            set
            {
                if (this._boardImgSrc != value)
                {
                    this._boardImgSrc = value;
                    NotifyPropertyChanged("BoardImgSrc");
                }
            }
        }
        public string BoardName
        {
            get
            {
                return this._boardName;
            }
            set
            {
                if (this._boardName != value)
                {
                    this._boardName = value;
                    NotifyPropertyChanged("BoardName");
                }
            }
        }
        
        public string BoardNameColor {
            get {
                if (HasMac)
                    return "white";
                else
                    return "#9EA3AA";
            }

        }

        public string MAC
        {
            get
            {
                return this._mac;
            }
            set
            {
                if (this._mac != value)
                {
                    Regex regex = new Regex("^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$");
                    if (regex.IsMatch(value))
                    {
                        this._mac = value;
                        this.HasMac = true;
                        NotifyPropertyChanged("MAC");
                        NotifyPropertyChanged("macGridFirst");
                        NotifyPropertyChanged("macGridSecond");
                        NotifyPropertyChanged("BoardNameColor");
                    }
                    if (value.Equals("")) {
                        this._mac = value;
                        this.HasMac = false;
                        NotifyPropertyChanged("MAC");
                        NotifyPropertyChanged("macGridFirst");
                        NotifyPropertyChanged("macGridSecond");
                        NotifyPropertyChanged("BoardNameColor");
                    }
                }
            }
        }
        public bool HasMac
        {
            get
            {
                return this._hasMac;
            }
            set
            {
                if (this._hasMac != value)
                {
                    this._hasMac = value;
                    NotifyPropertyChanged("HasMac");
                    NotifyPropertyChanged("macGridFirst");
                    NotifyPropertyChanged("macGridSecond");
                }
            }
        }

        public string macGridFirst {
            get {
                if (HasMac)
                {
                    return "Collapsed";
                }
                else {
                    return "Visible";
                }
            }
        }

        public string macGridSecond {
            get {
                if (HasMac)
                {
                    return "Visible";
                }
                else
                {
                    return "Collapsed";
                }
            }
        }

        public int boardNum {
            get
            {
                return this._boardNum;
            }
            set {
                if (this._boardNum != value)
                    this._boardNum = value;
            }
        }

        public Board(string imgSrc, string name, bool mac, int num)
        {
            this.BoardImgSrc = imgSrc;
            this.BoardName = name;
            this.HasMac = mac;
            this._connected = false;
            this._macEditable = true;
            this._boardNum = num;
            this.positioned = false;
            this._dragging = false;
            this._subtitle = "";
            this.unloaded = true;
            this._indexLoaded = -1;
            this._xMeters = 0;
            this._yMeters = 0;
        }

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
