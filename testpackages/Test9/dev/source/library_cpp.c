#include "library_cpp.h"

int MyClass::CppLibraryFunction()
{
    try
    {
        return 37;
    }
    catch (...)
    {
#ifndef _MSC_VER
        return -1;
#endif
    }
}
