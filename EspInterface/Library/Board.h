#pragma once
#include "stdafx.h"

using namespace std;

class Board
{
	SOCKET s;
	uint32_t address;
	string MAC; //mac from server
	string cMac; //mac from interface
	int bPosx;
	int bPosy;


public:
	Board();
	Board(char *cMac, int posx, int posy);
	~Board();
	void printInfo();
	int get_posX();
	int get_posY();
	string getMac(); // get mac from interface 

	void setBoard(SOCKET s, uint32_t address, std::string MAC);
	void setSocket(SOCKET s);
	void setAddress(uint32_t address);
	void setMAC(std::string MAC);
	SOCKET getSocket();
	uint32_t getAddress();
	std::string getMAC(); //get mac from server

};

