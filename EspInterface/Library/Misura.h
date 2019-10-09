#pragma once
#include "Board.h"

class Misura
{
private:
	std::string  MACaddr;     // identifico la scheda che ha fatto la misura
	Board*      Schedaptr;    // puntatore alla scheda che ha fatto la misura
	double       distanza;
	double       rssi;

public:
	Misura();
	Misura(string schedaMac, double new_rssi);
	~Misura();
	Misura(const Misura& misura);

	double getRssi();
	double getDistanza();
	double getRssi();
	const char* getMACaddrScheda();
	Board* getSchedaPtr();

	bool operator<(const Misura& m)const;
	bool operator>(const Misura& m)const;

};
