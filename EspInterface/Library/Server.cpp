#include "stdafx.h"
#include "Server.h"
#include <stdio.h>
#include <windows.h>

using namespace std;

int nameCount = 1;

HANDLE smWrite;
DWORD bytesWritten;
HANDLE mutCheckMac;

bool WriteSharedMemoryArea(string macToSend,int timeout)
{
		smWrite = OpenFileMappingA(FILE_MAP_ALL_ACCESS, TRUE,  "MAC_FOUND");
		WriteFile(smWrite, macToSend, strlen(macToSend), &bytesWritten, NULL);
		WriteFile(smWrite, &timeout, sizeof(int), &bytesWritten, NULL);
		cout << "ho scritto!";
		CloseHandle(smWrite);
		return true;
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

	/*opening mutex */
	mutCheckMac = OpenMutexW(MUTEX_ALL_ACCESS, TRUE, "MAC_ADDR_MUTEX");
}

int Server::serverGo(PacketQueue &pq, vector<Board>(&boards)) {

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
		acceptRes = AcceptConnections(boards, passive_socket, sniffingFlag, secondFlag);

		if (acceptRes > 0) {

			if (acceptRes == 2) {
				/* Must send NO at the first send avaliable */
				value = 0;
			}

			serverRes = serverLoop(boards, firstFlag, secondFlag, value, setUpFlag, sniffingFlag, pq);
			if (serverRes > 0) {
				value = serverRes;
			}

			/* closeConnections*/
			closeConnections(boards);
		}
	}

	return acceptRes;
}

int Server::serverLoop(vector<Board> boards, bool& firstFlag, bool& secondFlag, int value, bool& setUpFlag, bool& sniffingFlag, PacketQueue &pq) {

	int sendRes, recvRes;

	/* Avoids the recv Packet at the first iteration */
	if (firstFlag) {

		/* Send SEND PACKET ACK */
		/* If value = 0, send 'NO'. Otherwise send 'OK' */
		cout << "Sending SEND PACKET ACK";
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
	cout << "Sending GET TIME ACK";
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
	cout << "Receving GOT TIME ACK";
	recvRes = recvAck(boards);

	/* An error has occured */
	if (recvRes == 0) {
		return 0;
	}

	/* Send START SNIFFING ACK */
	cout << "Sending START SNIFFING ACK";
	value = sendAck(boards, recvRes);

	/* In order to enter receiving packet loop part */
	firstFlag = true;
	sniffingFlag = true;
	setUpFlag = false;

	return value;
}

/* I have to send only an ACK so i don't need to do it concurrently */
/*				If value=1 send OK, if value=0 send NO				*/
int Server::sendAck(vector<Board> boards, int value) {

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

		cout << "...done. Sent OK" << endl;
	}
	else {

		for (int i = 0; i < NUMBER_ESP; i++) {

			strncpy(answer, "NO", 2);
			if ((c_write = send(boards[i].getSocket(), answer, 2, 0)) != 2) {
				printf("...failed: %d\n", WSAGetLastError());
				return 0;
			}
		}

		cout << "...done. Sent NO" << endl;
	}

	return 1;
}

/*					Receive ack messagges from boards				*/
int Server::recvAck(vector<Board> boards) {

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
		boards[i].setAddress(0);
		boards[i].setMAC("");
	}
	cout << "...done" << endl;
}

/*						Send the ack message    					*/
int Server::sendA(SOCKET s, int value) {

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
/*							It's BLOCKING						    */
/*		------------------- SUBSTITUTE HERE ------------------		*/	
int Server::AcceptConnections(vector<Board>(&boards), int passive_socket, bool &sniffingFlag, bool &secondFlag) {

	int connections = 0;		/* Count for the connections */
	int attempts = 0;			/* Number of attemps to decide when quit the program */
	int countTimeout = 2;		/* Count for helping discriminating when a board disconnects while sniffing */
	int client_socket;
	struct sockaddr_in caddr;	/* Struct for client address */
	int caddr_length = sizeof(struct sockaddr_in);
	int result = 1;

	/* Try accepting the predicted number of connections */
	while (connections != NUMBER_ESP) {

		cout << "\t\t\t\t\t\tAccepting connections..." << endl;

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
			cout << "\t\t\t\t\t\tAccept fail within " << tval.tv_sec << " seconds" << endl;
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
				cout << "MAC not received. Error: " << WSAGetLastError() << endl;
				closesocket(client_socket);
				errorFlag = 1;
			}
			else {
				cout << "MAC received " << buffer << endl;
				errorFlag = 0;
			}

			/* If the reading of the MAC address has been done succesfully */
			if (!errorFlag) {

				string MAC(buffer);

				/* Check if the connection is not coming from an already saved board */
				for (int i = 0; i < NUMBER_ESP; i++) {
					if (strcmp(boards[i].getMAC().c_str(), MAC.c_str()) == 0) {
						newBoardFlag = 0;
						index = i;
					}
				}

				/* --- CONTROLS FOR THE NEW PART OF MACs --- */
				/* --------------- INSERT HERE ------------- */
				

				/* If it is a new board, save the socket and the address */
				/* PER MARTINA: QUI LA CONNESSIONE E' STATA ACCETTATA, INSERISCI QUI CIO' CHE TI SERVE */
				if (newBoardFlag) {
					showAddr("New connection accepted from: ", &caddr, MAC);
					boards[connections].setSocket(client_socket);
					boards[connections].setAddress(caddr.sin_addr.s_addr);
					boards[connections].setMAC(MAC);
					connections++;
				}
				else {
					showAddr("Connection accepted from: ", &caddr, MAC);
					boards[index].setSocket(client_socket);
				}

				/* Discriminates a board that can detach from electricity while sniffing. Need to do not receive packets */
				if (sniffingFlag) {
					if (countTimeout > 0) {
						cout << "The board has connected before the sniffing time end" << endl;
						result = 2;
					}
				}
			}
		}
	}

	cout << "Server has accepted " << NUMBER_ESP << " connections!" << endl;

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
int Server::recvPacketsSeq(vector<Board> boards, PacketQueue &pq) {

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
	nameCount++;

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

	return 1;
}

/*						Receving MAC from boards					*/
int Server::recvMAC(vector<Board>(&boards)) {

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


/* ----------------------------------- NEW ACCEPT ------------------------------------- */
/* QUESTA SARA' LA ACCEPT FINALE, OVVERO QUANDO AVRO' A DISPOSIZIONE I MAC DELLE SCEHDE */
int AcceptConnectionss(vector<Board>(&boards), int passive_socket, bool &sniffingFlag, bool &secondFlag) {

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
			cout << "Accept fail within " << tval.tv_sec << " seconds" << endl;
			client_socket = 0;
			if (sniffingFlag) {
				countTimeout--;
			}
			attempts++;
			if (attempts == 12) {
				//take mutex
				WriteSharedMemoryArea("FF:FF:FF:FF:FF:FF", 1);
				//release, notify
				return 0;
			}
		}

		if (client_socket == INVALID_SOCKET) {
			cout << "Accept failed with error: " << WSAGetLastError() << endl;
			/* Continue the loop without incrementing the number of ESP connected */
		}
		else if (client_socket > 0) {	/* Connection accepted */

			int newBoardFlag = 1, errorFlag = 0;

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
				cout << "MAC received " << buffer << endl;
				errorFlag = 0;
			}

			/* If the reading of the MAC address has been done succesfully */
			if (!errorFlag) {

				string MAC(buffer);

				/* Check if the connection is not coming from an already saved board */
				for (int i = 0; i < NUMBER_ESP; i++) {
					if (strcmp(boards[i].getMAC().c_str(), MAC.c_str()) == 0) {
						newBoardFlag = 0;
						//take mutex
						if (mutCheckMac.lock())
						{ 
							WriteSharedMemoryArea(boards[i].getMAC, 0);
							//release
							mutCheckMac.unlock();
						}
					}
				}

				/*		Save infos of board accepted	*/
				if (!newBoardFlag) {
					showAddr("New connection accepted from: ", &caddr, MAC);
					boards[connections].setSocket(client_socket);
					boards[connections].setAddress(caddr.sin_addr.s_addr);
					connections++;

					/* ----------- Only for the very first time of the loop -------------- */
					if (secondFlag) {

						// Segnalare al back end che la schedina si è connessa
					}
				}
				else {
					cout << "Incoming connection from an outsider board" << endl;
					closesocket(client_socket);
				}

				/* Discriminates a board that can detach from electricity while sniffing. Need to do not receive packets */
				if (sniffingFlag) {
					if (countTimeout > 0) {
						cout << "The board has connected before the sniffing time end" << endl;
						result = 2;
					}
				}
			}
		}
	}

	cout << "Server has accepted " << NUMBER_ESP << " connections!" << endl;

	return result;
}

