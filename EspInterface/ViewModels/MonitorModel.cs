using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EspInterface.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
namespace EspInterface.ViewModels
{
    public class MonitorModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Board> _boards;
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

        public void setBoardsPositions() {
            
        }

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public MonitorModel()
        {

        }

    }
}
