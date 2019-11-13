#include "stdafx.h"
#include <string>
#pragma once

class Packet
{
	std::string board_mac;
	uint64_t timestamp;
	uint8_t channel;
	uint8_t seq_ctl[2];
	int8_t rssi;
	uint8_t addr[6];
	uint8_t ssid_length;
	uint8_t ssid[32];
	uint8_t crc[4];

public:
	Packet();
	~Packet();
	void setTimestamp(uint64_t t);
	void setChannel(uint8_t t);
	void setSeqCtl(uint8_t t[2]);
	void setRSSI(int8_t t);
	void setAddr(uint8_t t[6]);
	void setSSIDLength(uint8_t t);
	void setSSID(uint8_t t[32]);
	void setCRC(uint8_t t[4]);
	void setBoardMac(std::string mac);
	void printPacket();
	void printFile(FILE *fd);
	std::string get_board_mac();
	std::string get_device_addr();
	int8_t get_rssi();
	uint64_t get_timestamp();
	std::string get_seq_ctl();
};

