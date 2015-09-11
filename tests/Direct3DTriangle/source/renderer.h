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

    bool CreateDevice();
    void DestroyDevice();

    bool CreateImmediateModeVertexBuffer();
    void DestroyImmediateModeVertexBuffer();
    void *BeginImmediateMode(const unsigned int lu32Size, unsigned int &lu32Offset);
    void EndImmediateMode();

    bool CreateShaders();
    void DestroyShaders();

    bool CreateTimerQuery();
    void DestroyTimerQuery();
    void StartTimerQuery();
    void EndTimerQuery();
    float GetMillisecondsElapsed();

private:
    // disable copying
    Renderer operator=(const Renderer&);

private:
    uint64 mu64TimerFrequency;
    uint64 mu64StartTime;
    uint64 mu64EndTime;
    void *mhWindowHandle;
    void *mhThread;
    void *mpD3D;
    void *mpD3DDevice;
    void *mpImmediateModeVB;
    void *mpVertexShader;
    void *mpPixelShader;
    void *mpTimerQuery;
    const unsigned int mu32ImmediateModeVBLength;
    unsigned int mu32ImmediateModeOffset;
    bool mbQuitFlag;
};

#endif // RENDERER_H
