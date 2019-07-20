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
        private bool _hasMac;
        private bool _macEditable;
        public bool macEditable {
            get {
                return this._macEditable;
            }
            set {
                if (this._macEditable != value)
                {
                    this._macEditable = value;
                    NotifyPropertyChanged("macEditable");
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

        public Board(string imgSrc, string name, bool mac)
        {
            this.BoardImgSrc = imgSrc;
            this.BoardName = name;
            this.HasMac = mac;
            this._macEditable = true;
        }

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
