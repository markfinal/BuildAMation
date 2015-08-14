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
#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>
#include <string.h>

int
main(int argc, char* argv[])
{
    if (argc < 3)
    {
        fprintf(stderr, "Not enough arguments");
        return -1;
    }

    /*fprintf(stdout, "Tool is '%s'\n", argv[0]);*/
    /*fprintf(stdout, "Arg1 is '%s'\n", argv[1]);*/
    /*fprintf(stdout, "Arg2 is '%s'\n", argv[2]);*/

    {
        char path[256];
        char body[256];
        FILE *file;

#ifdef WIN32
        sprintf(path, "%s\\%s.c", argv[1], argv[2]);
#else
        sprintf(path, "%s/%s.c", argv[1], argv[2]);
#endif
        file = fopen(path, "wt");
        if (0 == file)
        {
            fprintf(stderr, "Unable to open '%s' for writing", path);
            return -2;
        }

        sprintf(body, "#include <stdio.h>\n\nvoid MyGeneratedFunction(){ printf(\"Hello world\\n\"); }\n");
        fwrite(body, 1, strlen(body), file);

        fclose(file);
        fprintf(stdout, "Generated source file written to '%s'\n", path);
    }

    return 0;
}
