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
