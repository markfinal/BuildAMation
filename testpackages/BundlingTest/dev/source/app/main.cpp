// main.cpp
#include <iostream>
#include "dynlib/lib.h"

int main()
{
    std::cout << "Hello world from the application" << std::endl;
    std::cout << "From the dynamic library: '" << LibraryFunction() << std::endl;
    return 0;
}
