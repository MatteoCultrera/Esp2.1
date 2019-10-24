#define _CRT_SECURE_NO_WARNINGS
#include "stdafx.h"
#include "Server.h"
#include <stdio.h>
#include <windows.h>
#include <mutex>
#include <thread>
#include "Trilateration.h"

using namespace std;

int nameCount = 1;
int NUMBER_ESP;

vector<Dispositivo>* devicesFound = NULL;

HANDLE smWrite;
DWORD bytesWritten;
HANDLE mutCheckMac;

HANDLE sFull;
HANDLE sEmpty;

bool WriteSharedMemoryArea(string macToSend,int timeout)
{
		smWrite = OpenFileMappingA(FILE_MAP_ALL_ACCESS, TRUE,  "MAC_FOUND");
		WriteFile(smWrite, &macToSend, sizeof(&macToSend), &bytesWritten, NULL);
		WriteFile(smWrite, &timeout, sizeof(int), &bytesWritten, NULL);
		cout << "ho scritto!";
		CloseHandle(smWrite);
		return true;
}

void setTrilateration(vector<Board>boards)
{
	//TODO controllare il caso in cui boards sia nullo 

	Trilateration tr= Trilateration(boards);
	vector<Dispositivo>* tmpDevicesFound;

	/* Per adesso infinito ma dovrebbe esserci qualche variabile di controllo per sapere quando va chiuso */
	while (1) 
	{
		/* Aspetto che il file sia pronto */
		WaitForSingleObject(sFull, INFINITE);

		/* Leggo il file */
		tr.scanFile();

		/* Sveglio il server */
		ReleaseSemaphore(sEmpty, 1, NULL);

		/* Calcolo le coordinate dei dispositivi */

		tmpDevicesFound = tr.calcCoords();

		//acquisisco mutex
		devicesFound = tmpDevicesFound;
		//rilascio mutex

	}
	
}

vector<Dispositivo>*
Server::getDevices() 
{
	vector<Dispositivo>* tmpDevices;

	//acquisisco mutex
	tmpDevices = devicesFound;
	//rilascio mutex

	return tmpDevices;
}

Server::Server() {
}

Server::Server(int number)
{
	WSADATA wsaData;
	int Result;

	/* Initializing WinSock version 2.2 */
	Result = WSAStartup(MAKEWORD(2, 2), &wsaData);
	if (Result != 0) {
		cout << "WSAStartup failed with error: " << Result << endl;
		WSResult = 0;
	}

	this->NUMBER_ESP = number;
	
	/*creo i semafori per la trilaterazione*/
	sFull = CreateSemaphore(NULL, 0, 1, NULL);
	sEmpty = CreateSemaphore(NULL, 0, 1, NULL);

}

void handler(int signo) {
	if (signo == SIGABRT)
		cout << "sigabrt received" << endl;
	if (signo == SIGFPE)
		cout << "sigfpe received" << endl;
	if (signo == SIGILL)
		cout << "sigaill received" << endl;
	if (signo == SIGINT)
		cout << "sigint received" << endl;
	if (signo == SIGSEGV)
		cout << "sigsegv received" << endl;
	if (signo == SIGTERM)
		cout << "sigterm received" << endl;

	system("PAUSE");

	return;
}

int Server::doSetup() {

	if (WSResult == 0) {
		return -1;
	}

	signal(SIGABRT, handler);
	signal(SIGFPE, handler);
	signal(SIGILL, handler);
	signal(SIGINT, handler);
	signal(SIGSEGV, handler);
	signal(SIGTERM, handler);

	struct addrinfo *result = nullptr;
	struct addrinfo hints;
	int Result;

	ZeroMemory(&hints, sizeof(hints));
	hints.ai_family = AF_INET;
	hints.ai_socktype = SOCK_STREAM;
	hints.ai_protocol = IPPROTO_TCP;
	hints.ai_flags = AI_PASSIVE;

	/* Server Address, Server Port, struct server address family, struct to use later */
	Result = getaddrinfo(NULL, PORTN, &hints, &result);
	if (Result != 0) {
		cout << "Getaddrinfo failed with error: " << Result << endl;
		return 0;
	}

	/* Creating the socket */
	passive_socket = socket(result->ai_family, result->ai_socktype, result->ai_protocol);
	if (passive_socket == INVALID_SOCKET) {
		cout << "Socket failed with error: " << WSAGetLastError() << endl;
		freeaddrinfo(result);
		return 0;
	}

	/* Set up TCP socket */
	Result = ::bind(passive_socket, result->ai_addr, (int)result->ai_addrlen);
	if (Result == SOCKET_ERROR) {
		cout << "Bind failed with error: " << WSAGetLastError() << endl;
		freeaddrinfo(result);
		return 0;
	}

	freeaddrinfo(result);

	/* Listening for connection */
	Result = listen(passive_socket, SOMAXCONN);
	if (Result == SOCKET_ERROR) {
		cout << "Listen failed with error: " << WSAGetLastError() << endl;
		return 0;
	}
	printf("Listening at socket %d...\n", passive_socket);

	return 1;
}


int Server::serverGo(PacketQueue &pq, vector<Board>(&boards)) {
	//cout << "entering serverGo" << endl;
	
	/*inizio il set della trilaterazione*/
	thread t1(setTrilateration, boards);
	
	/*for (auto v : boards)
	{
		v.printInfo();
		cout  << " " << v.getSocket() << " "<<  v.getAddress() << endl;
	}*/

	int acceptRes = 1, value = 0, serverRes;

	/* firstFlag: determines the entrance for receiving packets */
	/* secondFlag: determine the send of the ACK NO the very first time in the loop */
	/* setUpFlag: set to true if we are in the parallel client setUp */
	/* sniffingFlag: set to true if we are in the parallel client sniffing minute */
	bool firstFlag = false, secondFlag = true, setUpFlag = true, sniffingFlag = false;

	while (acceptRes) {

		/* Accept loop !BLOCKING! */
		/* If returns 0, server must shutdown. Attempts runned out */
		/* If returns 2, only possible while client sniffing, a board has been shut-down and re-connected */
		if(!secondFlag)
			acceptRes = AcceptConnections(boards, passive_socket, sniffingFlag, secondFlag);

		if (acceptRes > 0) {

			if (acceptRes == 2) {
				/* Must send NO at the first send avaliable */
				cout << "value = 0" << endl;
				value = 0;
			}

			serverRes = serverLoop(boards, firstFlag, secondFlag, value, setUpFlag, sniffingFlag, pq);
			if (serverRes > 0) {
				cout << "value =  " << serverRes << endl;
				value = serverRes;
			}

			/* closeConnections*/
			closeConnections(boards);
		}
	}

	return acceptRes;
}

int Server::serverLoop(vector<Board>boards, bool& firstFlag, bool& secondFlag, int value, bool& setUpFlag, bool& sniffingFlag, PacketQueue &pq) {
	
	int sendRes, recvRes;

	/* Avoids the recv Packet at the first iteration */
	if (firstFlag) {

		/* Send SEND PACKET ACK */
		/* If value = 0, send 'NO'. Otherwise send 'OK' */
		cout << "Sending SEND PACKET ACK" <<endl;
		sendRes = sendAck(boards, value);

		if (sendRes == 0 || value == 0) {
			firstFlag = false;
			sniffingFlag = false;
			setUpFlag = true;
			return 1;
		}

		int packetRes;

		/* Recv Packets */
		cout << "Receving Packets.." << endl;

		//if (NUMBER_ESP > 8)
		//	packetRes = recvPacketsConc(boards);
		//else
			packetRes = recvPacketsSeq(boards, pq);

		if (packetRes == 0) {
			firstFlag = false;
			sniffingFlag = false;
			setUpFlag = true;
			return 1;
		}
	}

	/* Reset the flag in case of returns */
	firstFlag = false;
	sniffingFlag = false;
	setUpFlag = true;

	/* Send GET TIME ACK for boards */
	cout << "Sending GET TIME ACK" << endl;
	sendRes = sendAck(boards, value);

	/* The first time i send NO and then exit. To avoid problems at re run server */
	if (secondFlag) {
		secondFlag = false;
		return 1;
	}

	/* An error has occured */
	if (sendRes == 0) {
		return -1;
	}

	/* Receive GOT TIME ACK from boards */
	cout << "Receving GOT TIME ACK" << endl;
	recvRes = recvAck(boards);

	/* An error has occured */
	if (recvRes == 0) {
		return 0;
	}

	/* Send START SNIFFING ACK */
	cout << "Sending START SNIFFING ACK" << endl;
	value = sendAck(boards, recvRes);

	/* In order to enter receiving packet loop part */
	firstFlag = true;
	sniffingFlag = true;
	setUpFlag = false;

	return value;
}

/* I have to send only an ACK so i don't need to do it concurrently */
/*				If value=1 send OK, if value=0 send NO				*/
int Server::sendAck(vector<Board>boards, int value) {

	int c_write;
	char answer[2];

	if (value) {

		for (int i = 0; i < NUMBER_ESP; i++) {

			strncpy(answer, "OK", 2);
			if ((c_write = send(boards[i].getSocket(), answer, 2, 0)) != 2) {
				printf("...failed: %d\n", WSAGetLastError());
				return 0;
			}
		}

		cout << "...done. Sent OK" << endl << endl;;
	}
	else {

		for (int i = 0; i < NUMBER_ESP; i++) {

			strncpy(answer, "NO", 2);
			if ((c_write = send(boards[i].getSocket(), answer, 2, 0)) != 2) {
				printf("...failed: %d\n", WSAGetLastError());
				return 0;
			}
		}
		cout << "...done. Sent NO" << endl << endl;
	}
	return 1;
}

/*					Receive ack messagges from boards				*/
int Server::recvAck(vector<Board>boards) {
	
	int c_read, value = 1;
	char recvbuf[2];

	for (int i = 0; i < NUMBER_ESP; i++) {

		/* Timeout structure */
		fd_set sset;
		struct timeval tval;
		FD_ZERO(&sset);
		FD_SET(boards[i].getSocket(), &sset);
		tval.tv_sec = 10;
		tval.tv_usec = 0;
		int nread = select(FD_SETSIZE, &sset, NULL, NULL, &tval);

		if (nread > 0) {
			if ((c_read = recv(boards[i].getSocket(), recvbuf, 2, 0)) != 2) {
				cout << "Error while reading GOT TIME ACK: " << WSAGetLastError() << endl;
				return 0;
			}
			else if (strncmp(recvbuf, "NO", 2) == 0) {
				cout << "An error has occured in one of the boards. Received 'NO'" << endl;
				return 0;
			}
			else if (strncmp(recvbuf, "OK", 2) != 0) {
				cout << "Received rubbish from one of the boards." << endl;
				value = 0;
			}
		}
		else {
			cout << "No response received after 5 seconds while reading the GOT TIME ack" << endl;
			return 0;
		}
	}
	cout << "...done" << endl;

	return value;
}

/*   Close the sockets and zeros the memory of the boards array     */
void Server::closeConnections(vector<Board>(&boards)) {
	
	cout << "Closing sockets...";
	for (int i = 0; i < NUMBER_ESP; i++) {

		closesocket(boards[i].getSocket());
		boards[i].setSocket(0);
	}
	cout << "...done" << endl;
}

/*						Send the ack message    					*/
int Server::sendA(SOCKET s, int value) {
	cout << "entering sendA" << endl;
	int c_write;

	if (value) {

		for (int i = 0; i < NUMBER_ESP; i++) {

			if ((c_write = send(s, "OK", 2, 0)) != 2) {
				printf("Send Ack failed: %d\n", WSAGetLastError());
				return 0;
			}
		}
	}
	else {

		for (int i = 0; i < NUMBER_ESP; i++) {

			if ((c_write = send(s, "NO", 2, 0)) != 2) {
				printf("Send Ack failed: %d\n", WSAGetLastError());
				return 0;
			}
		}
	}

	return 1;
}

/*		Loops until the server has accepted NUMBER_ESP boards	    */
int Server::AcceptConnections(vector<Board>(&boards), int passive_socket, bool &sniffingFlag, bool &secondFlag) {

	cout << "\nSaved MAC are: " << endl;
	for (auto v : boards)
	{
		v.printInfo();
	}

	int connections = 0;		/* Count for the connections */
	int attempts = 0;			/* Number of attemps to decide when quit the program */
	int countTimeout = 2;		/* Count for helping discriminating when a board disconnects while sniffing */
	int client_socket;
	struct sockaddr_in caddr;	/* Struct for client address */
	int caddr_length = sizeof(struct sockaddr_in);
	int result = 1;

	/* Try accepting the predicted number of connections */
	while (connections != NUMBER_ESP) {

		cout << "Accepting connections..." << endl;

		/* Timeout structure */
		fd_set sset;
		struct timeval tval;
		FD_ZERO(&sset);
		FD_SET(passive_socket, &sset);

		/* If i'm in the setUpPart i need a quik timeout. In the sniffingPart i need it longer */
		if (!sniffingFlag) {
			tval.tv_sec = 5;
		}
		else {

			/* If i'm at the begin of the sniffingPart. With 30 sec, the accept needs to fail 2 times */
			if (countTimeout > 0) {
				tval.tv_sec = 30;
			}
			else {	/* The sniffing minute is already passed, i don't need to wait so long */
				tval.tv_sec = 5;
			}
		}

		tval.tv_usec = 0;
		int nread = select(FD_SETSIZE, &sset, NULL, NULL, &tval);

		if (nread > 0) {
			client_socket = accept(passive_socket, (struct sockaddr*)&caddr, &caddr_length);
		}
		else {
			cout << "\t\tAccept fail within " << tval.tv_sec << " seconds" << endl;
			client_socket = 0;
			if (sniffingFlag) {
				countTimeout--;
			}
			attempts++;
			if (attempts == 12) {
				return 0;
			}
		}

		if (client_socket == INVALID_SOCKET) {
			cout << "Accept failed with error: " << WSAGetLastError() << endl;
			/* Continue the loop without incrementing the number of ESP connected */
		}
		else if (client_socket > 0) {	/* Connection accepted */

			int newBoardFlag = 1, index, errorFlag = 0;

			/* Timeout for the receive */
			int tv = 5000;
			setsockopt(client_socket, SOL_SOCKET, SO_RCVTIMEO, (char*)tv, sizeof(tv));

			/* Receive immediatly the client MAC */
			int cread;
			char buffer[18];
			cread = recv(client_socket, buffer, 18, 0);
			if (cread != 18) {
				cout << "\tMAC not received. Error: " << WSAGetLastError() << endl;
				closesocket(client_socket);
				errorFlag = 1;
			}
			else {
				cout << "\tMAC received " << buffer << endl;
				errorFlag = 0;
			}

			/* If the reading of the MAC address has been done succesfully */
			if (!errorFlag) {

				string MAC(buffer);

				/* Check if the connection is not coming from an already saved board */
				for (int i = 0; i < NUMBER_ESP; i++) {
					//cout << "COMPARING saved MAC: " << boards[i].getMAC() << " and found MAC: " << MAC << endl;
					if (boards[i].getMAC().compare(MAC) == 0) {
						newBoardFlag = 0;
						index = i;
					}
				}				

				if (newBoardFlag) {
					cout << "\tDifferent socket found!" << endl;
					closesocket(client_socket);
				}
				else {
					showAddr("\tConnection accepted from: ", &caddr, MAC);
					boards[index].setSocket(client_socket);
					connections = connections++;

					/* Discriminates a board that can detach from electricity while sniffing. Need to do not receive packets */
					if (sniffingFlag) {
						if (countTimeout > 0) {
							cout << "\tThe board has connected before the sniffing time end" << endl;
							result = 2;
						}
					}
				}
			}
		}
	}

	cout << "Server has accepted " << NUMBER_ESP << " connections!" << endl << endl;

	return result;
}

/*      Shows the address of the host passed among the values       */
void Server::showAddr(const char * s, struct sockaddr_in *a, string MAC)
{
	char buf[INET_ADDRSTRLEN];

	if (inet_ntop(AF_INET, &(a->sin_addr), buf, sizeof(buf)) != NULL)
		cout << s << buf << " with MAC: " << MAC.c_str() << endl;
	else {
		cout << "Failed printing the client address!" << endl;
	}
}

/*					Reciving packet sequentially					*/
int Server::recvPacketsSeq(vector<Board>boards, PacketQueue &pq) {
	
	int res = 1;
	//Sleep(10000);
	/* DELETE WHEN NO MORE NECESSARY */
	FILE *fd;
	string name;

	for (int i = 0; i < NUMBER_ESP && res == 1; i++) {

		/* DELETE WHEN NO MORE NECESSARY */
		name.clear();
		name.append("file");
		name.append(to_string(nameCount));
		name.append("_");
		name.append(to_string(i));
		name.append(".txt");
		fd = fopen(name.c_str(), "w");

		res = recvPseq(boards[i].getSocket(), boards[i].getMAC(), fd, pq);
		if (res == 0) {
			fclose(fd);
			remove(name.c_str());
		}
		else
		{
			fclose(fd);
		}
	}

	/* DELETE WHEN NO MORE NECESSARY */
	//nameCount++;

	return res;
}

/*					Receving packets fragmented    					*/
int Server::recvPseq(SOCKET s, string MAC, FILE *fd, PacketQueue &pq) {
	
	uint32_t numP;
	unsigned char netP[4];
	int cwrite;

	// Set timeout for the recv
	int tv = 30000;
	setsockopt(s, SOL_SOCKET, SO_RCVTIMEO, (char*)tv, sizeof(tv));

	cwrite = recv(s, (char *)netP, 4, 0);
	if (cwrite > 0) {
		if (cwrite != 4) {
		cout << "Number of packet not entirely received! Only: " << cwrite << " bytes" << endl;
		return 0;
		}
	}
	else if (cwrite == 0) {
		cout << "Socket closed by the client" << endl;
		return 0;
	}
	else {
		cout << "Error while receving the number of packets: " << WSAGetLastError() << endl;
		return 0;
	}

	// NumP contains the number of packets
	numP = ntohl(*(uint32_t*)netP);
	cout << "Number of packets: " << numP << endl;

	// Reading the packets
	for (int i = 0; i < numP; i++) {

		char ts[8];
		cwrite = recv(s, ts, sizeof(uint64_t), 0);
		if (cwrite != sizeof(uint64_t)) {
			printf("Error sending timestamp\n");
			std::cout << "Cwrite is " << cwrite << "with error " << strerror(errno) << std::endl;
			return 0;
		}

		char chan;
		cwrite = recv(s,&chan, sizeof(uint8_t), 0);
		if (cwrite != sizeof(uint8_t)) {
			printf("Error sending channel\n");
			std::cout << "Cwrite is " << cwrite << "with error " << strerror(errno) << std::endl;
			return 0;
		}

		char seq[2];
		cwrite = recv(s, seq, 2 * sizeof(uint8_t), 0);
		if (cwrite != 2 * sizeof(uint8_t)) {
			printf("Error sending seq_ctl\n");
			std::cout << "Cwrite is " << cwrite << "with error " << strerror(errno) << std::endl;
			return 0;
		}

		char rs;
		cwrite = recv(s, &rs, sizeof(int8_t), 0);
		if (cwrite != 1) {
			printf("Error sending rssi\n");
			std::cout << "Cwrite is " << cwrite << "with error " << strerror(errno) << std::endl;
			return 0;
		}

		char ad[6];
		cwrite = recv(s, ad, 6 * sizeof(uint8_t), 0);
		if (cwrite != 6 * sizeof(uint8_t)) {
			printf("Error sending address\n");
			std::cout << "Cwrite is " << cwrite << "with error " << strerror(errno) << std::endl;
			return 0;
		}

		char length;
		cwrite = recv(s, (char*)(&length), sizeof(uint8_t), 0);
		if (cwrite != sizeof(uint8_t)) {
			printf("Error sending ssid length\n");
			std::cout << "Cwrite is " << cwrite << "with error " << strerror(errno) << std::endl;
			return 0;
		}

		char ss[32];
		cwrite = recv(s, ss, 32 * sizeof(uint8_t), 0);
		if (cwrite != 32 * sizeof(uint8_t)) {
			printf("Error sending ssid\n");
			std::cout << "Cwrite is " << cwrite << "with error " << strerror(errno) << std::endl;
			return 0;
		}

		char cr[4];
		cwrite = recv(s, cr, 4 * sizeof(uint8_t), 0);
		if (cwrite != 4 * sizeof(uint8_t)) {
			printf("Error sending crc\n");
			std::cout << "Cwrite is " << cwrite << "with error " << strerror(errno) << std::endl;
			return 0;
		}

		Packet p;
		
		uint64_t timestamp;
		timestamp = NTOHLL(*(uint64_t*)ts);
		p.setTimestamp(timestamp);

		uint8_t channel;
		channel = chan;
		p.setChannel(channel);

		uint8_t seq_ctl[2];
		for (int i = 0; i < 2; i++) {
			seq_ctl[i] = seq[i];
		}
		p.setSeqCtl(seq_ctl);

		int8_t rssi;
		rssi = rs;
		p.setRSSI(rssi);

		uint8_t addr[6];
		for (int i = 0; i < 6; i++) {
			addr[i] =  ad[i];
		}
		p.setAddr(addr);

		uint8_t ssid_length;
		ssid_length = length;
		p.setSSIDLength(ssid_length);

		uint8_t ssid[32];
		for (int i = 0; i < 32; i++) {
			ssid[i] = ss[i];
		}
		p.setSSID(ssid);

		uint8_t crc[4];
		for (int i = 0; i < 4; i++) {
			crc[i] = cr[i];
		}
		p.setCRC(crc);

		p.setBoardMac(MAC);

		pq.pushPacket(p);
		
		// DELETE WHEN NO MORE NECESSARY
		p.printFile(fd);
	}
	cout << "\t\tDo stuff" << endl;
	ReleaseSemaphore(sFull, 1, NULL);
	WaitForSingleObject(sEmpty, INFINITE);

	return 1;
}

/*						Receving MAC from boards					*/
int Server::recvMAC(vector<Board>(&boards)) {
	cout << "entering recvMac" << endl;
	int cread;
	int result = 1;
	char buffer[6];

	for (int i = 0; i < NUMBER_ESP && result == 1; i++) {

		cread = recv(boards[i].getSocket(), buffer, 6, 0);
		if (cread != 6) {
			cout << "MAC not received. Error: " << WSAGetLastError() << endl;
			result = 0;
		}
		else {
			cout << "MAC received" << endl;
			string str(buffer);
			boards[i].setMAC(str);
		}
	}
	return result;
}

Server::~Server()
{
	WSACleanup();
	if (passive_socket > 0) {
		closesocket(passive_socket);
	}
}

int Server::acceptBoard(int x,vector<Board>(&boards)) {

	//cout << "Enter acceptBoard" << endl;

	int attempts = 0;									/* Number of attemps to decide when quit the program */
	int client_socket;
	struct sockaddr_in caddr;							/* Struct for client address */
	int caddr_length = sizeof(struct sockaddr_in);
	bool found = false;
	int result = -1;

	/* Timeout structure */
	fd_set sset;
	struct timeval tval;
	FD_ZERO(&sset);
	FD_SET(passive_socket, &sset);
	tval.tv_sec = 5;
	tval.tv_usec = 0;

	while (!found) {

		int nread = select(FD_SETSIZE, &sset, NULL, NULL, &tval);

		if (nread > 0) {
			client_socket = accept(passive_socket, (struct sockaddr*)&caddr, &caddr_length);
		}
		else {
			cout << "Accept fail within " << tval.tv_sec << " seconds" << endl;
			client_socket = 0;
			attempts++;
			if (attempts == 12) {

				cout << "Maximum attempts reached!" << endl << "Shutting down the server...." << endl;
				return -1;
			}
		}

		if (client_socket == INVALID_SOCKET) {
			cout << "Accept failed with error: " << WSAGetLastError() << endl;
		}
		else if (client_socket > 0) {	/* Connection accepted */

			//cout << "\tAccepted conn" << endl;
			int errorFlag = 0;

			/* Timeout for the receive */
			int tv = 5000;
			setsockopt(client_socket, SOL_SOCKET, SO_RCVTIMEO, (char*)tv, sizeof(tv));

			/* Receive immediatly the client MAC */
			int cread;
			char buffer[18];
			cread = recv(client_socket, buffer, 18, 0);
			if (cread != 18) {
				cout << "MAC not received. Error: " << WSAGetLastError() << endl;
				closesocket(client_socket);
				errorFlag = 1;
			}
			else {
				//cout << "\tMAC received " << buffer << endl;
				errorFlag = 0;
			}

			/* If the reading of the MAC address has been done succesfully */
			if (!errorFlag) {
				//cout << "da interfaccia " << boards[x].getMAC() << endl;
				string MAC(buffer);
				//cout << "entering strcmp" << endl;
				
				if (strcmp(boards[x].getMAC().c_str(), MAC.c_str()) == 0)
				{
					//cout << "\t\tFound" << endl;
					showAddr("New connection accepted from: ", &caddr, MAC);
					boards[x].setSocket(client_socket);
					boards[x].setAddress(caddr.sin_addr.s_addr);
					result = 0;
					found = true;
				}
				else {
					//cout << "\t\tNot found" << endl;
					closesocket(client_socket);
				}
			}
		}
	}

	//cout << "Exit acceptBoard" << endl;
	return result;
}