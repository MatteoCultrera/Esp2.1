#pragma once
class Server
{
	
	SOCKET passive_socket;
	int WSResult = 1;

public:
	Server();
	~Server();
	int doSetup();
	int *checkMac(char* macString, int numBoard);
	int serverGo(const int number);
};

