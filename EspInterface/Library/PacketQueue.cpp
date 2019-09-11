#include "stdafx.h"
#include "PacketQueue.h"


PacketQueue::PacketQueue()
{
}


PacketQueue::~PacketQueue()
{
}

void PacketQueue::pushPacket(Packet p) {

	lock_guard<mutex> lm(m);
	q.push(p);
}

Packet PacketQueue::popPacket() {

	lock_guard<mutex> lm(m);
	Packet p = q.front();
	q.pop();
	return p;
}

int PacketQueue::queueSize() {

	lock_guard<mutex> lm(m);
	return q.size();
}

