#include "stdafx.h"
#include <algorithm>
#include "Dispositivo.h"

using namespace std;

Dispositivo::Dispositivo()
{
	x = 0.0;
	y = 0.0;
}

Dispositivo::Dispositivo(const Dispositivo& disp)
{
	MACaddr = disp.MACaddr;
	MACschede = disp.MACschede;
	x = disp.x;
	y = disp.y;
	misure = disp.misure;
}

void Dispositivo::inserisciMAC(std::string new_MACaddr)
{
	MACaddr = new_MACaddr;
}

void Dispositivo::aggiungiMisura(Misura new_misura) //Aggiungo una nuova misura solo se viene da una schedina che non ha ancora fornito misure
{													//così mantengo una sola misura per schedina --- ma così facendo non tengo solo la prima trovata nel file?
	for (unsigned int i = 0; i < MACschede.size(); i++)
	{
		std::string tmp = MACschede[i];
		if (strcmp(new_misura.getMACaddrScheda(),tmp.c_str()) == 0)
			if (misure.find(new_misura.getMACaddrScheda()) == misure.end())
			{
				misure.insert(std::pair<const char*, Misura>(new_misura.getMACaddrScheda(), new_misura));
			}
	}
}

void Dispositivo::inserisciSchede(vector<string>& new_schede)   //vettore con Mac schedine
{
	MACschede = new_schede;
}

void Dispositivo::calcolaCoordinate()
{
	vector<Misura> Misure_Forti;		//Da map a vector
	double schedaX = 0.0, schedaY = 0.0;
	double coeff_normalizzazione = 0.0;
	vector <double> pesi(misure.size(), 0.0);
	Misure_Forti.reserve(misure.size());
	for (auto elemento : misure)
	{
		Misure_Forti.push_back(elemento.second);
	}

	if (misure.size() >= 3)
	{
		
		sort(Misure_Forti.begin(), Misure_Forti.end()); //Riordino le misure per forza decrescente
		for (unsigned int i = 0; i < Misure_Forti.size(); i++)
			coeff_normalizzazione += 1.0 / fabs(Misure_Forti[i].getDistanza());
		for (unsigned int i = 0; i < Misure_Forti.size(); i++)
		{
			pesi[i] += 1.0 / (fabs(Misure_Forti[i].getDistanza() * coeff_normalizzazione));
			schedaX = Misure_Forti[i].getSchedaPtr()->get_posX();
			schedaY = Misure_Forti[i].getSchedaPtr()->get_posY();

			this->x += pesi[i] * schedaX;
			this->y += pesi[i] * schedaY;
		}
		
	}
	if (misure.size() == 2)  //Caso particolare, ho solo due schede
	{
		double r1 = Misure_Forti[0].getDistanza(), r2 = Misure_Forti[1].getDistanza();
	
		this->x = ((Misure_Forti[0].getSchedaPtr()->get_posX()*r1) + (Misure_Forti[1].getSchedaPtr()->get_posX()*r2)) / (r1 + r2);
		this->y = ((Misure_Forti[0].getSchedaPtr()->get_posY()*r1) + (Misure_Forti[1].getSchedaPtr()->get_posY()*r2)) / (r1 + r2);

		if (Misure_Forti[0].getSchedaPtr()->get_posX() == Misure_Forti[1].getSchedaPtr()->get_posX())
		{
			this->x = Misure_Forti[0].getSchedaPtr()->get_posX();
		}
		if (Misure_Forti[0].getSchedaPtr()->get_posY() == Misure_Forti[1].getSchedaPtr()->get_posY())
		{
			this->y = Misure_Forti[1].getSchedaPtr()->get_posX();
		}
	}

}

Misura Dispositivo::getMisura() 
{
	//TODO modificare
	Misura m;
	return m;
}

int Dispositivo::getNumMisure()
{
	return misure.size();
}

const char* Dispositivo::getMAC()
{
	return MACaddr.c_str();
}

double Dispositivo::getX()
{
	return x;
}
double Dispositivo::getY()
{
	return y;
}