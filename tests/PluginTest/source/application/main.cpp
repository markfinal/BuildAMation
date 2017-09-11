/*
Copyright (c) 2010-2017, Mark Final
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

* Neither the name of BuildAMation nor the names of its
  contributors may be used to endorse or promote products derived from
  this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

#include <string>
#include <stdexcept>
#include <iostream>

#ifdef D_BAM_PLATFORM_WINDOWS
#include <Windows.h>
#else
#include <dlfcn.h>
#endif

typedef void(*PluginFunc)();

class Plugin
{
public:
    Plugin(
        const std::string inPath)
    {
#ifdef D_BAM_PLATFORM_WINDOWS
        this->_module = ::LoadLibrary(inPath.c_str());
#else
        this->_module = ::dlopen(inPath.c_str(), RTLD_LAZY);
#endif
        if (0 == this->_module)
        {
            throw std::runtime_error("Failed to load plugin");
        }
    }

    ~Plugin()
    {
#ifdef D_BAM_PLATFORM_WINDOWS
        ::FreeLibrary(this->_module);
#else
        ::dlclose(this->_module);
#endif
        this->_module = 0;
    }

    PluginFunc
    getFunc()
    {
#ifdef D_BAM_PLATFORM_WINDOWS
        PluginFunc func = reinterpret_cast<PluginFunc>(::GetProcAddress(this->_module, "PluginMain"));
#else
        PluginFunc func = reinterpret_cast<PluginFunc>(::dlsym(this->_module, "PluginMain"));
#endif
        if (0 == func)
        {
            throw std::runtime_error("Unable to locate exported function from plugin");
        }
        return func;
    }

private:
#ifdef D_BAM_PLATFORM_WINDOWS
    ::HMODULE _module;
#else
    void *_module;
#endif
};

int main()
{
    try
    {
        Plugin plugin("testPlugin.plugin");
        PluginFunc func = plugin.getFunc();
        func();
        return 0;
    }
    catch (const std::runtime_error &ex)
    {
        std::cerr << "Failed: '" << ex.what() << "'" << std::endl;
        return -1;
    }
}
