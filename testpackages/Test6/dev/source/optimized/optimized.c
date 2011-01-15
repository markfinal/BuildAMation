#ifndef D_OPUS_CONFIGURATION_OPTIMIZED
#error This file can only be compiled in Opus optimized configuration builds
#endif

#include "header.h"

char *GetConfiguration()
{
    return "Optimized";
}
