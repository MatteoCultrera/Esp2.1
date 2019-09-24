#pragma once
#include <list> 
#include <iterator> 
#include "Board.h"
#include "Server.h"
using namespace std;

class NativeObject
{
public:

	NativeObject(int value);
	~NativeObject(void);

	int get_value(void);
	void set_value(int value);
	int checkMacAddr();
	int set_board_user(char *macAddr, int posx, int posy);
	int set_board_toCheck(char *macAddr);
	void serverGo();
	void printBoardList();
	//list<Board> list1; delete if vector works
	vector<Board> boardsVect;  // vector containing final positions of all boards truly connected
	vector<Board> boardsVect2; //the one we use for the CheckMacAddress, posx and posy of all boards is 0 here
	PacketQueue* pkt;
	Server* server;

private:
	int counter;
	int value_;
	char* macAddresses_;
	int cposx;
	int cposy;
	char *MacAddr;
	int counterBoardToCheck;
};

