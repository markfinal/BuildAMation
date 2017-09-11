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

#include <cstdio>

/* specific platform settings */
#if defined(_WIN32)
#define PLUGIN_API_EXPORT __declspec(dllexport)
#define PLUGIN_API_IMPORT __declspec(dllimport)
#elif __GNUC__ >= 4
#define PLUGIN_API_EXPORT __attribute__ ((visibility("default")))
#endif

/* defaults */
#ifndef PLUGIN_API_EXPORT
#define PLUGIN_API_EXPORT /* empty */
#endif
#ifndef PLUGIN_API_IMPORT
#define PLUGIN_API_IMPORT /* empty */
#endif

/* Note: could have used D_BAM_DYNAMICLIBRARY_BUILD, but this demonstrates things better */
#if defined(DYNAMICLIB_SOURCE)
#define DYNAMICLIB_API PLUGIN_API_EXPORT
#else
#define DYNAMICLIB_API PLUGIN_API_IMPORT
#endif

extern "C" DYNAMICLIB_API void PluginMain();

void PluginMain()
{
    printf("Hello World from a Plugin\n");
}

