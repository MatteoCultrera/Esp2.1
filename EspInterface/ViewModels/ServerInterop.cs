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

namespace EspInterface.ViewModels
{
    public class ServerInterop
    {
        private SetupModel.ExampleCallback callback;
        private ManagedObject myObj;
        private int boards;
        private ObservableCollection<Board> BoardObjs;

        public ServerInterop(SetupModel.ExampleCallback callbackDelegate, int boards, ObservableCollection<Board> BoardObjs)
        {
            callback = callbackDelegate;
            this.boards = boards;
            this.BoardObjs = new ObservableCollection<Board>(BoardObjs);
            myObj = new ManagedObject(boards); //send number of boards
        }
            //this class and its methods are the ones called by the c# thread in setupModel. The Thread runs all server functions ! 

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
