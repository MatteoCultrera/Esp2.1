#include "NativeObject.h"
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
#include "Board.h"
using namespace std;



NativeObject::NativeObject(int value) : value_(value)
{
	counter = 0;
	ofstream myfile;
	myfile.open("example.txt");
	myfile << "I received this value: " << value_;
	myfile.close();

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

int
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
	return 0;
}


int
NativeObject::set_board(char *macAddr, int posx, int posy) {
	cBoard b = cBoard(macAddr, posx, posy);
	list1.push_back(b);
	return 1;
}

void
NativeObject::printBoardList() {
	ofstream myfile2;
	myfile2.open("BOARD_INFO.txt");
	for (auto v : list1) {
		myfile2 << v.getMac() << ", " << v.get_posX() << ", " << v.get_posY() << endl;
	}
}




