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
