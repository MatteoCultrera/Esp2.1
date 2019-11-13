#pragma once
#include "ExposedFunctions.h"

#ifndef LIBRARY_EXPORT

#define LIBRARY_EXPORT _declspec(dllexport)

//Constructor
extern "C" LIBRARY_EXPORT ExposedFunctions* new_ExposedFunctions();

//Destructor
extern "C" LIBRARY_EXPORT void delete_ExposedFunctions(ExposedFunctions* instance);


#endif 

