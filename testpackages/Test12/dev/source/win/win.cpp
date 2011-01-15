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
