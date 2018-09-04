@ECHO OFF

SETLOCAL ENABLEDELAYEDEXPANSION

SET DefaultFlavour=Release

IF NOT "%1"=="" (
  SET flavour=%1
) ELSE (
  SET flavour=%DefaultFlavour%
)

SET ExecutablePath=%~dp0bin\%flavour%\netcoreapp2.1
REM Using delayed expansion in case PATH has some spaces in
SET NewPath=!PATH!

REM Export the PATH
IF NOT EXIST !ExecutablePath! (
  ECHO *** ERROR: BuildAMation directory '!ExecutablePath!' does not exist ***
) ELSE (
  SET NewPath=!ExecutablePath!;!NewPath!
  SET PATH=!ExecutablePath!;!PATH!
  dotnet !ExecutablePath!\Bam.dll --version
)

REM Pass the local variable out to the global
ENDLOCAL&SET PATH=%NewPath%
REM ENDLOCAL
