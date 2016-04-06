#ifdef D_BAM_PLATFORM_OSX
#include <ncurses.h>

#else
#error Unsupported platform
#endif

int
main()
{
#ifdef D_BAM_PLATFORM_OSX
    initscr();
    endwin();
#else
#error Unsupported platform
#endif

    return 0;
}
