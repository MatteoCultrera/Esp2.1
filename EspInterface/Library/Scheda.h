#pragma once
#include <string>
#include <cstring>

class Scheda
{
private:
	std::string MACaddr;
	double x, y;

public:
	Scheda();
	Scheda(const Scheda& scheda);
	~Scheda() {};
	void setScheda(const double x, const double y, const std::string& MACaddr);
	const char* getMACaddr();
	double getX();
	double getY();

};
