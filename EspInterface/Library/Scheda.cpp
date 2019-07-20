#include "stdafx.h"

#include "Scheda.h"

using namespace std;

Scheda::Scheda()
{
	this->x = 0.0;
	this->y = 0.0;
}


//mi serve questo costruttore? il resto c'è già tutto analogo in board.cpp
Scheda::Scheda(const Scheda& scheda)
{
	x = scheda.x;
	y = scheda.y;
	MACaddr = scheda.MACaddr;
}

void Scheda::setScheda(const double x, const double y, const std::string& MACaddr)
{
	this->x = x;
	this->y = y;
	this->MACaddr = MACaddr;
}

const char* Scheda::getMACaddr()
{
	return MACaddr.c_str();
}

double Scheda::getX()
{
	return x;
}

double Scheda::getY()
{
	return y;
}