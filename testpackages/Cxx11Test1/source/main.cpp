// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#include <iostream>
#include <vector>
#include <initializer_list>
#include <memory>

template <class T>
struct S
{
    std::vector<T> v;
    S(std::initializer_list<T> l) :
        v(l)
    {
         std::cout << "constructed vector with a " << l.size() << "-element initializer list\n";
    }
};

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

    auto vector = std::make_shared<S<int>>(std::initializer_list<int>{1, 2, 3, 4});
    vector.reset();

    return 0;
}
