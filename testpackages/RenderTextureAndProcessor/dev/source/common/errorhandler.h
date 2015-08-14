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
#ifndef ERRORHANDLER_H
#define ERRORHANDLER_H

class ErrorHandler
{
public:
    static void Report(const char *file, int line, const char *message, ...);
    static void ReportWin32Error(const char *file, int line, const void *hModule, const char *message, int errorCode);
};

#define REPORTERROR(_message)                               ErrorHandler::Report(__FILE__, __LINE__, _message)
#define REPORTERROR1(_message, _value1)                     ErrorHandler::Report(__FILE__, __LINE__, _message, _value1)
#define REPORTERROR2(_message, _value1, _value2)            ErrorHandler::Report(__FILE__, __LINE__, _message, _value1, _value2)
#define REPORTERROR3(_message, _value1, _value2, _value3)   ErrorHandler::Report(__FILE__, __LINE__, _message, _value1, _value2, _value3)
#define REPORTERROR4(_message, _value1, _value2, _value3, _value4) \
    ErrorHandler::Report(__FILE__, __LINE__, _message, _value1, _value2, _value3, _value4)
#define REPORTWIN32ERROR(_message, _errCode)                ErrorHandler::ReportWin32Error(__FILE__, __LINE__, 0, _message, _errCode)
#define REPORTWIN32MODULEERROR(_module, _message, _errCode) ErrorHandler::ReportWin32Error(__FILE__, __LINE__, _module, _message, _errCode)

#endif // ERRORHANDLER_H
