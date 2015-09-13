@ECHO OFF

SETLOCAL

IF "%1" == "-nopause" (
    SET NOPAUSE=1
) ELSE (
    SET NOPAUSE=0
)

FOR /D %%A IN (%CD%\*) DO CALL :FIND_PACKAGE_VERSIONS %%A
IF %NOPAUSE%==0 (
    PAUSE
)
GOTO :EOF

:FIND_PACKAGE_VERSIONS
CALL :DELETE_DEBUG_PROJECT_DIRECTORY %%1
CALL :DELETE_BUILD_DIRECTORY %%1
GOTO :EOF

:DELETE_DEBUG_PROJECT_DIRECTORY
IF EXIST %1\PackageDebug (
    ECHO Deleting '%1\PackageDebug' directory and all children
    RMDIR /S /Q %1\PackageDebug
)

:DELETE_BUILD_DIRECTORY
IF EXIST %1\build (
    ECHO Deleting '%1\build' directory and all children
    RMDIR /S /Q %1\build
)
IF EXIST %1\debug_build (
    ECHO Deleting '%1\debug_build' directory and all children
    RMDIR /S /Q %1\debug_build
)

ENDLOCAL
