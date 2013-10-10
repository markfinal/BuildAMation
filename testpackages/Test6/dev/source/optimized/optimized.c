#ifdef MAIN_C
#error MAIN_C has been defined
#endif

#ifndef D_OPUS_CONFIGURATION_OPTIMIZED
#error This file can only be compiled in Opus optimized configuration builds
#endif

#include "header.h"

const char *GetConfiguration()
{
    return "Optimized";
}
