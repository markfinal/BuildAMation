$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("BuildAMation-Environment.lnk")
$Shortcut.TargetPath = "%comspec%"
$Shortcut.Arguments="/K env.bat"
$Shortcut.Save()
