﻿using System;
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
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Data.SqlClient;

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
        private MainWindow main;

        private ICommand _errorConnectingDialog = null;
        public ICommand errorConnectingDialog {
            get { return this._errorConnectingDialog; }
            set { this._errorConnectingDialog = value; }
        }

        private string _draggingBoardVisibility;

        public string draggingBoardVisibility {
            get {
                return this._draggingBoardVisibility;
            }
            set {
                if (this._draggingBoardVisibility != value) {
                    this._draggingBoardVisibility = value;
                    NotifyPropertyChanged("draggingBoardVisibility");
                }

            }

        }

        private string _title;
        private string _subtitle;
        private string _numBoards;
        public int boards;
        private int screen;
        private bool _buttonEnabled;
        private ICommand _okButton;
        public int numMac,res;
        public List<string> macList = new List<string>();
        public ServerInterop thr;
        private ManagedObject myObj;
        Thread t,t2;

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

        public void boardConnected(string mac) {

            int boardsConnected = 0;

            foreach (Board b in boardObjs) {
                if (b.MAC != null)
                {
                    if (b.MAC.Equals(mac))
                    {
                        b.connected = true;
                    }
                }

                if (b.connected == true)
                    boardsConnected++;
            }

            if (boardsConnected == boardObjs.ToArray<Board>().Length) {
                Title = "All Boards Connected";
                Subtitle = "Drag them in order into the Grid";
                boardObjs[0].subtitle = "Drag to position";
            }

        }

        public void errorBoard(string mac) {
            String boardName = "";
            foreach (Board b in boardObjs) {
                if (b.MAC.Equals(mac))
                {
                    boardName = b.BoardName;
                    break;
                }

            }
            string errorDialog = "Looks like " + boardName + " didn’t manage to connect. Check if the MAC is correct or try moving the board elsewhere";
            var args = new ErrorEventArgs
            {
                Message = errorDialog
            };
            
            //Raising event to show dialog in the View
            OnErrorConnection(args);

            //Checking dialog Response (useless this time since is always yes)
            if (args.Confirmed)
            {
                screen = 2;
                foreach (Board b in BoardObjs)
                {
                    b.macEditable = true;
                    //give to the server the board mac
                    b.connected = false;
                }
                screen2?.Invoke(this, null);
                checkMacs();
           
            }

            
            //DialogErrorConnecting error = new DialogErrorConnecting("Ops, looks like board not connected");
            //error.ShowDialog();

        }

/*      elimina quando funziona correttamente la checkMacAddr asincrona
        public delegate void ExampleCallback(int[] resArray, int boards, ServerInterop thr); // in order to accept thread returns, we must add this class. do not delete ! 

        public static void ResultCallback(int[] resArray, int boards, ServerInterop thr)
        {
            int greenBoards = 0;
            for (int i = 0; i < boards; i++) { //x matte qui il thread ritorna il valore del checkMacAddress ! 
                if (resArray[i] == 0)
                    Debug.WriteLine("Board " + (i + 1) + " SET RED COLOR");  // board non è connessa al server, devi colorarla di rosso. devi tornare allo screen in cui vengono inseriti tutti i macaddress delle board x modificare gli errati
                else
                {
                    Debug.WriteLine("Board " + (i + 1) + " SET GREEN COLOR"); //board  è connessa al server, devi colorarla di verde
                    greenBoards++;
                }
            }
            if (greenBoards == boards) //all boards are connected
            {
                Thread t = new Thread(new ThreadStart(thr.CreateSetBoard));
                t.Start();
            }
        }*/


        private void okClick(object sender)
        {
            switch (screen) {
                case 1:
                    screen = 2;
/*                  //TO DO LATER- DO NOT DELETE
                    //string badgeNum;
                    // string sql = "SELECT * FROM test"; //SELECT BadgeNum FROM tableOfficeInfo WHERE BadgeNum = @BadgeNum";
                    using (SqlConnection conn = new SqlConnection("Data Source = (local); Initial Catalog = progetto_pds; "+ "Integrated Security=SSPI;User ID=root;Password=1234"))
                    {
                        SqlCommand cmd = new SqlCommand("SELECT * FROM test", conn);
                        //cmd.Parameters.AddWithValue("@BadgeNum", 1);
                        try
                        {
                            conn.Open();
                            SqlDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                                Debug.WriteLine("{0} {1} {2}", reader.GetString(0), reader.GetDouble(1), reader.GetDouble(2));
                            Debug.WriteLine("tutto ok");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
*/

                    numBoards = "";
                    Title = "Insert Boards MAC";
                    Subtitle = "You can also change board's names";
                    NotifyPropertyChanged("colorTextBox");
                    NotifyPropertyChanged("textBoxEnabled");
                    ButtonEnabled = false;
                    for (int i = 1; i <= boards; i++)
                        BoardObjs.Add(new Board("/Resources/Icons/Boards/Board" + i + "N.png", "Board " + i.ToString(), false, i));
                    screen2?.Invoke(this, null);
                    break;
                case 2:
                    screen = 3;
                    foreach (Board b in BoardObjs) {
                        b.macEditable = false;
                    }
                    Title = "Connecting";
                    Subtitle = "";
                    ButtonEnabled = false;
                    screen3?.Invoke(this, null);
                    //Start connecting with the server

                    //nel costruttore del serverInterop setto il costruttore del server e lancio server.dosetup()
                    if(thr == null)
                        thr = new ServerInterop(boards, BoardObjs, this);
                 
                    Thread t1 = new Thread(thr.connectBoards);

                    if (t1.IsAlive)
                    {
                        t1.Abort();
                    }

                    t1.Start();


                    //before launching the server thread we check if it already exists
                    /*if (t != null && !t.IsAlive)
                    {
                        foreach (Board b in BoardObjs)
                        {
                            t = new Thread(new ThreadStart(thr.CheckMacAddr));
                            t.Start();
                            Debug.WriteLine("t created and started");
                        }
                    }
                    else
                        Debug.WriteLine("t already running!");
                    

                    
                    foreach (Board b in BoardObjs)
                    {
                       char[] c = b.MAC.ToCharArray(0,17);
                       thr.myObj.set_board_toCheck(c);
                    }

                    foreach (Board b in BoardObjs)
                    {

                        res = thr.myObj.checkMacAddr();
                        if (res == 0)
                        {
                            boardConnected(b.MAC);
                        }
                        else
                        {
                            errorBoard(b.MAC);
                        }
                    }
                    */
                    break;

                case 3:
                    string s = "";
                    foreach (Board b in BoardObjs) {

                        s += b.BoardName + " " + b.posX + " " + b.posY + "\n";


                    }
                    //ora che le schedine sono connesse e ho inserito le posizioni, lancio t2.serverGo()
                    if (t2 == null)
                    {
                           t2 = new Thread(new ThreadStart(thr.ServerGo));
                           t2.Start();
                            Debug.WriteLine("t2 created and started");
                    }
                    else
                        Debug.WriteLine("t2 already running!");

                    //MessageBox.Show(s);
                    var args = new ErrorEventArgs
                    {
                        Message = ""
                    };
                    setupFinished?.Invoke(this, args);

                    if (args.Confirmed)
                    {
                        //MessageBox.Show("ok");
                        main.setupEnded(BoardObjs.ToList());

                    }
                    else
                    {
                        //MessageBox.Show("no");
                        main.setupEnded(BoardObjs.ToList());
                    }

                    break;

            }
        }



        public bool ButtonEnabled {
            get {
                return this._buttonEnabled;
            }
            set {

                this._buttonEnabled = value;
                this.NotifyPropertyChanged("ButtonEnabled");

            }
        }

        public void roomValidity(bool valid)
        {
            if (valid)
            {
                Title = "All Boards Positioned";
                Subtitle = "";
                ButtonEnabled = true;
            }
            else
            {
                Title = "Room Not Valid";
                Subtitle = "The lines should not intersect";
                ButtonEnabled = false;
            }
            
        }

        public void allPositioned() {
            int i = 0;
            foreach (Board b in BoardObjs) {
                if (b.positioned == true)
                    i++;
            }
            if (i == BoardObjs.ToArray<Board>().Length) {
                Title = "All Boards Positioned";
                Subtitle = "";
                ButtonEnabled = true;
            }
            else
            {
                Title = "All Boards Connected";
                Subtitle = "Drag them in order into the Grid";
                ButtonEnabled = false;
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
                    else if (screen == 1)
                    {
                        boards = 0;
                    }

                    if (boards > 0)
                        ButtonEnabled = true;
                    else
                        ButtonEnabled = false;

                    this.NotifyPropertyChanged("numBoards");
                    numMac = boards;
                }
            }

        }

        public bool macRepeated(string mac) {
            foreach (Board b in boardObjs) {
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

        public bool canDrag() {
            if (Title.Equals("All Boards Connected"))
                return true;
            return false;
        }

        public SetupModel(MainWindow main) {
            this.screen = 1;
            this.Title = "Insert number of boards:";
            this.Subtitle = "Press ok when done";
            this._buttonEnabled = false;
            this._numBoards = "";
            this.boards = 0;
            this.boardObjs = new ObservableCollection<Board>();
            okButton = new RelayCommand(okClick, param => this.ButtonEnabled);
            screen = 1;
            draggingBoardVisibility = "Collapsed";
            this.main = main;
        }

        public void dragNext(int i) {
            BoardObjs[i].subtitle = "Drag to position";
        }


        //Raise Event for Error Dialog

        public event EventHandler<ErrorEventArgs> ErrorConnection;

        protected virtual void OnErrorConnection(ErrorEventArgs args)
        {
            ErrorConnection?.Invoke(this, args);
        }

        public event EventHandler<EventArgs> screen3;
        public event EventHandler<EventArgs> screen2;
        public event EventHandler<ErrorEventArgs> setupFinished;


    }

    public class ErrorEventArgs : EventArgs
    {

        public string Message { get; set; }
        public bool Confirmed { get; set; }
    }



}
