using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using EspInterface.Models;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;


namespace EspInterface.ViewModels
{
    
    public class ServerInterop
    {
       // private SetupModel.ExampleCallback callback;
        public ManagedObject myObj;
        private int boards;
        private ObservableCollection<Board> BoardObjs;
        public ObservableCollection<Device> DeviceObjs;
        private int res;
        private List<int> nToConnBoards;
        SetupModel instance;
        


        public ServerInterop(int boards, ObservableCollection<Board> BoardObjs, SetupModel instance)
        {
            this.boards = boards;
            this.BoardObjs = new ObservableCollection<Board>(BoardObjs);
            myObj = new ManagedObject(boards); //send number of boards
            this.instance = instance;
            
            //metto qui la createsetboardtocheck?
            /*foreach (Board b in BoardObjs)
            {
                myObj.set_board_toCheck(b.MAC.ToCharArray(0, 17));
            }*/
        }

        public void connectBoards()
        {
            nToConnBoards = Enumerable.Range(0, boards).ToList();

            foreach (Board b in BoardObjs)
            {
                char[] c = b.MAC.ToCharArray(0, 17);
                myObj.set_board_toCheck(c);
            }

            while (nToConnBoards.Count != 0)
            {
                res = myObj.checkMacAddr();

                /* res -> [0-n] dove n = boards, accendi icona corrispondente */
                /* res -> -1 significa timeout nel server quindi chiama errorBoard() */

                if (res >= 0 && Application.Current != null)
                {
                    instance.boardConnected(BoardObjs[res].MAC);
                    nToConnBoards.Remove(res);
                }
                else if (res == -1 && Application.Current != null)
                {
                    foreach (int i in nToConnBoards)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            instance.errorBoard(BoardObjs[i].MAC);
                        }));
                        break;
                    }
                }
            }

            /*
            foreach (Board b in BoardObjs)
            {

                res = myObj.checkMacAddr();

                if (res == 0 && Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() => {
                        //This will be executed in the main thread
                        instance.boardConnected(b.MAC);
                    }));
               
                }
                else if (Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() => {
                        instance.errorBoard(b.MAC);
                    }));
                    //instance.errorBoard(b.MAC);

                    break;
                }
            }*/
        }

      /*  
       *  VERSIONE CON CALLBACK(PUò SEMPRE SERVIRE PER DOPO
       *  public ServerInterop(SetupModel.ExampleCallback callbackDelegate, int boards, ObservableCollection<Board> BoardObjs)
        {
            callback = callbackDelegate;
            this.boards = boards;
            this.BoardObjs = new ObservableCollection<Board>(BoardObjs);
            myObj = new ManagedObject(boards); //send number of boards
            sharedArea = MemoryMappedFile.CreateNew("MAC_FOUND", sizeof(char)*12);  // ho creato la shared memory per passare i mac trovati
            sharedAreaVS = sharedArea.CreateViewStream();// si apre la sm come se fosse un file
            checkMac = new Mutex(false, "MAC_ADDR_MUTEX");
        }*/
            //this class and its methods are the ones called by the c# thread in setupModel. The Thread runs all server functions ! 


        public void UpdateDevicePos(string MacDevice, int x, int y)
        {
            
        }

        public void ServerGo()
        {
            //send all data togheter, with real posx,posy
            foreach (Board b in BoardObjs)
            {
                Debug.WriteLine(b.MAC);
                myObj.set_board_user(b.MAC.ToCharArray(0, 17), b.posX, b.posY);
            }
            myObj.printBoardList();
            Debug.WriteLine("All Boards connected. Object Board set. Gonna launch Server.go\n\n");
            myObj.serverGo();
            //to do:
            //sleep 60 sec
            //call myobj.devicesfound 
            //callback (<list>macdevice,posx,posy) to draw the device found
            //sleep 60
        }
    }

}
