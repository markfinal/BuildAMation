ECHO OFF

SETLOCAL

REM Must be run from a command prompt that Opus has been pathed onto
Opus -verbosity=0 -showdirectory > opusdirectory.temp.txt
IF NOT ERRORLEVEL 0 GOTO error

SET /P OPUS_DIR=<opusdirectory.temp.txt
erase opusdirectory.temp.txt

SET OPUS_PACKAGE_DIR=%OPUS_DIR%\..\..\packages

ECHO Opus directory is %OPUS_DIR%
ECHO Opus package directory is %OPUS_PACKAGE_DIR%

REM C compiler option properties
%OPUS_DIR%\OpusOptionInterfacePropertyGenerator -i=%OPUS_PACKAGE_DIR%\Mingw\4.5.0\Scripts\ICCompilerOptions.cs -o=CCompilerOptionProperties.cs -n=Mingw -c=CCompilerOptionCollection
IF NOT ERRORLEVEL 0 GOTO error

REM C++ compiler option properties
%OPUS_DIR%\OpusOptionInterfacePropertyGenerator -i=%OPUS_PACKAGE_DIR%\C\dev\Scripts\ICPlusPlusCompilerOptions.cs -o=CPlusPlusCompilerOptionProperties.cs -n=Mingw -c=CPlusPlusCompilerOptionCollection
IF NOT ERRORLEVEL 0 GOTO error

PAUSE
GOTO end

:error
ECHO "There was an error running Opus."

:end

ENDLOCAL
