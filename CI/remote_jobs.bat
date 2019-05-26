SET ThisScriptsDirectory=%~dp0

SET BatchScriptPath=%ThisScriptsDirectory%travisci_jobs.bat
ECHO Launching Travis-CI temote jobs
CALL %BatchScriptPath%

SET PowerShellScriptPath=%ThisScriptsDirectory%appveyor_jobs.ps1
ECHO Launching AppVeyor remote jobs
PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '%PowerShellScriptPath%'";
