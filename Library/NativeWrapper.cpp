#include "NativeWrapper.h"

//Constructor
extern "C" LIBRARY_EXPORT NativeObject* new_NativeObject(int value) {
	return new NativeObject(value);
}

//Destructor
extern "C" LIBRARY_EXPORT void delete_NativeObject(NativeObject* instance) {
	delete instance;
}

//Get value

extern "C" LIBRARY_EXPORT int get_value(NativeObject* instance) {
	return instance->get_value();
}

//Set value

extern "C" LIBRARY_EXPORT void set_value(NativeObject* instance, int value) {
	instance->set_value(value);
}

extern "C" LIBRARY_EXPORT int checkMacAddr(NativeObject* instance, char* macAddr, int size) {
	return instance->checkMacAddr(macAddr, size);
}

extern "C" LIBRARY_EXPORT int set_board(NativeObject* instance, char *macAddr, int posx, int posy) {
	return instance->set_board(macAddr,posx,posy);
}

extern "C" LIBRARY_EXPORT void printBoardList(NativeObject* instance) {
		return instance->printBoardList();
}