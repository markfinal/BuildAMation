#ifndef MAIN_C
#error MAIN_C has not been defined
#endif

#include "header.h"
#include "platform.h"

#include <stdio.h>

int main()
{
    const char *configuration = GetConfiguration();
    printf("Configuration is '%s'\n", configuration);

    return 0;
}
