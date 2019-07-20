using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using EspInterface.Models;

namespace EspInterface.ViewModels
{
    public class RelayCommand : ICommand
    {
        private Action<object> execute;

        private Predicate<object> canExecute;

        private event EventHandler CanExecuteChangedInternal;

        public RelayCommand(Action<object> execute)
            : this(execute, DefaultCanExecute)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            if (canExecute == null)
            {
                throw new ArgumentNullException("canExecute");
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                this.CanExecuteChangedInternal += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
                this.CanExecuteChangedInternal -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute != null && this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }

        public void OnCanExecuteChanged()
        {
            EventHandler handler = this.CanExecuteChangedInternal;
            if (handler != null)
            {
                //DispatcherHelper.BeginInvokeOnUIThread(() => handler.Invoke(this, EventArgs.Empty));
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        public void Destroy()
        {
            this.canExecute = _ => false;
            this.execute = _ => { return; };
        }

        private static bool DefaultCanExecute(object parameter)
        {
            return true;
        }
    }

    public class SetupModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _title;
        private string _subtitle;
        private string _numBoards;
        private int boards;
        private int screen;
        private bool _buttonEnabled;
        private ICommand _okButton;
       
        public string textBoxEnabled {
            get {
                if (screen == 1)
                    return "visible";
                return "collapsed";
            }
        }


        public string colorTextBox {
            get {
                if (screen == 1)
                    return "White";
                return "#3C4149";
                
            }
        }
        private ObservableCollection<Board> BoardObjs;
        public ObservableCollection<Board> boardObjs
        {
            get { return BoardObjs; }
            set {
                if (value != this.BoardObjs)
                    BoardObjs = value;
                this.NotifyPropertyChanged("boardObjs");
            }

        }
        public ICommand okButton
        {
            get
            {
                return _okButton;
            }
            set
            {
                _okButton = value;
            }
        }

        public void checkMacs() {
            Regex regex = new Regex("^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$");
            foreach (Board b in boardObjs) {
                if (b.MAC != null)
                {
                    if (!regex.IsMatch(b.MAC))
                    {
                        Title = "Insert Boards MAC";
                        ButtonEnabled = false;
                        return;
                    }
                }
                if (b.MAC == null) {
                    Title = "Insert Boards MAC";
                    ButtonEnabled = false;
                    return;
                }
            }
            Title = "Position Boards in the room";
            Subtitle = "Press ok when done";
            ButtonEnabled = true;
        }

        private void okClick(object sender)
        {
            switch (screen) {
                case 1:
                    screen = 2;
                    numBoards = "";
                    Title = "Insert Boards MAC";
                    Subtitle = "";
                    NotifyPropertyChanged("colorTextBox");
                    NotifyPropertyChanged("textBoxEnabled");
                    ButtonEnabled = false;
                    for (int i = 1; i <= boards; i++) 
                        BoardObjs.Add(new Board("/Resources/Icons/Boards/BoardN.png", "Board " + i.ToString(), false));

                    break;
                case 2:
                    screen = 3;
                    foreach(Board b in BoardObjs) {
                        b.macEditable = false;
                    }
                    Title = "Connecting";
                    Subtitle="";
                    ButtonEnabled = false;

                    //sending data to c++
                    ManagedObject myObj = new ManagedObject(boards);


                    break;

            }
        }



        public bool ButtonEnabled {
            get {
                return this._buttonEnabled;
            }
            set {
                if (this._buttonEnabled != value) {
                    this._buttonEnabled = value;
                    this.NotifyPropertyChanged("ButtonEnabled");
                }
            }
        }
        public string Title {
            get {
                return this._title;

            }
            set {
                if (this._title != value)
                {
                    this._title = value;
                    this.NotifyPropertyChanged("Title");
                }
            }
        }
        public string Subtitle {
            get {
                return this._subtitle;
            }
            set {
                if (this._subtitle != value)
                {
                    this._subtitle = value;
                    this.NotifyPropertyChanged("Subtitle");
                }
            }
        }
        
        public string numBoards {
            get {
                return this._numBoards;
            }
            set {
                if (this._numBoards != value)
                {
                    this._numBoards = value;
                    int x;

                    if (Int32.TryParse(_numBoards, out x))
                    {
                        boards = x;
                    }
                    else if(screen == 1)
                    {
                        boards = 0;
                    }

                    if (boards > 0)
                        ButtonEnabled = true;
                    else
                        ButtonEnabled = false;

                    this.NotifyPropertyChanged("numBoards");
                }
            }

        }


        public bool macRepeated(string mac) {
            foreach(Board b in boardObjs) {
                if (b.MAC != null)
                {
                    if (b.MAC.Equals(mac))
                        return true;
                }
            }
            return false;

        }

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public SetupModel(){
            this.screen = 1;
            this.Title = "Insert number of boards:";
            this.Subtitle = "Press ok when done";
            this._buttonEnabled = false;
            this._numBoards = "";
            this.boards = 0;
            this.boardObjs = new ObservableCollection<Board>();
            okButton = new RelayCommand(okClick, param => this.ButtonEnabled);
            screen = 1;
        }


    }

   
}
