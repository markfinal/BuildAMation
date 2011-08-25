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

REM Moc option properties
%OPUS_DIR%\OpusOptionInterfacePropertyGenerator -i=IMocOptions.cs -o=MocOptionProperties.cs -n=QtCommon -c=MocOptionCollection
IF NOT ERRORLEVEL 0 GOTO error

PAUSE
GOTO end

:error
ECHO "There was an error running Opus."

:end

ENDLOCAL
