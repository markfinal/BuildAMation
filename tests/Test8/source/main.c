/*
Copyright (c) 2010-2016, Mark Final
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
#include "dynamiclibrary.h"

#include <Windows.h>
#if defined(_MSC_VER)
#include <DbgHelp.h>
#endif
#include <stdio.h>

static LONG WINAPI ExceptionHandler(struct _EXCEPTION_POINTERS *excp)
{
    printf("#### Unhandled exception caught: data @ %p\n", (void *)excp);
#if defined(_MSC_VER)
    if (0 != excp)
    {
        char *miniDumpPathName = "minidumptest.dmp";
        HANDLE currentProcessHandle = GetCurrentProcess();
        DWORD processId = GetCurrentProcessId();
        MINIDUMP_TYPE type = MiniDumpNormal;
        HANDLE outputFileHandle = CreateFile(miniDumpPathName, GENERIC_WRITE, 0, 0, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, 0);
        MINIDUMP_EXCEPTION_INFORMATION exceptionInfo;
        exceptionInfo.ThreadId = GetCurrentThreadId();
        exceptionInfo.ExceptionPointers = excp;
        exceptionInfo.ClientPointers = FALSE;
        if (INVALID_HANDLE_VALUE != outputFileHandle)
        {
            MiniDumpWriteDump(currentProcessHandle, processId, outputFileHandle, type, &exceptionInfo, 0, 0);
            CloseHandle(outputFileHandle);
        }
        else
        {
            printf("Cannot create minidump file '%s'\n", miniDumpPathName);
        }
    }
#endif /* defined(_MSC_VER) */

    return EXCEPTION_CONTINUE_SEARCH;
}

int main()
{
    const char * const dllPathname = "ExplicitDynamicLibrary.dll";
    int returnCode;
    HANDLE hDLL;

    SetUnhandledExceptionFilter(ExceptionHandler);

    returnCode = 0;
    hDLL = LoadLibrary(dllPathname);
    if (NULL != hDLL)
    {
        ExportedFunction_t exportFunction = (ExportedFunction_t)GetProcAddress(hDLL, "MyTestFunction");
        if (0 != exportFunction)
        {
            int input = 4;
            int output = exportFunction(input);
            printf("From DLL, input was %d, output is %d\n", input, output);
        }
        else
        {
            printf("ERR: Unable to locate function MyTestFunction\n");
            returnCode = -1;
        }
        FreeLibrary(hDLL);
        hDLL = 0;
    }
    else
    {
        printf("ERR: Unable to load DLL '%s'\n", dllPathname);
        returnCode = -2;
    }

    {
        int *a = 0;
        *a = 3;
    }

    return returnCode;
}
