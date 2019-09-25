#pragma once
#include "PacketQueue.h"
#include "Board.h"
#include "Packet.h"

class Server
{

	SOCKET passive_socket;
	int WSResult = 1;
	int NUMBER_ESP;

	void showAddr(const char * s, struct sockaddr_in *a, string MAC);
	int AcceptConnections(vector<Board>(&boards), int passive_socket, bool &sniffingFlag, bool &secondFlag);
	void closeConnections(vector<Board>(&boards));
	int sendAck(vector<Board> boards, int value);
	int sendA(SOCKET s, int value);
	int recvAck(vector<Board> boards);
	int recvPacketsSeq(vector<Board> boards, PacketQueue &pq);
	int recvPseq(SOCKET s, string MAC, FILE *fd, PacketQueue &pq);
	int serverLoop(vector<Board> boards, bool& firstFlag, bool& secondFlag, int value, bool& setUpFlag, bool& sniffingFlag, PacketQueue &pq);
	int recvMAC(vector<Board>(&boards));
	
public:
	Server();
	Server(int number);
	~Server();
	int doSetup();
	int serverGo(PacketQueue &pq, vector<Board>(&boards));
	int acceptBoard(int x, vector<Board>(&boards));
};

