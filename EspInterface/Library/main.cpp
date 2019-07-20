#include "stdafx.h"
#include "Server.h"
#include <iostream>
#include <thread>
#include <fstream>
#include <vector>
#include <string>
#include <sstream>
#include <list>
#include <windows.h> 
#include <stdio.h>
#include <conio.h>
#include <tchar.h>
#include <sstream>
#include "Board.h"
using namespace std;

int main()
{
	//retrive number of esp from UI
	int number_esp = 2, result = 0;

	/* This while is needed just to make sure    */
	/* that WSAStartup is inizialized succesflly */
	while (result >= 0) {

		Server server;

		/* Try to setup the server until success */
		while (result == 0) {

			result = server.doSetup();
		}

		if (result > 0) {

						// Retrieve MACs from UI
						std::vector<Board> boards(NUMBER_ESP);
						for (int i = 0; i < NUMBER_ESP; i++) {
							boards[i].setBoard(0, 0, "");
						}

						/* Start Server Functionalities */
									server.serverGo(number_esp);

									cout << "Closing the server" << std::endl;

									return 0;
						}
		}
						
			return 0;
}