#if defined(_WIN32)
extern __declspec(dllimport) int MyFunction();
#else
extern int MyFunction();
#endif

int main()
{
    return MyFunction();
}
