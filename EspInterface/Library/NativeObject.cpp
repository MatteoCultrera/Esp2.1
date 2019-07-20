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
	ofstream myfile;
	myfile.open("example.txt");
	myfile << "I received this value: " << value_;
	myfile.close();
	//SETUP_SERVER
	int result = 0;

	/* Try to setup the server until success */ 
	while (result == 0) {
		result = server.doSetup();
		ofstream serverlog;
		serverlog.open("serverlog.txt");
		serverlog << result << endl;

	}		
}

NativeObject::~NativeObject()
{
}

int
NativeObject::get_value() {
	return value_;
}

void
NativeObject::set_value(int value) {
	value_ = value;
}

int*
NativeObject::checkMacAddr(char *macAddr, int size) {
	/*Contact server and check addresses*/

	ofstream myfile, libLog;
	libLog.open("LibraryLog.txt");
	myfile.open("MAC_ADDRESSES.txt");
	int i = 0;
	vector<string> vect;

	stringstream ss(macAddr);
	string token;

	while (std::getline(ss, token, ',')) {
		myfile << i << ") " << token << endl;
		i++;
	}
	

	//server.checkmac address here  
	return server.checkMac(macAddr,value_);
	//if checkmac all 1 then server starts
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

	return 0;
}


int
NativeObject::set_board_user(char *macAddr, int posx, int posy) {
	Board b = Board(macAddr, posx, posy);
	list1.push_back(b);
	return 1;
}

//TODO HERE:
//metodo get position finali per l'interfaccia
//metodo che lancia server 

void
NativeObject::printBoardList() {
	ofstream myfile2;
	myfile2.open("BOARD_INFO.txt");
	for (auto v : list1) {
		myfile2 << v.getMac() << ", " << v.get_posX() << ", " << v.get_posY() << endl;
	}
}




