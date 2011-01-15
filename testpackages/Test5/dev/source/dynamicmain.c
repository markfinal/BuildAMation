#include <stdio.h>
#include "dynamiclibrary.h"
#include "staticlibrary.h"

#if defined(_WIN32)
#ifndef _CONSOLE
#error Must be compiled with _CONSOLE
#endif
#endif // defined(_WIN32)

int main()
{
    int a = 42;
    int b = StaticTestFunction(a);

    printf("Hello world\n");

    TestFunction();

    printf("From application, %d & %d\n", a, b);

    return 0;
}
