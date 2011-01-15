#ifndef D_OPUS_CONFIGURATION_DEBUG
#error This file can only be compiled in Opus debug configuration builds
#endif

#include "header.h"

char *GetConfiguration()
{
    return "Debug";
}
