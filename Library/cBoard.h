#pragma once
#include <list> 
#include <iterator> 
using namespace std;

class cBoard
{
public:
	cBoard(char *cMac, int posx, int posy);
	~cBoard(void);

	void printInfo();
	int get_posX();
	int get_posY();
	string getMac();

private:
	string cMac;
	int bPosx;
	int bPosy;
};
