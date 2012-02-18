@ECHO OFF

SETLOCAL

REM I don't know how this works, but see http://superuser.com/questions/80485/exit-batch-file-from-subroutine
IF "%selfWrapped%"=="" (
  REM this is necessary so that we can use "exit" to terminate the batch file,
  REM and all subroutines, but not the original cmd.exe
  SET selfWrapped=true
  %ComSpec% /s /c ""%~0" %*"
  GOTO :EOF
)

SET PLATFORM=%1
SET BUILDER=%2
SET BUILDROOT=build

ECHO Platform is %PLATFORM%
ECHO Builder is %BUILDER%

FOR /D %%A IN (%CD%\*) DO CALL :FIND_PACKAGE_VERSIONS %%A
PAUSE
GOTO :EOF

:FIND_PACKAGE_VERSIONS
FOR /D %%B IN (%1\*) DO CALL :EXECUTETEST %%B
GOTO :EOF

:EXECUTETEST
CALL :DELETE_BUILD_DIRECTORY %1
CALL :EXECUTE_TEST %1
CALL :DELETE_BUILD_DIRECTORY %1
GOTO :EOF

:EXECUTE_TEST
IF EXIST %1\test%PLATFORM%.txt (
    PUSHD %1
    FOR /f "tokens=*" %%A in (%1\test%PLATFORM%.txt) do (
        Opus @%1\%%A -platforms=%PLATFORM% -configurations="debug;optimized" -buildroot=%BUILDROOT% -builder=%BUILDER% -verbosity=1
        IF NOT ERRORLEVEL 0 EXIT ERRORLEVEL
    )
    POPD
)
GOTO :EOF

:DELETE_BUILD_DIRECTORY
IF EXIST %1\build (
    ECHO Deleting '%1\%BUILDROOT%' directory and all children
    RMDIR /S /Q %1\%BUILDROOT%
)
GOTO :EOF

ENDLOCAL