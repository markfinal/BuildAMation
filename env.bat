@ECHO OFF

SETLOCAL ENABLEDELAYEDEXPANSION

SET DefaultFlavour=Release

IF NOT "%1"=="" (
  SET flavour=%1
) ELSE (
  SET flavour=%DefaultFlavour%
)

SET OpusPath=%CD%\bin\%flavour%
REM Using delayed expansion in case PATH has some spaces in
SET NewPath=!PATH!

REM Export the PATH
IF NOT EXIST %OpusPath% (
  ECHO *** ERROR: Opus directory '%OpusPath%' does not exist ***
) ELSE (
  ECHO Added '%OpusPath%' to the start of PATH
  SET NewPath=%OPUSPATH%;!NewPath!
)

REM Pass the local variable out to the global
ENDLOCAL&SET PATH=%NewPath%
