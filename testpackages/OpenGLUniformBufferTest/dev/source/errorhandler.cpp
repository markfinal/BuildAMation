// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#include "errorhandler.h"
#include <Windows.h>
#include <cstdio>
#include <cstdarg>

void ErrorHandler::Report(const char *file, int line, const char *message, ...)
{
    char buffer[1024];
    std::sprintf(buffer, "%s(%d) : %s\n", file, line, message);

    va_list list;
    va_start(list, message);
    if (::IsDebuggerPresent())
    {
        char buffer2[1024];
        vsprintf(buffer2, buffer, list);
        ::OutputDebugString(buffer2);
    }
    else
    {
        vfprintf(stderr, buffer, list);
    }
    va_end(list);
}

void ErrorHandler::ReportWin32Error(const char *file, int line, const char *message, int errorCode)
{
    //translate the error code into a message
    void *p_text;
    DWORD count = ::FormatMessage(
        FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_MAX_WIDTH_MASK,
        NULL,
        errorCode,
        MAKELANGID (LANG_NEUTRAL, SUBLANG_DEFAULT),
        (LPTSTR) &p_text,
        0,
        NULL);
    if (0 == count)
    {
        REPORTERROR("Format message failed reporting a Win32 error");
        return;
    }

    Report(file, line, message, errorCode, p_text);

    //free buffer
    ::LocalFree(p_text);
}
