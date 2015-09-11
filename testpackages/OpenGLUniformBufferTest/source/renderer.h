/*
Copyright (c) 2010-2015, Mark Final
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
#ifndef RENDERER_H
#define RENDERER_H

#ifdef _MSC_VER
typedef unsigned __int64 uint64;
#else
#include <stdint.h>
typedef uint64_t uint64;
#endif

#include <new>
#include <cstddef> // for size_t

class Renderer
{
public:
    Renderer(void *windowHandle);

    void Initialize();
    void Release();

    void Exit();

    void *operator new(size_t size);
    void operator delete(void *object);

protected:
    static void threadFunction(void* param);
    void runThread();

    void CreateContext();
    void DestroyContext();

    void InitializeGLEW();
    void ReleaseGLEW();

    void CreateTimerQuery();
    void DestroyTimerQuery();
    void StartTimerQuery();
    void EndTimerQuery();
    uint64 GetTimeElapsed();

    void CreateShaders();
    void DestroyShaders();
    void PrintShaderLog(int handle);

    void SetupFragmentUniformBuffer();

private:
    uint64 mi64TimeElapsed;
    void *mhWindowHandle;
    void *mhDC;
    void *mhRC;
    void *mhThread;
    int mhVertexShader;
    int mhFragmentShader;
    int mhProgram;
    unsigned int miTimerQuery;
    bool mbQuitFlag;
};

#endif // RENDERER_H
