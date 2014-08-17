// Copyright 2010-2014 Mark Final
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
