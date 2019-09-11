#pragma once
#include "stdafx.h"
#include "Packet.h"

using namespace std;

class PacketQueue
{
	mutex m;
	queue<Packet> q;

public:
	PacketQueue();
	void pushPacket(Packet p);
	Packet popPacket();
	int queueSize();
	~PacketQueue();
};

