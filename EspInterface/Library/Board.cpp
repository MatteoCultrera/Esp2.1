#include "Board.h"
using namespace std;



Board::Board(char *cMac, int posx, int posy) {
	this->bPosx = posx;
	this->bPosy = posy;
	this->s = 0;
	this->address = 0;
	this->MAC = cMac; //mac from server
}

Board::Board()
{
}

Board::~Board()
{
}

void
Board::printInfo()
{
	cout << " " << MAC << endl;
}

int
Board::get_posX() {
	return bPosx;
}

int
Board::get_posY() {
	return bPosy;
}

string
Board::getMac() { //mac from interface
	return this->MAC;
}


void Board::setBoard(SOCKET s, uint32_t address, std::string MAC) {
	this->address = address;
	this->MAC = MAC;
	this->s = s;
}

void Board::setSocket(SOCKET s) {
	this->s = s;
}

void Board::setAddress(uint32_t address) {
	this->address = address;
}

void Board::setMAC(std::string MAC) {
	this->MAC = MAC;
}

SOCKET Board::getSocket() {
	return this->s;
}

uint32_t Board::getAddress() {
	return this->address;
}

std::string Board::getMAC() {
	return MAC;
}

/* Per programma dinamico */  //dal server di fede 
/*void Board::setConnected(bool value) {
this->connected = value;
}

void Board::setRemovable(bool value) {
this->removable = value;
}

bool Board::isConnected() {
return connected;
}

bool Board::isRemovable() {
return removable;
}*/
