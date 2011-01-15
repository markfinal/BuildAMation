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
%OPUS_DIR%\OpusOptionInterfacePropertyGenerator -i=%OPUS_PACKAGE_DIR%\C\dev\Scripts\ICCompilerOptions.cs;%OPUS_PACKAGE_DIR%\MingwCommon\dev\Scripts\ICCompilerOptions.cs -o=CCompilerOptionProperties.cs -n=MingwCommon -c=CCompilerOptionCollection
IF NOT ERRORLEVEL 0 GOTO error

REM C++ compiler option properties
%OPUS_DIR%\OpusOptionInterfacePropertyGenerator -i=%OPUS_PACKAGE_DIR%\C\dev\Scripts\ICPlusPlusCompilerOptions.cs -o=CPlusPlusCompilerOptionProperties.cs -n=MingwCommon -c=CPlusPlusCompilerOptionCollection
IF NOT ERRORLEVEL 0 GOTO error

REM Linker option properties
%OPUS_DIR%\OpusOptionInterfacePropertyGenerator -i=%OPUS_PACKAGE_DIR%\C\dev\Scripts\ILinkerOptions.cs -o=LinkerOptionProperties.cs -n=MingwCommon -c=LinkerOptionCollection
IF NOT ERRORLEVEL 0 GOTO error

REM Archiver option properties
%OPUS_DIR%\OpusOptionInterfacePropertyGenerator -i=%OPUS_PACKAGE_DIR%\C\dev\Scripts\IArchiverOptions.cs;%OPUS_PACKAGE_DIR%\MingwCommon\dev\Scripts\IArchiverOptions.cs -o=ArchiverOptionProperties.cs -n=MingwCommon -c=ArchiverOptionCollection
IF NOT ERRORLEVEL 0 GOTO error

REM Toolchain option properties
%OPUS_DIR%\OpusOptionInterfacePropertyGenerator -i=%OPUS_PACKAGE_DIR%\C\dev\Scripts\IToolchainOptions.cs -o=ToolchainOptionProperties.cs -n=MingwCommon -c=ToolchainOptionCollection
IF NOT ERRORLEVEL 0 GOTO error

PAUSE
GOTO end

:error
ECHO "There was an error running Opus."

:end

ENDLOCAL
