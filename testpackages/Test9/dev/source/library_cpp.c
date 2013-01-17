#include "library_cpp.h"

int MyClass::CppLibraryFunction()
{
    try
    {
        return 37;
    }
    catch (...)
    {
        return -1;
    }
}
