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
        private SetupModel.ExampleCallback callback;
        private ManagedObject myObj;
        private int boards;
        private ObservableCollection<Board> BoardObjs;
        private MemoryMappedFile sharedArea;
        private MemoryMappedViewStream sharedAreaVS;
        private Mutex checkMac;
        private String[] vMacFound;

        private struct sharedData 
        {
            public string MAC;
            public int timeout;

            public sharedData(string MAC, int timeout)
            {
                this.MAC=MAC;
                this.timeout = timeout;
            }
        }

        sharedData mySharedData; 

        public ServerInterop(int boards, ObservableCollection<Board> BoardObjs)
        {
            this.boards = boards;
            this.BoardObjs = new ObservableCollection<Board>(BoardObjs);
            myObj = new ManagedObject(boards); //send number of boards

            sharedArea = MemoryMappedFile.CreateNew("MAC_FOUND", sizeof(char)*18 + sizeof(int));  // ho creato la shared memory per passare i mac trovati
            sharedAreaVS = sharedArea.CreateViewStream();// si apre la sm come se fosse un file(tipo fin o fout, è lo stream)
            mySharedData = new sharedData("FF:FF:FF:FF:FF:FF", 0);//salvo una istanza di tipo sm
            checkMac = new Mutex(false, "MAC_ADDR_MUTEX");
            vMacFound = new String[boards];
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

        
        public void ReadSharedMemoryArea()
        {
            checkMac.WaitOne();
            sharedAreaVS.Read(Encoding.ASCII.GetBytes(mySharedData.MAC), 0, 18);
            sharedAreaVS.Read(Encoding.ASCII.GetBytes(mySharedData.timeout.ToString()), 0, 4);
            checkMac.ReleaseMutex();
        }

        public String GetMacFound() 
        {
            String foundMac = mySharedData.MAC;
            return foundMac;
        }

        public int GetTimeoutStatus()
        {
            return mySharedData.timeout;//se è 1 c'è stato il timeout
        }

        public void CheckMacAdddr()
        {
            string tmp = string.Empty;
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
            callback(resArray, boards, this);
            //x Matte :bisogna aggiungere che sulla base dell'array di int alcune schede diventano verdi altre rosse ! in alternativa (forse è meglio) tornare alla schermata di inserimento di tutte le schedine
        }

        public void CreateSetBoard()
        {
            //send all data togheter
            foreach (Board b in BoardObjs)
            {
                myObj.set_board_user(b.MAC.ToCharArray(0, 17), b.posX, b.posY);
            }
            myObj.printBoardList();
            Debug.WriteLine("All Boards connected. Object Board set. Gonna launch Server.go");
            //to do:
            //myobj.launchserver(servergo)
            //sleep 60 sec
            //call myobj.devicesfound 
            //callback (<list>macdevice,posx,posy) to draw the device found
            //sleep 60
        }
    }
}
