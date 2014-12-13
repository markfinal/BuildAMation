#include <iostream>

int main()
{
    // auto variables
    auto integer = 42;
    auto string = "My text";
    std::cout << "Auto integer value is " << integer << std::endl;
    std::cout << "Auto string value is '" << string << "'" << std::endl;

    // lambda functions
    auto func = [] () { std::cout << "Hello world, from a lambda function" << std::endl; };
    func();

    char *pointer = nullptr;
    if (nullptr == pointer)
    {
        std::cout << "Is a null pointer" << std::endl;
    }
    else
    {
        std::cout << pointer << " is not a null pointer" << std::endl;
    }

	std::cout << "C++ " << __cplusplus << std::endl;

    return 0;
}
