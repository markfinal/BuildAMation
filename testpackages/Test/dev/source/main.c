#include <stdio.h>

#define C_UNUSED(_arg) (void)_arg

#ifdef __cplusplus
class MyClass
{
public:
    MyClass()
    {
        try
        {
            printf("MyClass constructor\n");
        }
        catch (...)
        {
            printf("Exception...\n");
            throw;
        }
    }
    
    int returnValue() const
    {
        return 0;
    }
};
#endif

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
#ifdef __cplusplus
    MyClass instance;
    return instance.returnValue();
#else
    return 0;
#endif
}
