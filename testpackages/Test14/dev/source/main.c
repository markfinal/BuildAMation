#include "dynamicLibraryA.h"
#include "dynamicLibraryB.h"

#include <stdio.h>

int main()
{
    printf("From dynamic library A '%s'\n", dynamicLibraryAFunction());
    printf("From dynamic library B '%s'\n", dynamicLibraryBFunction());

    return 0;
}
