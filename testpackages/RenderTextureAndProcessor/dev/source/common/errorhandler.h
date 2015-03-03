/*
Copyright 2010-2015 Mark Final

This file is part of BuildAMation.

BuildAMation is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

BuildAMation is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
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
