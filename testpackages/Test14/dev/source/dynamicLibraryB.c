#include "dynamicLibraryA.h"
#include "dynamicLibraryB.h"

#include <stdio.h>

char *dynamicLibraryBFunction()
{
    static char buffer[256];

#ifdef _MSC_VER
    sprintf_s(buffer, sizeof(buffer), "FromDynamicLibraryB, but also calling '%s'", dynamicLibraryAFunction());
#else
    sprintf(buffer, "FromDynamicLibraryB, but also calling '%s'", dynamicLibraryAFunction());
#endif

    return buffer;
}
