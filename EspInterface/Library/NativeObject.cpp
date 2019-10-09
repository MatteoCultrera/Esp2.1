#include "NativeObject.h"
#include "stdafx.h"
#include <iostream>
#include <fstream>
#include <vector>
#include <string>
#include <sstream>
#include <list>
#include <windows.h> 
#include <stdio.h>
#include <conio.h>
#include <tchar.h>
#include <thread>
#include "Board.h"
#include "Server.h"

#include <map>
#include "Dispositivo.h"
#include "Misura.h"
#include "Scheda.h"
using namespace std;


NativeObject::NativeObject(int value) : value_(value)
{
	counter = 0;
	counterBoardToCheck = 0;
	ofstream myfile;
	myfile.open("example.txt");
	myfile << "I received this value: " << value_;
	myfile.close();
	//SETUP_SERVER
	int result = 0;

	/* Try to setup the server until success */ 
	while (result == 0) {
		server = new Server(value_); //qui passo al server il NUMBER_ESP dell'interfaccia
		pkt = new PacketQueue();
		result = server->doSetup(); //faccio la dosetup, la prossim chiamata al server e la acceptboard, e successivamente la servergo
		ofstream serverlog;
		serverlog.open("serverlog.txt");
		serverlog << result << endl;
	}		
}

NativeObject::~NativeObject()
{
}

int
NativeObject::get_value() 
{
	return value_;
}

void
NativeObject::set_value(int value) 
{
	value_ = value;
}

int
NativeObject::checkMacAddr() 
{
	/*Contact server and check addresses*/
	int result = server->acceptBoard(counterBoardToCheck, boardsVect2);
	counterBoardToCheck++;
	return result;
}

void
NativeObject::getDevices()
{
	vector<Dispositivo>* devices = server->getDevices();
	//qua devo far tornare gli address dei dispositivi e le relative coordinate
}


int
NativeObject::set_board_toCheck(char *macAddr) 
{
	cout << "macAddr len set board to check = " << strlen(macAddr) << endl;
	char cleanMac[18];
	strncpy_s(cleanMac, macAddr, 17);
	cleanMac[17] = '\0';
	cout << "macAddr was: " << macAddr << "and now is: " << cleanMac << endl;
	Board b = Board(cleanMac, 0,0);
	boardsVect2.push_back(b);
	return 1;
}

int
NativeObject::set_board_user(char *macAddr, int posx, int posy) 
{
 //cerca in vettore 2 lo stesso mac addr inserito qui. una volta che lo trovi, copiati il suo valore di s e adr in board b che pushi nel vect1
	cout << "macAddr len set board user = " << strlen(macAddr) << endl;
	char cleanMac[18];
	strncpy_s(cleanMac, macAddr, 17);
	cleanMac[17] = '\0';
	Board bNew = Board(cleanMac, posx, posy);
	cout << "I want to update board with address: " << cleanMac << endl;
	for (Board bOld : boardsVect2)
	{
		cout << bOld.getMAC()<< " " << bOld.getSocket() << " " << bOld.getAddress() << endl;
		if (bOld.getMAC().compare(cleanMac) == 0)
		{
			bNew.setSocket(bOld.getSocket());
			bNew.setAddress(bOld.getAddress());
			cout << "nuova board" << endl << "MAC: " << bNew.getMAC() << endl << "SOCKET: " << bNew.getSocket() << endl << "ADDRESS: " << " " << bNew.getAddress() << endl;
		}
	}
	boardsVect.push_back(bNew);
	return 1;
}

void
NativeObject::serverGo() 
{
	server->serverGo(*pkt,boardsVect);
}

void
NativeObject::printBoardList() 
{
	ofstream myfile2;
	myfile2.open("BOARD_INFO.txt");
	for (auto v : boardsVect) 
	{
		myfile2 << v.getMac() << ", " << v.get_posX() << ", " << v.get_posY() << endl;
	}
}


//TODO HERE:
	/* server launch
		//creo un vettore di board per il server, con le board appena create
			std::vector<Board> boards(value_);
			for each (Board b in BoardObjs) {
				boards[i] = b;
			}

			/* Start Server Functionalities */
			/*			server.serverGo(value_);

						cout << "Closing the server" << std::endl;
						*/

//metodo get position finali per l'interfaccia
//metodo che lancia server 



