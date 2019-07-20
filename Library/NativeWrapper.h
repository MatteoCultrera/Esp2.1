#pragma once
#include "NativeObject.h";

#define LIBRARY_EXPORT _declspec (dllexport)

//Constructor

extern "C" LIBRARY_EXPORT NativeObject* new_NativeObject(int value);

//Destructor

extern "C" LIBRARY_EXPORT void delete_NativeObject(NativeObject* instance);

//Get value function

extern "C" LIBRARY_EXPORT int get_value(NativeObject* instance);

//Set value function

extern "C" LIBRARY_EXPORT void set_value(NativeObject* instance, int value);

//get char(MACID)
extern "C" LIBRARY_EXPORT int checkMacAddr(NativeObject* instance, char *macAddr, int size);

//set board(macAdd,posx,posy)
extern "C" LIBRARY_EXPORT int set_board(NativeObject* instance, char *macAddr, int posx, int posy);

//set printBoardList
extern "C" LIBRARY_EXPORT  void printBoardList(NativeObject* instance);