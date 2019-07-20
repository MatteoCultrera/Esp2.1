#include "stdafx.h"
#include "Misura.h"
#define TXPower -62
#define N 2
Misura::Misura()
{
	Schedaptr = nullptr;
	rssi = 1.0;
}
Misura::Misura(Board* scheda, double new_rssi)
{
	Schedaptr = scheda;
	rssi = new_rssi;
	distanza = distanza;
	MACaddr = scheda->getMac();
	distanza = pow(10, (TXPower - rssi) / (10 * N));
}

Misura::~Misura()
{
	Schedaptr = nullptr;
}

Misura::Misura(const Misura& misura)
{
	Schedaptr = misura.Schedaptr;
	rssi = misura.rssi;
	distanza = misura.distanza;
	MACaddr = misura.MACaddr;
}

double Misura::getRssi()
{
	return rssi;
}

double Misura::getDistanza()
{
	return distanza;
}
const char* Misura::getMACaddrScheda()
{
	return MACaddr.c_str();
}
Board* Misura::getSchedaPtr()
{
	return Schedaptr;
}

bool Misura::operator<(const Misura& m)const
{
	return distanza < m.distanza;
}


bool Misura::operator>(const Misura& m)const
{
	return m < *this;
}