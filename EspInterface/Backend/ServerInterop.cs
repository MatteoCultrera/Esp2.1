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
        private int boards;
        private ObservableCollection<Board> BoardObjs;
        public ObservableCollection<Device> DeviceObjs;
        private int res;
        private List<int> nToConnBoards;
        SetupModel instance;

        private char[] listOfMac = null;
        private int[] listOfPosX = null;
        private int[] listOfPosY = null;
        private IntPtr nDevices;


        public ServerInterop(int boards, ObservableCollection<Board> BoardObjs, SetupModel instance)
        {
            this.boards = boards;
            this.BoardObjs = new ObservableCollection<Board>(BoardObjs);
            //myObj = new ManagedObject(boards); //send number of boards
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
                //myObj.set_board_toCheck(c);
            }

            while (nToConnBoards.Count != 0) /*controlla fino a quando non sono connesse tutte le schedine o fino al timeout , evito loop */
            {
                //res = myObj.checkMacAddr();

                /* res -> [0-n] dove n = boards, accendi icona corrispondente */
                /* res -> -1 significa timeout nel server quindi chiama errorBoard() */

                if (res >= 0 && Application.Current != null)
                {
                    instance.boardConnected(BoardObjs[res].MAC);
                    nToConnBoards.Remove(res);
                }
                else if (res == -1 && Application.Current != null)
                {
                    foreach (int i in nToConnBoards) /* chiamo la error per ogni schedina ancora presente nella lista di interi*/
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            instance.errorBoard(BoardObjs[i].MAC);
                        }));

                    }
                    break;
                }
            }
        }

                public void updateDevicePos() //chiamata in serverinterop.servergo
                {
 /*                   int res;
                    char[] delimiter = { ',' };
                    String[] lMac;

                    if (DeviceObjs.Count != 0) // se sto mandando la n-esima lista di dispostivi trovati, pulisco prima la collection precedente
                        DeviceObjs.Clear();

                    res = myObj.getDeviceAndPos(listOfMac, listOfPosX, listOfPosY, nDevices);
                    if (res != 0)
                    {
                        Debug.WriteLine("getDeviceAndPos error");
                    }
                    lMac = (new String(listOfMac)).Split(delimiter);
                    for (int i = 0; i < nDevices.ToInt32(); i++)
                    {
                        Device dev = new Device(lMac[i], listOfPosX[i], listOfPosY[i]);
                        DeviceObjs.Add(dev);
                    }
                   //a questo punto disegno i dispositivi rilevati chiamando un evento presente in monitor, es: 

                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                                //instance.drawDevice();
                            }));

*/
                }


        public void ServerGo()
        {
            //send all data togheter, with real posx,posy
            foreach (Board b in BoardObjs)
            {
                Debug.WriteLine(b.MAC);
                //myObj.set_board_user(b.MAC.ToCharArray(0, 17), b.posX, b.posY);
            }
            //myObj.printBoardList();
            Debug.WriteLine("All Boards connected. Object Board set. Gonna launch Server.go\n\n");
            //myObj.serverGo();
            //to do:
            //sleep 60 sec
            //updateDevicePos();
            //sleep 60
        }
    }

}
