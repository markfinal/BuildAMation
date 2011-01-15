#include "application.h"
#include "common.h"

#if defined(_WIN32)
#include <Windows.h>
#endif // defined(_WIN32)

#if defined(_WIN32)
int APIENTRY WinMain(
    HINSTANCE instance,
    HINSTANCE UNUSEDARG(prevInstance),
    LPSTR UNUSEDARG(lpCmdLine),
    int UNUSEDARG(nShowCmd))
{
    Application app(__argc, __argv);
    app.SetWin32Instance(instance);
    int exitCode = app.Run();
    return exitCode;
}
#else
int main(int argc, char *argv[])
{
    Application app(argc, argv);
    int exitCode = app.Run();
    return exitCode;
}
#endif
