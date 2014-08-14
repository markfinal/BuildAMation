#include "library.h"
#include "library2.h"
#include <stdio.h>
#include <stdint.h>

char *
GetBitSize()
{
    size_t pointerSize = sizeof(intptr_t);
    if (4 == pointerSize)
    {
        return "32-bits";
    }
    else if (8 == pointerSize)
    {
        return "64-bits";
    }
    else
    {
        return "Non-standard bits";
    }
}

char *
GetConfiguration()
{
#if defined(D_OPUS_CONFIGURATION_DEBUG)
    return "debug";
#elif defined(D_OPUS_CONFIGURATION_OPTIMIZED)
    return "optimized";
#elif defined(D_OPUS_CONFIGURATION_PROFILE)
    return "profile";
#else
    return "Unrecognized configuration";
#endif
}

int main()
{
    const char *bitSize = GetBitSize();
    const char *config = GetConfiguration();
    printf("Hello world, C, in %s (%s)\n", bitSize, config);
    printf("From library, '%s'\n", libraryFunction());
    printf("From library2, '%d'\n", library2Function());

    return 0;
}
