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

extern "C" LIBRARY_EXPORT int checkMacAddr(NativeObject* instance) {
	return instance->checkMacAddr();
}

extern "C" LIBRARY_EXPORT int set_board_user(NativeObject* instance, char *macAddr, int posx, int posy) {
	return instance->set_board_user(macAddr,posx,posy);
}

extern "C" LIBRARY_EXPORT int set_board_toCheck(NativeObject* instance, char *macAddr){
	return instance->set_board_toCheck(macAddr);
}

extern "C" LIBRARY_EXPORT void serverGo(NativeObject* instance) {
	return instance->serverGo();
}


extern "C" LIBRARY_EXPORT void printBoardList(NativeObject* instance) {
		return instance->printBoardList();
}