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

    return 0;
}
