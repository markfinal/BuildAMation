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
#include "application.h"
#include "renderer.h"
#include "errorhandler.h"
#include "common.h"
#include "socket.h"
#include "texture.h"

#include <Windows.h>
#include <string>

#define DEFAULT_PORT "8888"

std::string ssWindowClassName("Test Window Class");
::HWND shMainWindow = 0;

static ::LRESULT CALLBACK WindowProc(::HWND hWnd, ::UINT Msg, ::WPARAM wParam, ::LPARAM lParam)
{
    Application *lpApplication = Application::GetInstance();
    switch (Msg)
    {
    case WM_CREATE:
        {
            Renderer *renderer = new Renderer(hWnd);
            renderer->Initialize();
            lpApplication->SetRenderer(renderer);

            int li32SocketVersion = MAKEWORD(1, 1);
            Networking::Socket::Initialize(li32SocketVersion);
        }
        break;

    case WM_DESTROY:
        {
            Networking::Socket::Release();

            Renderer *lpRenderer = lpApplication->GetRenderer();
            if (lpRenderer != 0)
            {
                lpRenderer->Release();
                delete lpRenderer;
                lpApplication->SetRenderer(0);
            }
        }
        break;

    case WM_CLOSE:
        {
            Renderer *lpRenderer = lpApplication->GetRenderer();
            if (0 != lpRenderer)
            {
                lpRenderer->Exit();
            }

            REPORTERROR("Sending quit message");
            ::PostQuitMessage(0);
        }
        return 0;

    case WM_DROPFILES:
        {
            ::HDROP hDrop = reinterpret_cast< ::HDROP>(wParam);
            ::UINT luFilesDropped = ::DragQueryFile(hDrop, 0xFFFFFFFF, 0, 0);
            REPORTERROR1("There were %d files dropped", luFilesDropped);

            if (1 == luFilesDropped)
            {
                ::UINT luBufferSize = ::DragQueryFile(hDrop, 0, 0, 0);
                luBufferSize += 1;
                char *lpBuffer = new char[luBufferSize];

                ::UINT luResult = ::DragQueryFile(hDrop, 0, lpBuffer, luBufferSize);
                if (0 == luResult)
                {
                    ::DWORD luErrorCode = ::GetLastError();
                    REPORTWIN32ERROR("DragQueryFile failed", luErrorCode);
                }

                struct addrinfo hints;
                ::ZeroMemory(&hints, sizeof(hints));

                hints.ai_family = AF_UNSPEC;
                hints.ai_socktype = SOCK_STREAM;
                hints.ai_protocol = IPPROTO_TCP;

#if 1
                Networking::Socket mySocket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

                struct hostent *host = ::gethostbyname("localhost");

                ::SOCKADDR_IN SockAddr;
                SockAddr.sin_port = ::htons(8888);
                SockAddr.sin_family = AF_INET;
                SockAddr.sin_addr.s_addr = *((unsigned long *)host->h_addr);

                bool lbResult = mySocket.Connect(&SockAddr, sizeof(SockAddr));
#else
                // Resolve the server address and port
                struct addrinfo *result;
                int iResult = ::getaddrinfo("127.0.0.1", DEFAULT_PORT, &hints, &result);
                if ( iResult != 0 )
                {
                    REPORTWIN32MODULEERROR(::GetModuleHandle("WS2_32.dll"), "Failed to getaddrinfo, error %d, '%s'", iResult);
                }

                //::hostent *lpLocalHost = ::gethostbyname("localhost");

                Networking::Socket mySocket(result->ai_family, result->ai_socktype, result->ai_protocol);
                bool lbResult = mySocket.Connect(result->ai_addr, result->ai_addrlen);

                ::freeaddrinfo(result);
#endif

                if (lbResult)
                {
                    lbResult = mySocket.Send(lpBuffer, luBufferSize);
                }
                if (lbResult)
                {
                    TextureHeader textureHeader;
                    int liSize;
                    lbResult = mySocket.Receive(&textureHeader, liSize);
                    if (lbResult)
                    {
                        REPORTERROR3("Texture %dx%d of size %d", textureHeader.mu32Width, textureHeader.mu32Height, textureHeader.mu32TotalTextureDataSize);

                        unsigned char *lpTextureData = new unsigned char[textureHeader.mu32TotalTextureDataSize];
                        lbResult = mySocket.Receive(lpTextureData, liSize);

                        textureHeader.mpData = lpTextureData;

                        Renderer *lpRenderer = lpApplication->GetRenderer();
                        if (0 != lpRenderer)
                        {
                            lpRenderer->UploadTexture(textureHeader);
                        }

                        delete [] lpTextureData;
                    }
                }

                delete [] lpBuffer;
            }
        }
        break;
    }

    return ::DefWindowProc(hWnd, Msg, wParam, lParam);
}

Application *Application::spInstance = 0;
Application *Application::GetInstance()
{
    return spInstance;
}

Application::Application(int UNUSEDARG(argc), char *UNUSEDARG(argv)[])
: mpRenderer(0), mhWin32Instance(0), mi32ExitCode(0)
{
    if (0 != spInstance)
    {
        REPORTERROR("There is already an instance of the application running");
        return;
    }

    spInstance = this;
}

void Application::SetWin32Instance(void *instance)
{
    mhWin32Instance = instance;
}

int Application::Run()
{
    this->RegisterWindowClass();
    this->CreateMainWindow();
    this->MainLoop();
    this->DestroyMainWindow();
    this->UnregisterWindowClass();
    return this->mi32ExitCode;
}

void Application::RegisterWindowClass()
{
    ::WNDCLASSEX windowClass;
    ::ZeroMemory(&windowClass, sizeof(windowClass));
    windowClass.cbSize = sizeof(windowClass);
    windowClass.hInstance = static_cast< ::HINSTANCE>(this->mhWin32Instance);
    windowClass.lpfnWndProc = WindowProc;
    windowClass.lpszClassName = ssWindowClassName.c_str();
    UINT style = CS_OWNDC;
    windowClass.style = style;
    windowClass.hbrBackground = reinterpret_cast<HBRUSH>(COLOR_BACKGROUND);

    ::ATOM returnValue = ::RegisterClassEx(&windowClass);
    if (0 == returnValue)
    {
        ::DWORD error = ::GetLastError();
        REPORTWIN32ERROR("Failed to register class; error %d '%s'", error);
        return;
    }

    return;
}

void Application::UnregisterWindowClass()
{
    ::BOOL result = ::UnregisterClass(ssWindowClassName.c_str(), static_cast< ::HINSTANCE>(this->mhWin32Instance));
    if (0 == result)
    {
        ::DWORD error = ::GetLastError();
        REPORTWIN32ERROR("Failed to unregister class; error %d, '%s'", error);
        return;
    }
}

void Application::CreateMainWindow()
{
    DWORD exStyle = WS_EX_ACCEPTFILES;
    std::string mainWindowName("Textured quad renderer");
    DWORD style = WS_OVERLAPPEDWINDOW;
    int x = CW_USEDEFAULT;
    int y = CW_USEDEFAULT;
    int width = 512;
    int height = 512;
    ::HWND parentWindow = 0;
    ::HMENU menuHandle = 0;
    ::LPVOID lpParam = 0;

    shMainWindow = ::CreateWindowEx(
        exStyle,
        ssWindowClassName.c_str(),
        mainWindowName.c_str(),
        style,
        x,
        y,
        width,
        height,
        parentWindow,
        menuHandle,
        static_cast< ::HINSTANCE>(this->mhWin32Instance),
        lpParam);
    if (0 == shMainWindow)
    {
        ::DWORD error = ::GetLastError();
        REPORTWIN32ERROR("Failed to create window; error %d, '%s'", error);
        return;
    }

    ::UpdateWindow(shMainWindow);
    ::ShowWindow(shMainWindow, SW_SHOWDEFAULT);
}

void Application::DestroyMainWindow()
{
    if (0 != shMainWindow && ::IsWindow(shMainWindow))
    {
        BOOL result = ::DestroyWindow(shMainWindow);
        if (0 == result)
        {
            ::DWORD error = ::GetLastError();
            REPORTWIN32ERROR("Failed to destroy window; error %d, '%s'", error);
            return;
        }
    }
}

void Application::MainLoop()
{
    ::MSG msg;

    // loop until WM_QUIT(0) received
    while(::GetMessage(&msg, 0, 0, 0) > 0)
    {
        ::TranslateMessage(&msg);
        ::DispatchMessage(&msg);
    }

    this->mi32ExitCode = (int)msg.wParam;
}

void Application::SetRenderer(Renderer *lpRenderer)
{
    this->mpRenderer = lpRenderer;
}

Renderer *Application::GetRenderer()
{
    return this->mpRenderer;
}
