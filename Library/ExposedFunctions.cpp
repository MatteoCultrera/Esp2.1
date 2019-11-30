#include "ExposedFunctions.h"
//#include <thread>
//#include <stdio.h>
//#include <stdlib.h>
//#include <iostream>


ExposedFunctions::ExposedFunctions()
{
}


ExposedFunctions::~ExposedFunctions()
{
}

void mainThreadWork()
{

	// main thread loop after setup
	//We should enter here only if the setup went ok
	while (true) {

		//std::cout << "I'm alive" << std::endl;



	}

}

void ExposedFunctions::startBackendThread()
{
	//std::thread t(mainThreadWork);
	//t.detach();

	//std::cout << "Hello";


}

