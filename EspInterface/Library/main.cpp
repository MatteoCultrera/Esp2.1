#include "stdafx.h"
#include "Server.h"
#include "Board.h"
#include "PacketQueue.h"

int main()
{
	int number_ESPs = 4;	// ------- retrive number of esp from UI
	int result = 1;
	
	/* This while is needed just to make sure    */
	/* that WSAStartup is inizialized successfully */
	while (result) {

		Server server(number_ESPs);

		result = server.doSetup();

		if (result > 0) {

			// ---------- RETRIEVE MACS FROM UI ----------------
			std::vector<Board> boards(number_ESPs);
			for (int i = 0; i < number_ESPs; i++) {
				boards[i].setBoard(0, 0, "");
			}

			/* ---------- Create the packet queue ---------- */
			PacketQueue packet_queue;

			/* ---------- Start Server Functionalities -------------- */
			server.serverGo(packet_queue, boards);

			std::cout << "Closing the server..." << std::endl;

			return 0;
		}
	}
	
    return 0;
}

