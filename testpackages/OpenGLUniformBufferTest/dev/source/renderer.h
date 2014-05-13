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
