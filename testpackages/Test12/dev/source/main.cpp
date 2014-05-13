#define C_UNUSED(_arg) (void)_arg

extern int MyCreateWindow();
extern int MyWindowLoop();
extern void MyDestroyWindow();

int Main()
{
    if (MyCreateWindow() < 0)
    {
        return -1;
    }
    int returnValue = MyWindowLoop();
    MyDestroyWindow();
    return returnValue;
}

#if defined(_WIN32)
#include <Windows.h>

HINSTANCE gInstance = 0;

int APIENTRY WinMain(
  HINSTANCE hInstance,
  HINSTANCE hPrevInstance,
  LPSTR lpCmdLine,
  int nCmdShow
)
{
    C_UNUSED(hPrevInstance);
    C_UNUSED(lpCmdLine);
    C_UNUSED(nCmdShow);

    gInstance = hInstance;
#else // defined(_WIN32)
int main()
{
#endif // defined(_WIN32)
    int returnValue = Main();
    return returnValue;
}
