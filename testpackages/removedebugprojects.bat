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
FOR /D %%B IN (%1\*) DO CALL :DELETE_BAM_DIRECTORY %%B
FOR /D %%B IN (%1\*) DO CALL :DELETE_BUILD_DIRECTORY %%B
GOTO :EOF

:DELETE_BAM_DIRECTORY
IF EXIST %1\BamProject (
    ECHO Deleting '%1\BamProject' directory and all children
    RMDIR /S /Q %1\BamProject
)

:DELETE_BUILD_DIRECTORY
IF EXIST %1\build (
    ECHO Deleting '%1\build' directory and all children
    RMDIR /S /Q %1\build
)

ENDLOCAL
