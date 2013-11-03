#ifndef RENDERER_H
#define RENDERER_H

#include <new>
#include <cstddef> // for size_t

struct TextureHeader;

class Renderer
{
public:
    Renderer(void *windowHandle);

    void Initialize();
    void Release();

    void Exit();

    void *operator new(size_t size);
    void operator delete(void *object);

    void UploadTexture(const TextureHeader &lHeader);

protected:
    static void threadFunction(void* param);
    void runThread();

    void CreateContext();
    void DestroyContext();

    void ProcessRequests();

private:
    void *mhWindowHandle;
    void *mhDC;
    void *mhRC;
    void *mhThread;
    const TextureHeader *mpTextureToProcess;
    unsigned int mhTexture;
    bool mbQuitFlag;
};

#endif // RENDERER_H
