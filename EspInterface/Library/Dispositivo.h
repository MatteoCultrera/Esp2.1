#pragma once
#include <stdio.h>
#include <string>
#include <math.h>
#include <vector>
#include <map>
#include "Board.h"
#include "Misura.h"

class Dispositivo
{
private:

	std::string MACaddr;
	std::vector<std::string> MACschede;
	std::map<std::string,Misura> misure;
	double x, y;

public:

	Dispositivo();
	~Dispositivo() {};
	Dispositivo(const Dispositivo& dis);
	void inserisciMAC(std::string new_MACaddr);
	void setPos(int posx,int posy);
	void aggiungiMisura(Misura new_misura);
	void inserisciSchede(std::vector <std::string>& new_schede);

	Misura getMisura();
	void calcolaCoordinate();

	int getNumMisure();
	const char* getMAC();
	double getX();
	double getY();
};
