#include "dynamiclibrary.h"
#include "staticlibrary.h"
#include <stdio.h>

int TestFunction()
{
    int a = 5;
    int b = StaticTestFunction(a);
    printf("From a DLL\n");
    printf("From the static library %d & %d\n", a, b);
    return 0;
}
