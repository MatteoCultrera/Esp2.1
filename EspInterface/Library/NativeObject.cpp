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
	int result = server->acceptBoard(boardsVect2);
	return result;
}

void
NativeObject::getDevices()
{
	//qua devo far tornare gli address dei dispositivi e le relative coordinate
}


int
NativeObject::set_board_toCheck(char *macAddr) 
{
	//cout << "macAddr len set board to check = " << strlen(macAddr) << endl;
	char cleanMac[18];
	strncpy_s(cleanMac, macAddr, 17);
	cleanMac[17] = '\0';
	//cout << "macAddr was: " << macAddr << "and now is: " << cleanMac << endl;
	Board b = Board(cleanMac, 0,0);
	boardsVect2.push_back(b);
	return 1;
}

int
NativeObject::set_board_user(char *macAddr, int posx, int posy) 
{
 //cerca in vettore 2 lo stesso mac addr inserito qui. una volta che lo trovi, copiati il suo valore di s e adr in board b che pushi nel vect1
	//cout << "macAddr len set board user = " << strlen(macAddr) << endl;
	char cleanMac[18];
	strncpy_s(cleanMac, macAddr, 17);
	cleanMac[17] = '\0';
	Board bNew = Board(cleanMac, posx, posy);
	//cout << "I want to update board with address: " << cleanMac << endl;
	for (Board bOld : boardsVect2)
	{
		//cout << bOld.getMAC()<< " " << bOld.getSocket() << " " << bOld.getAddress() << endl;
		if (bOld.getMAC().compare(cleanMac) == 0)
		{
			bNew.setSocket(bOld.getSocket());
			bNew.setAddress(bOld.getAddress());
			//cout << "nuova board" << endl << "MAC: " << bNew.getMAC() << endl << "SOCKET: " << bNew.getSocket() << endl << "ADDRESS: " << " " << bNew.getAddress() << endl;
		}
	}
	boardsVect.push_back(bNew);
	return 1;
}

void
NativeObject::serverGo() 
{
	cout << "nativeobj->servergo" << endl;
	server->serverGo(pkt,boardsVect);
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

//int 
//NativeObject::getDevicesAndPos(char* listOfMac, int* listOfPosX, int* listOfPosY, int *nDevices)
//{
	/*
	cout << "entering getDeviceAndPos" << endl;
	string tmp = NULL;
	int i = 0;
	//prendo i valori dei dispositivi trovati alla fine della trilateration
	vector<Dispositivo> &tmpDevices = server->getDevices(); 

	*nDevices = tmpDevices.size();
	
	
	for (Dispositivo dev : tmpDevices) 
	{
		if (i == 0)
			tmp = dev.getMAC(); // alla prima lettura non avrò "," prima del MacAddr del dispositivo
		else
			tmp += ',' + dev.getMAC();
		listOfPosX[i] = dev.getX();
		listOfPosY[i] = dev.getY();
		i++;
	}
	//copio nell'area di memoria puntata da listOfMac la stringa tmp
	strcpy_s(listOfMac, tmp.size()+1 ,tmp.c_str());
	*/
	//return 0;

//}




