#include "stdafx.h"
#include <map>
#include <vector>
#include "Dispositivo.h"
#include "Misura.h"
#include "Board.h"

class Trilateration
{
private:
	unsigned int _nBoards;
	std::map<std::string, Board> schede;
	std::vector<std::string> MACschede;
	std::vector <Misura> misure;
	std::map<std::string, Dispositivo> dispositivi;
	std::map<std::string, Board>::iterator it_schede;
	std::map<std::string, Dispositivo>::iterator it_dispositivi;
	vector<Dispositivo> devicesFound;
	double x, y;
	int roundx, roundy;
	string foundMac;

public:

	Trilateration(vector<Board>(&boards));
	~Trilateration() {};

	void scanFile();
	vector<Dispositivo>* calcCoords();
	
};