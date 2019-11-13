#include "ExposedWrapper.h"



//Constructor
extern "C" LIBRARY_EXPORT ExposedFunctions* new_ExposedFunctions() {
	return new ExposedFunctions();
}


//Destructor
extern "C" LIBRARY_EXPORT void delete_ExposedFunctions(ExposedFunctions* instance) {
	delete instance;
}
