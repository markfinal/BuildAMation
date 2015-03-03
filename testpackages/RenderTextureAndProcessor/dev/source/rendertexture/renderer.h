/*
Copyright 2010-2015 Mark Final

This file is part of BuildAMation.

BuildAMation is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

BuildAMation is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
*/
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
