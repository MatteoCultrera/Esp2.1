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
		result = server.doSetup(); //faccio la dosetup, la prossim chiamata al server e la acceptboard, e successivamente la servergo
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
	int result = server.acceptBoard(counterBoardToCheck, boardsVect2);
	counterBoardToCheck++;
	return result;
}


int
NativeObject::set_board_toCheck(char *macAddr) 
{
	Board b = Board(macAddr, 0,0);
	boardsVect2.push_back(b);
	return 1;
}

int
NativeObject::set_board_user(char *macAddr, int posx, int posy) 
{
	Board b = Board(macAddr, posx, posy);
	boardsVect.push_back(b);
	return 1;
}

void
NativeObject::serverGo() 
{
	server.serverGo(pkt,boardsVect);
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



