// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
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

    return 0;
}
