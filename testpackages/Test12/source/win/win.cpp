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
#include <Windows.h>
#include <stdio.h>

extern HINSTANCE gInstance;
HWND hwnd = 0;
bool running = true;

LRESULT WINAPI WindowProc(HWND hWnd, UINT Msg, WPARAM wParam, LPARAM lParam)
{
    if (WM_CLOSE == Msg)
    {
        running = false;
        return 0;
    }

    return DefWindowProc(hWnd, Msg, wParam, lParam);
}

int MyCreateWindow()
{
    WNDCLASSEX windowClass;
    ZeroMemory(&windowClass, sizeof(windowClass));
    windowClass.cbSize = sizeof(windowClass);
    windowClass.hInstance = gInstance;
    windowClass.lpszClassName = "Test";
    windowClass.lpfnWndProc = WindowProc;
    windowClass.hbrBackground = (HBRUSH)COLOR_BACKGROUND;
    ::RegisterClassEx(&windowClass);

    DWORD exStyle = 0;
    const char *className = "Test";
    const char *windowName = "OpenGL test";
    DWORD style = WS_OVERLAPPEDWINDOW | WS_VISIBLE;
    int x = CW_USEDEFAULT;
    int y = CW_USEDEFAULT;
    int width = 100;
    int height = 100;
    HWND parent = 0;
    HMENU menu = 0;
    HINSTANCE instance = gInstance;
    void *param = 0;

    hwnd = ::CreateWindowEx(exStyle, className, windowName, style, x, y, width, height, parent, menu, instance, param);
    if (0 == hwnd)
    {
        printf("Cannot create window, %lu", GetLastError());
        return -1;
    }

    ::ShowWindow(hwnd, SW_SHOWNORMAL);

    return 0;
}

int MyWindowLoop()
{
    MSG message;
    ZeroMemory(&message, sizeof(message));
    while (running && GetMessage(&message, 0, 0, 0) > 0)
    {
        TranslateMessage(&message);
        DispatchMessage(&message);
    }
    return static_cast<int>(message.wParam);
}

void MyDestroyWindow()
{
    ::DestroyWindow(hwnd);
    hwnd = 0;
    ::UnregisterClass("Test", gInstance);
}
