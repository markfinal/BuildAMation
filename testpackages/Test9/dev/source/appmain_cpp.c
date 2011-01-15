#ifdef __cplusplus
extern "C"
{
    #include "library_c.h"
}
#else
#include "library_c.h"
#endif

#include "library_cpp.h"

int main()
{
    try
    {
        int c = CLibraryFunction();
        int cpp = MyClass::CppLibraryFunction();
        return c + cpp;
    }
    catch(...)
    {
        throw;
    }
}
