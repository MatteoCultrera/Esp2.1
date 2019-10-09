#include "stdafx.h"
#include "Packet.h"

Packet::Packet()
{
}

Packet::~Packet()
{
}

void Packet::setTimestamp(uint64_t t) {
	timestamp = t;
}

void Packet::setChannel(uint8_t t) {
	channel = t;
}

void Packet::setSeqCtl(uint8_t t[2]) {
	seq_ctl[0] = t[0];
	seq_ctl[1] = t[1];
}

void Packet::setRSSI(int8_t t) {
	rssi = t;
}

void Packet::setAddr(uint8_t t[6]) {
	for (int i = 0; i < 6; i++) {
		addr[i] = t[i];
	}
}

void Packet::setSSIDLength(uint8_t t) {
	ssid_length = t;
}

void Packet::setSSID(uint8_t t[32]) {
	for (int i = 0; i < 32; i++) {
		ssid[i] = t[i];
	}
}

void Packet::setCRC(uint8_t t[4]) {
	for (int i = 0; i < 4; i++) {
		crc[i] = t[i];
	}
}

void Packet::setBoardMac(std::string MAC) {
	this->board_mac = MAC;
}

void Packet::printFile(FILE *fd) {
	
	fprintf(fd, "Board Mac=%s ----> %" PRIu64 " PROBE CHAN=%02d,  SEQ=%02x%02x,  RSSI=%02d, "
		" ADDR=%02x:%02x:%02x:%02x:%02x:%02x,  ",
		board_mac.c_str(),
		timestamp,
		channel,
		seq_ctl[0], seq_ctl[1],
		rssi,
		addr[0], addr[1], addr[2],
		addr[3], addr[4], addr[5]
	);
	fprintf(fd, "SSID=");
	for (int i = 0; i<ssid_length; i++)
		fprintf(fd, "%c", (char)ssid[i]);
	fprintf(fd, "  CRC=");
	for (int i = 0; i<4; i++)
		fprintf(fd, "%02x", crc[i]);
	fprintf(fd, "\n");
}

