#pragma once
#include "ExposedFunctions.h"

#ifndef LIBRARY_EXPORT

#define LIBRARY_EXPORT _declspec(dllexport)

//Constructor
extern "C" LIBRARY_EXPORT ExposedFunctions* new_ExposedFunctions();

//Destructor
extern "C" LIBRARY_EXPORT void delete_ExposedFunctions(ExposedFunctions* instance);

//Start Main Thread
extern "C" LIBRARY_EXPORT void startMainThread(ExposedFunctions* instance);


#endif 
