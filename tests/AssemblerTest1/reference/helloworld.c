#include <stdio.h>

int main()
{
#ifdef BUILD32
    printf("Hello world, from 32-bits\n");
#else
    printf("Hello world, from 64-bits\n");
#endif
    return 0;
}
