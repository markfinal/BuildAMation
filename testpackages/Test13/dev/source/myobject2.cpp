#include "myobject2.h"

#include <stdio.h>

MyClass2::MyClass2(QObject *parent)
: QObject(parent)
{
    printf("MyClass2 instance constructed\n");
}

MyClass2::~MyClass2()
{
    printf("MyClass2 instance destructed\n");
}

void MyClass2::DoSomething()
{
    emit mySignal();
}

void MyClass2::mySlot()
{
    printf("MyClass2 instance slot invoked\n");
}

