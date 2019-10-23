#include "stdafx.h"
#include <iostream>
#include <sstream>
#include <string>


#include "Funzioni_db.h";

using namespace std;

/*In a nonmultithreaded environment, the call to mysql_library_init() may be omitted, because mysql_init() will invoke it automatically as necessary.
However, mysql_library_init() is not thread-safe in a multithreaded environment, and thus neither is mysql_init(), which calls mysql_library_init().
You must either call mysql_library_init() prior to spawning any threads, or else use a mutex to protect the call,
whether you invoke mysql_library_init() or indirectly through mysql_init(). This should be done prior to any other client library call.*/

MYSQL* ConnectDB()
{
	MYSQL* conn;
	conn = mysql_init(0);
	mysql_real_connect(conn, "localhost", "root", "1234", "progetto_pds", 3306, nullptr, 0);
	return conn;
}

int InsertDB(MYSQL* conn, const char* mac, double pos_x, double pos_y)//, int timestamp)
{
	MYSQL_STMT *stmt;		/*devo gestire il return con errore*/
	MYSQL_BIND bind[3];
	int status = 0;
	stmt = mysql_stmt_init(conn);
	if (!stmt)
		return 1;
	char macstr[18]; 
	strcpy_s(macstr, sizeof(macstr), mac);
	const char* statement = "INSERT INTO test VALUES (?,?,?, NOW())";
	if (mysql_stmt_prepare(stmt, statement, strlen(statement)))
		return 1;

	memset(bind, 0, sizeof(bind));


	bind[0].buffer_type = MYSQL_TYPE_VARCHAR;
	bind[0].buffer = macstr;
	bind[0].buffer_length = strlen(macstr);
	bind[0].is_null = 0;
	bind[0].length = 0;

	bind[1].buffer_type = MYSQL_TYPE_DOUBLE;
	bind[1].buffer = (char*) &pos_x;
	bind[1].is_null = 0;
	bind[1].length = 0;

	bind[2].buffer_type = MYSQL_TYPE_DOUBLE;
	bind[2].buffer = (char*) &pos_y;
	bind[2].is_null = 0;
	bind[2].length = 0;

	/*bind[3].buffer_type = MYSQL_TYPE_LONG;
	bind[3].buffer = (char*) &timestamp;
	bind[3].is_null = 0;
	bind[3].length = 0;*/

	if (mysql_stmt_bind_param(stmt, bind))
		return 1;
	if(mysql_stmt_execute(stmt))
		return 1;
	return 0;
}

int SelectDB(MYSQL* conn) /*alla fine questo ritornerà una lista di oggetti, per ora indica se l'operazione è andata a buon fine*/
{
	MYSQL_ROW row;
	MYSQL_RES *res;
	string query = "SELECT * FROM test";
	const char *q = query.c_str();
	if (!mysql_query(conn, q))
	{
		res = mysql_store_result(conn);
		while (row = mysql_fetch_row(res))
		{
			printf("%s, %s, %s, %s\n", row[0], row[1], row[2], row[3]);
		}
		return 0;
	}
	return 1;

}