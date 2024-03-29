// Progetto_PDS.cpp: definisce il punto di ingresso dell'applicazione console.
//
#include "stdafx.h"
#include <map>
#include <vector>
#include "Trilateration.h"
#include "Dispositivo.h"
#include "Misura.h"
#include "Board.h"
#include <string>
#include <sstream>
#include <iostream>
#include <fstream>


/* Costruttore da chiamare in fase di setup una volta ottenute tutte le board collegate */

Trilateration::Trilateration(vector<Board>(&boards)) 
{
	_nBoards = boards.size();
	cout << "entering trilateration constructor " << endl;
	for (auto v = boards.begin(); v != boards.end(); v++)
	{
		cout <<"trilateration received this board:"<<" " << v->getMAC() << " " << v->get_posX()<< " " << v->get_posY() << endl;
		schede.insert(pair<string, Board>(v->getMac(), *v));
		MACschede.push_back(v->getMac());
	}
}


/* Da chiamare alla fine dei 60 secondi del server per leggere i dati ricevuti dalle board */
/* Mentre leggo da ogni file elimino i duplicati */
/* Finita la lettura effettuo filtraggio eliminando le schede che non sono presenti in tutti gli N file corrdispondenti alle N boards */

void
Trilateration::scanFile(PacketQueue &pq)
{
	string boardMac;
	string packSeq;
	double rssi;
	string dispositivoMac;
	Dispositivo d_tmp;
	Packet p;
	dispositivi.clear();
	cout << "inside trilateration thread->scan file" << endl;
	while (pq.queueSize() != NULL)
	{
		p = pq.popPacket();
		boardMac = p.get_board_mac();
		packSeq = p.get_seq_ctl();
		rssi= p.get_rssi();
		dispositivoMac = p.get_device_addr();

		cout << "board mac:" << " " << boardMac << " " << "packseq: " << " " << packSeq << " " << "rssi: " << " " << rssi << " "  << "dispositivo mac:" << " " << dispositivoMac << endl;

		it_dispositivi = dispositivi.find(dispositivoMac);
		//p.printPacket();
		d_tmp.inserisciSchede(MACschede);
		Misura m_tmp(boardMac, rssi);
		m_tmp.setSchedaptr(&schede.find(boardMac)->second);
		if (it_dispositivi == dispositivi.end())
		{
			d_tmp.aggiungiMisura(m_tmp);
			d_tmp.inserisciMAC(dispositivoMac);
			dispositivi.insert(std::pair<std::string, Dispositivo>(d_tmp.getMAC(), d_tmp));
		}
		else
			/* non dovrebbe esserci la necessità di mantere tutte le misure per ogni board ma solo la più recente */
			it_dispositivi->second.aggiungiMisura(m_tmp);
	}	//}
	//}
	
	cout << endl << endl << endl;

	for (it_dispositivi = dispositivi.begin(); it_dispositivi != dispositivi.end(); it_dispositivi++)
	{
		cout << "MAC Dispositivo: " << it_dispositivi->second.getMAC() << " " << "Numero misure effettuate: " << it_dispositivi->second.getNumMisure() << endl;
	}

	/* Ho letto tutti gli n file relativi alle n boards */
	/* Adesso bisogna filtrare lasciando solo le board presenti in tutti e tre i file */
	
	for (it_dispositivi = dispositivi.begin(); it_dispositivi != dispositivi.end();)  //elimino le entry che hanno un numero di misure minore del numero di schede (elimina i dispositivi non rilevati da almeno una scheda)
	{
		if (it_dispositivi->second.getNumMisure() < _nBoards)
			dispositivi.erase(it_dispositivi++);
		else
			++it_dispositivi;
	}


	cout << endl << endl << endl;

	for (it_dispositivi = dispositivi.begin(); it_dispositivi != dispositivi.end(); it_dispositivi++)
	{
		cout << "post filtraggio" << endl;
		cout << "MAC Dispositivo: " << it_dispositivi->second.getMAC() << " " << "Numero misure effettuate: " << it_dispositivi->second.getNumMisure() << endl;
	}
}

/* Da chiamare dopo aver letto il file e aver correttamente riempito le strutture richieste per il calcolo delle coordinate */

vector<Dispositivo>&
Trilateration::calcCoords() 
{
	cout << "Trilateration Thread->calcCoords" << endl;
	if (!devicesFound.empty())
		devicesFound.clear();
	cout << "entro ciclo for calccoords" << endl;
	for (it_dispositivi = dispositivi.begin(); it_dispositivi != dispositivi.end(); ++it_dispositivi) //per i dispositivi filtrati(rilevati da tutte le schedine) calcolo le coordinate
	{
		foundMac = it_dispositivi->second.getMAC();
		it_dispositivi->second.calcolaCoordinate();
		x = it_dispositivi->second.getX();
		y = it_dispositivi->second.getY();
		cout << "Coordinate calcolate (double):" << x << " " << y << endl;
		/*creo struttura definitiva da passare a interfaccia */
		// devicesFound.add o che ne so per aggiungere il dispositivo con roundx e roundy
		Dispositivo found;
		found.setPos(x, y);
		found.inserisciMAC(foundMac);
		devicesFound.push_back(Dispositivo(found));
	}

	return devicesFound;
}

/*
int main()
{
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
*/

//the old scanfile
	//string fName;
	//string row;
	//string delimiter;
	//ostringstream oss;
//for (int i = 0; i < _nBoards; i++) {

	/* Apro il file relativo alla board i */
	/*
	int first = 1;
	oss << "file1_" << i << ".txt";
	fName = oss.str();
	ifstream f(fName);

	while (f.good())
	{
		getline(f, row);

		if (first)
		{
			first = 0;
			delimiter = "Mac=";
			boardMac = row.substr(row.find(delimiter) + 4, 17);
		}

		delimiter = "SEQ=";
		packSeq = row.substr(row.find(delimiter) + 4, 4);

		delimiter = "RSSI=";
		rssi = row.substr(row.find(delimiter) + 5, 2);

		delimiter = "ADDR=";
		dispositivoMac = row.substr(row.find(delimiter) + 5, 17);
		*/