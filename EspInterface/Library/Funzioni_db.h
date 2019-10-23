#pragma once
#include <mysql.h>
using namespace std;

MYSQL* ConnectDB();
int InsertDB(MYSQL* conn, const char* mac, double pos_x, double pos_y);// , int timestamp);
int SelectDB(MYSQL* conn);