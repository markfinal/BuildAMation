#include "application.h"
#include "common.h"

#include <Windows.h>

int APIENTRY WinMain(HINSTANCE hInstance, HINSTANCE UNUSEDARG(hPrevInstance), LPSTR UNUSEDARG(lpCmdLine), int UNUSEDARG(nShowCmd))
{
    Application app(__argc, __argv);
    app.SetWin32Instance(hInstance);
    int exitCode = app.Run();
    return exitCode;
}
