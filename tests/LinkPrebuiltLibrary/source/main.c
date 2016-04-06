#if defined(D_BAM_PLATFORM_OSX)
#include <ncurses.h>
#elif defined(D_BAM_PLATFORM_LINUX)
#include <dlfcn.h>
#elif defined(D_BAM_PLATFORM_WINDOWS)
#include <Winsock2.h>
#else
#error Unsupported platform
#endif

int
main()
{
#if defined(D_BAM_PLATFORM_OSX)
    initscr();
    endwin();
#elif defined(D_BAM_PLATFORM_LINUX)
    dlerror();
#elif defined(D_BAM_PLATFORM_WINDOWS)
    WSADATA wsaData;
    WORD wVersionRequested = MAKEWORD(2, 2);
    WSAStartup(wVersionRequested, &wsaData);
#else
#error Unsupported platform
#endif

    return 0;
}
