// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
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
