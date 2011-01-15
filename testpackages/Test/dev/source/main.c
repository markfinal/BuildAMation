#include <stdio.h>

#define C_UNUSED(_arg) (void)_arg

#if defined(_WINDOWS)
#include <Windows.h>

int APIENTRY WinMain(
  HINSTANCE hInstance,
  HINSTANCE hPrevInstance,
  LPSTR lpCmdLine,
  int nCmdShow
)
{
    C_UNUSED(hInstance);
    C_UNUSED(hPrevInstance);
    C_UNUSED(lpCmdLine);
    C_UNUSED(nCmdShow);
#else
int main()
{
#endif
    printf("Hello world, C\n");
    return 0;
}
