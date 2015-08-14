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
#include "renderer.h"
#include "errorhandler.h"
#include "texture.h"
#include <Windows.h>
#include <process.h>
#include <gl/gl.h>
#include <string>

//#define USE_UNIFORM_BUFFER

#define GLFN(_call) _call; CheckForGLErrors(__FILE__, __LINE__, true)
#define GLFN_DONOTBREAK(_call) _call; CheckForGLErrors(__FILE__, __LINE__, false)

void CheckForGLErrors(const char *file, int line, bool breakOnError)
{
    GLenum error = glGetError();
    if (GL_NO_ERROR != error)
    {
        switch (error)
        {
        case GL_INVALID_ENUM:
            ErrorHandler::Report(file, line, "GL invalid enum");
            break;

        case GL_INVALID_VALUE:
            ErrorHandler::Report(file, line, "GL invalid value");
            break;

        case GL_INVALID_OPERATION:
            ErrorHandler::Report(file, line, "GL invalid operation");
            break;

        case GL_STACK_OVERFLOW:
            ErrorHandler::Report(file, line, "GL stack overflow");
            break;

        case GL_STACK_UNDERFLOW:
            ErrorHandler::Report(file, line, "GL stack underflow");
            break;

        case GL_OUT_OF_MEMORY:
            ErrorHandler::Report(file, line, "GL out of memory");
            break;

        default:
            ErrorHandler::Report(file, line, "Unrecognized GL error 0x%x", error);
        }

        if (breakOnError)
        {
            ::DebugBreak();
        }
    }
}

Renderer::Renderer(void *windowHandle)
: mhWindowHandle(windowHandle),
  mhDC(0),
  mhRC(0),
  mhThread(0),
  mpTextureToProcess(0),
  mhTexture(0),
  mbQuitFlag(false)
{
}

void *Renderer::operator new(size_t size)
{
    void *memory = ::malloc(size);
    return memory;
}

void Renderer::operator delete(void *object)
{
    ::free(object);
}

void Renderer::Initialize()
{
    this->CreateContext();

    // create a thread for OpenGL rendering
    unsigned int threadId;
    this->mhThread = reinterpret_cast<HANDLE>(_beginthreadex(
        0,
        0,
        (unsigned (__stdcall *)(void *))threadFunction,
        this,
        0,
        &threadId));
}

void Renderer::Release()
{
    this->Exit();
    this->mhThread = 0;

    this->DestroyContext();
}

void Renderer::CreateContext()
{
    ::HWND hWindow = static_cast< ::HWND>(this->mhWindowHandle);
    ::HDC hDC = ::GetDC(hWindow);
    ::PIXELFORMATDESCRIPTOR pfDescriptor;
    ::ZeroMemory(&pfDescriptor, sizeof(pfDescriptor));
    pfDescriptor.nSize = sizeof(pfDescriptor);
    pfDescriptor.nVersion = 1;
    DWORD flags = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER;
    pfDescriptor.dwFlags = flags;
    pfDescriptor.iPixelType = PFD_TYPE_RGBA;
    pfDescriptor.cColorBits = 8;
    //pfDescriptor.cDepthBits = 24;
    //pfDescriptor.cStencilBits = 8;

    int pixelFormat = ::ChoosePixelFormat(hDC, &pfDescriptor);
    if (0 == pixelFormat)
    {
        ::DWORD errorCode = ::GetLastError();
        REPORTWIN32ERROR("Unable to choose pixel format; error %d, '%s'", errorCode);
        return;
    }

    BOOL result = ::SetPixelFormat(hDC, pixelFormat, &pfDescriptor);
    if (FALSE == result)
    {
        ::DWORD errorCode = ::GetLastError();
        REPORTWIN32ERROR("Unable to set pixel format; error %d, '%s'", errorCode);
        return;
    }

    ::HGLRC hRC = ::wglCreateContext(hDC);
    if (0 == hRC)
    {
        ::DWORD errorCode = ::GetLastError();
        REPORTWIN32ERROR("Unable to create context; error %d, '%s'", errorCode);
        return;
    }

    ::ReleaseDC(hWindow, hDC);

    this->mhDC = hDC;
    this->mhRC = hRC;
}

void Renderer::DestroyContext()
{
    ::HWND hWindow = static_cast< ::HWND>(this->mhWindowHandle);

    ::BOOL result = ::wglDeleteContext(static_cast<HGLRC>(this->mhRC));
    if (FALSE == result)
    {
        ::DWORD errorCode = ::GetLastError();
        REPORTWIN32ERROR("Unable to delete context; error %d, '%s'", errorCode);
        return;
    }

    ::ReleaseDC(hWindow, static_cast< ::HDC>(this->mhDC));
}

void Renderer::Exit()
{
    if (0 != this->mhThread)
    {
        mbQuitFlag = true;
        REPORTERROR("Request thread shutdown");
        WaitForSingleObject(this->mhThread, INFINITE);

        for (;;)
        {
            ::DWORD exitCode;
            ::BOOL result = ::GetExitCodeThread(this->mhThread, &exitCode);
            if (0 == result)
            {
                ::DWORD errorCode = ::GetLastError();
                REPORTWIN32ERROR("Unable to exit the rendering thread; error %d, '%s'", errorCode);
                return;
            }
            else if (STILL_ACTIVE == result)
            {
                REPORTERROR("Thread is still alive");
                continue;
            }
            else
            {
                REPORTERROR1("Thread exit code is %d", result);
                ::CloseHandle(this->mhThread);
                this->mhThread = 0;
                break;
            }
        }
    }
}

// route to the worker thread
void Renderer::threadFunction(void* param)
{
    ((Renderer*)param)->runThread();
}

// rendering loop
void Renderer::runThread()
{
    ::HDC hDC = static_cast< ::HDC>(this->mhDC);

    // set the current RC in this thread
    BOOL result = ::wglMakeCurrent(hDC, static_cast< ::HGLRC>(this->mhRC));
    if (FALSE == result)
    {
        ::DWORD errorCode = ::GetLastError();
        REPORTWIN32ERROR("Unable to set current context; error %d, '%s'", errorCode);
        return;
    }

    const GLubyte *lacVendor = GLFN(::glGetString(GL_VENDOR));
    const GLubyte *lacRenderer = GLFN(::glGetString(GL_RENDERER));
    const GLubyte *lacVersion = GLFN(::glGetString(GL_VERSION));
    REPORTERROR1("GL_VENDOR   = '%s'", lacVendor);
    REPORTERROR1("GL_RENDERER = '%s'", lacRenderer);
    REPORTERROR1("GL_VERSION  = '%s'", lacVersion);

    GLFN(::glGenTextures(1, &this->mhTexture));

    GLFN(::glBindTexture(GL_TEXTURE_2D, this->mhTexture));

    const unsigned char buffer[4] = { 255, 0, 0, 255 };
    GLFN(::glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, 1, 1, 0, GL_RGBA, GL_UNSIGNED_BYTE, buffer));

    // rendering loop
    while(!this->mbQuitFlag)
    {
        this->ProcessRequests();

        ::SwitchToThread();

        GLFN(::glClearColor(0.2f, 0.2f, 0.2f, 0.2f));
        GLFN(::glClear(GL_COLOR_BUFFER_BIT));

        GLFN(::glMatrixMode(GL_PROJECTION));
        GLFN(::glLoadIdentity());

        GLFN(::glMatrixMode(GL_MODELVIEW));
        GLFN(::glLoadIdentity());

        GLFN(::glViewport(0, 0, 512, 512));

        GLFN(::glDisable(GL_CULL_FACE));
        GLFN(::glDisable(GL_DEPTH_TEST));
        GLFN(::glDepthMask(GL_FALSE));
        GLFN(::glDisable(GL_LIGHTING));
        GLFN(::glDisable(GL_BLEND));

        GLFN(::glEnable(GL_TEXTURE_2D));
        GLFN(::glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST));
        GLFN(::glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST));
        GLFN(::glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT));
        GLFN(::glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT));

        GLFN(::glColor3f(1.0f, 1.0f, 1.0f));

        ::glBegin(GL_QUADS);
            ::glTexCoord2f(0.0f, 0.0f);
            ::glVertex2f(-0.5f, -0.5f);
            ::glTexCoord2f(1.0f, 0.0f);
            ::glVertex2f(0.5f, -0.5f);
            ::glTexCoord2f(1.0f, 1.0f);
            ::glVertex2f(0.5f, 0.5f);
            ::glTexCoord2f(0.0f, 1.0f);
            ::glVertex2f(-0.5f, 0.5f);
        GLFN(::glEnd());

        ::SwapBuffers(hDC);
    }

    REPORTERROR("Begin shutting down render thread");

    GLFN(::glDeleteTextures(1, &this->mhTexture));

    // terminate rendering thread
    result = ::wglMakeCurrent(hDC, 0);
    if (FALSE == result)
    {
        ::DWORD errorCode = ::GetLastError();
        REPORTWIN32ERROR("Unable to unset current context; error %d, '%s'", errorCode);
        return;
    }

    _endthread();
    REPORTERROR("Render thread has terminated");
}

void
Renderer::UploadTexture(const TextureHeader &lHeader)
{
    this->mpTextureToProcess = &lHeader;

    while (0 != this->mpTextureToProcess)
    {
        ::SwitchToThread();
    }
}

void
Renderer::ProcessRequests()
{
    if (0 != this->mpTextureToProcess)
    {
        ::glFinish();

        const TextureHeader *lpHeader = this->mpTextureToProcess;

        GLFN(::glBindTexture(GL_TEXTURE_2D, this->mhTexture));
        GLFN(::glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, lpHeader->mu32Width, lpHeader->mu32Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, lpHeader->mpData));

        this->mpTextureToProcess = 0;
    }
}
