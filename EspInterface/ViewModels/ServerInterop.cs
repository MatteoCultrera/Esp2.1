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
        private int res;
        SetupModel instance;


        public ServerInterop(int boards, ObservableCollection<Board> BoardObjs)
        {
            this.boards = boards;
            this.BoardObjs = new ObservableCollection<Board>(BoardObjs);
            myObj = new ManagedObject(boards); //send number of boards
            
            //metto qui la createsetboardtocheck?
            /*foreach (Board b in BoardObjs)
            {
                myObj.set_board_toCheck(b.MAC.ToCharArray(0, 17));
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


        public void CheckMacAddr()
        {

            /* string tmp = string.Empty;
             int y = 0;
             foreach (Board b in BoardObjs)
             {
                 if (y != 0)
                     tmp += ',' + b.MAC;
                 else
                     tmp = b.MAC;
                 y++;
             }
             IntPtr result = myObj.checkMacAddr(tmp.ToCharArray(0, 17 * (y) + y - 1), tmp.Length); //check a single string with all macAdd. returns an array of int with 0 if macAddr not connecter, 1 otherwise
             int[] resArray = new int[boards];
             Marshal.Copy(result, resArray, 0, boards);
             //callback(resArray, boards, this);
             //x Matte :bisogna aggiungere che sulla base dell'array di int alcune schede diventano verdi altre rosse ! in alternativa (forse è meglio) tornare alla schermata di inserimento di tutte le schedine
         
            foreach (Board b in BoardObjs)
            {
                
                res=myObj.checkMacAddr();
                if(res == 0)
                {
                    boardConnected(b.MAC);
                }
                else
                {
                    errorBoard(b.MAC);
                }
            }
            */
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
            Debug.WriteLine("All Boards connected. Object Board set. Gonna launch Server.go");
            myObj.serverGo();
            //to do:
            //sleep 60 sec
            //call myobj.devicesfound 
            //callback (<list>macdevice,posx,posy) to draw the device found
            //sleep 60
        }
    }
}
