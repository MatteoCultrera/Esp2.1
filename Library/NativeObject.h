#pragma once
#include <list> 
#include <iterator> 
#include "Board.h"
using namespace std;

class NativeObject
{
public:
	NativeObject(int value);
	~NativeObject(void);

	int get_value(void);
	void set_value(int value);
	int checkMacAddr(char *macAddr, int size);
	int set_board(char *macAddr, int posx, int posy);
	void printBoardList();
	list<Board> list1;

private:
	int counter;
	int value_;
	char* macAddresses_;
	int cposx;
	int cposy;
	char *MacAddr;
};

