#include "myobject.h"

#include <stdio.h>

MyClass::MyClass(QObject *parent)
: QObject(parent)
{
    printf("MyClass instance constructed\n");
}

MyClass::~MyClass()
{
    printf("MyClass instance destructed\n");
}

void MyClass::DoSomething()
{
    emit mySignal();
}

void MyClass::mySlot()
{
    printf("MyClass instance slot invoked\n");
}

