#include "staticlibrary2.h"
#include "staticlibrary1.h"

int StaticLibrary2Function(char c)
{
    return StaticLibrary1Function(c) + 42;
}
