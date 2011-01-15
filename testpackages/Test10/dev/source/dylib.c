#if defined(_WIN32)
extern __declspec(dllexport) int MyFunction();
#elif defined(__unix__) && (__GNUC__ >= 4)
extern __attribute__ ((visibility("default"))) int MyFunction();
#endif

int MyFunction()
{
    return 42;
}
