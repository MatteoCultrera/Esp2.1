// stdafx.h : file di inclusione per file di inclusione di sistema standard
// o file di inclusione specifici del progetto utilizzati di frequente, ma
// modificati raramente
//

#pragma once
#include "targetver.h"

#include	<stdio.h>
#include	<tchar.h>
#include    <stdlib.h>
#include    <inttypes.h>
#include	<ctime>
#include	<string>
#include	<iostream>
#include	<sstream>
#include	<chrono>
#include	<list>
#include	<future>
#include	<thread>
#include	<winsock2.h>
#include	<ws2tcpip.h>
#include	<csignal>
#include	<iostream>
#include	<queue>
#include	<mutex>

#define PORTN "3010"
#define HTONLL(x) ((1==htonl(1)) ? (x) : (((uint64_t)htonl((x) & 0xFFFFFFFFUL)) << 32) | htonl((uint32_t)((x) >> 32)))
#define NTOHLL(x) ((1==ntohl(1)) ? (x) : (((uint64_t)ntohl((x) & 0xFFFFFFFFUL)) << 32) | ntohl((uint32_t)((x) >> 32)))

#pragma comment(lib,"Ws2_32.lib")


