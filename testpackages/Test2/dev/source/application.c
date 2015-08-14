/*
Copyright (c) 2010-2015, Mark Final
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

* Neither the name of BuildAMation nor the names of its
  contributors may be used to endorse or promote products derived from
  this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
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
#if defined(D_BAM_CONFIGURATION_DEBUG)
    return "debug";
#elif defined(D_BAM_CONFIGURATION_OPTIMIZED)
    return "optimized";
#elif defined(D_BAM_CONFIGURATION_PROFILE)
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
