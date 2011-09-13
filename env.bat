@ECHO OFF

SET DefaultFlavour=Release

IF NOT "%1"=="" (
  SET flavour=%1
) ELSE (
  SET flavour=%DefaultFlavour%
)

SET OpusPath=%CD%\bin\%flavour%

REM Export the PATH
IF NOT EXIST %OpusPath% (
  ECHO Opus directory '%OpusPath%' does not exist
) ELSE (
  ECHO Adding '%OpusPath%' to the start of PATH
  REM Using delayed expansion in case PATH has some spaces in
  SET PATH=%OPUSPATH%;!PATH!
)

REM Unset local variables
SET OpusPath=
SET DefaultFlavour=
