using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace EspInterface.Models
{
    public class Device : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string Mac;
        public int posX, posY;



        public int setPosx
        {
            get { return posX; }
            set { this.posX = value; }
        }

        public int setPosy
        {
            get { return posY; }
            set { this.posY = value; }
        }


        public string MAC
        {
            get
            {
                return this.Mac;
            }
            set
            {
                if (this.Mac != value)
                {
                    Regex regex = new Regex("^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$");
                    if (regex.IsMatch(value))
                    {
                        this.Mac = value;
                    }
                }
                
            }
        }


        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
