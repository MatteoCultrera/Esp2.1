// Progetto_PDS.cpp: definisce il punto di ingresso dell'applicazione console.
//

#include "stdafx.h"
#include <map>
#include <vector>
#include "Dispositivo.h"
#include "Misura.h"
#include "Board.h"

//deve diventare classe da usare nel server alla fine dei 60secondi

unsigned int N_SCHEDE;

int main()
{
	N_SCHEDE = 2;
	std::map<std::string, Board> schede;	 
	std::vector<std::string> MACschede;		 
	std::vector <Misura> misure;			
	std::map<std::string, Dispositivo> dispositivi;  
	std::map<std::string, Board>::iterator it_schede;  
	std::map<std::string, Dispositivo>::iterator it_dispositivi; 
	double x, y;
	int roundx, roundy;
	std::string MACdispositivo = "abc";
	Board scheda_tmp;
	Misura misura_tmp;
	Dispositivo dispositivo_tmp;

	//Questo è un esempio per testare il funzionamento con dati fittizi
	schede.insert(std::pair<std::string, Board>(scheda_tmp.getMac(), scheda_tmp)); //scheda1
	MACschede.push_back(scheda_tmp.getMac());
	schede.insert(std::pair<std::string, Board>(scheda_tmp.getMac(), scheda_tmp)); //scheda2
	MACschede.push_back(scheda_tmp.getMac());
	schede.insert(std::pair<std::string, Board>(scheda_tmp.getMac(), scheda_tmp)); //scheda3
	MACschede.push_back(scheda_tmp.getMac());


	dispositivo_tmp.inserisciMAC(MACdispositivo);		
	dispositivo_tmp.inserisciSchede(MACschede);

	it_schede = schede.find("aaa");						 
	if (it_schede != schede.end())  
	{
		misura_tmp = Misura(&it_schede->second, -86.57);   
		it_dispositivi = dispositivi.find(MACdispositivo);   
		if (it_dispositivi == dispositivi.end())
		{
			dispositivo_tmp.aggiungiMisura(misura_tmp);
			dispositivi.insert(std::pair<std::string, Dispositivo>(dispositivo_tmp.getMAC(), dispositivo_tmp));
		}
		else
			it_dispositivi->second.aggiungiMisura(misura_tmp);
	}

	it_schede = schede.find("bbb");
	if (it_schede != schede.end())
	{
		misura_tmp = Misura(&it_schede->second, -90.80);
		it_dispositivi = dispositivi.find(MACdispositivo);  
		if (it_dispositivi == dispositivi.end())
		{
			dispositivo_tmp.aggiungiMisura(misura_tmp);
			dispositivi.insert(std::pair<std::string, Dispositivo>(dispositivo_tmp.getMAC(), dispositivo_tmp));
		}
		else
			it_dispositivi->second.aggiungiMisura(misura_tmp);
	}

	it_schede = schede.find("ccc");
	if (it_schede != schede.end())
	{
		misura_tmp = Misura(&it_schede->second, -86.41);
		it_dispositivi = dispositivi.find(MACdispositivo); 
		if (it_dispositivi == dispositivi.end())
		{
			dispositivo_tmp.aggiungiMisura(misura_tmp);
			dispositivi.insert(std::pair<std::string, Dispositivo>(dispositivo_tmp.getMAC(), dispositivo_tmp));
		}
		else
			it_dispositivi->second.aggiungiMisura(misura_tmp);
	}

	

	for (it_dispositivi = dispositivi.begin(); it_dispositivi != dispositivi.end();)  //elimino le entry che hanno un numero di misure minore del numero di schede (elimina i dispositivi non rilevati da almeno una scheda)
	{
		if (it_dispositivi->second.getNumMisure() < N_SCHEDE) 
			dispositivi.erase(it_dispositivi++);
		else
			++it_dispositivi;
	}

	for (it_dispositivi = dispositivi.begin(); it_dispositivi != dispositivi.end(); ++it_dispositivi) //per i dispositivi filtrati(rilevati da tutte le schedine) calcolo le coordinate
	{
		it_dispositivi->second.calcolaCoordinate();
		x = it_dispositivi->second.getX();
		y = it_dispositivi->second.getY();
		printf("Coordinate calcolate (double): %lf  %lf\n", x, y);
		x += 0.5;
		y += 0.5;
		roundx = round(x);
		roundy = round(y);
		printf("Coordinate calcolate (int): %d  %d\n", roundx, roundy);  //queste vanno poi mandate all'interfaccia

	}

	return 0;
}

