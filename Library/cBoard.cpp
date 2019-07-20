#include <iostream>
#include <fstream>
#include <vector>
#include <string>
#include <sstream>
#include "cBoard.h"
using namespace std;


cBoard::cBoard(char *cMac, int posx, int posy){
		this->cMac = cMac;
		this->bPosx = posx;
		this->bPosy = posy;
	}


cBoard::~cBoard()
{
}

void
cBoard::printInfo()
	{
		cout << "my info are: " << cMac << " , " << bPosx << " , " << bPosy;
	}

int
cBoard::get_posX() {
		return bPosx;
	}

int
cBoard::get_posY() {
		return bPosy;
	}

string
cBoard::getMac() {
		return cMac;
	}

