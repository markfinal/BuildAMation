#ifdef D_BAM_PLATFORM_OSX
#include <ncurses.h>
#elif D_BAM_PLATFORM_LINUX
#include <dlfcn.h>
#else
#error Unsupported platform
#endif

int
main()
{
#ifdef D_BAM_PLATFORM_OSX
    initscr();
    endwin();
#elif D_BAM_PLATFORM_LINUX
    dlerror();
#else
#error Unsupported platform
#endif

    return 0;
}
