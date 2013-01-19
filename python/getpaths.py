from executeprocess import ExecuteProcess
import os
import string

def GetOpusPaths():
	get_opus_dir_command = [
		"Opus",
		"-verbosity=0",
		"-showdirectory"
	]
	(stdout,stderr) = ExecuteProcess(get_opus_dir_command)
	opusBinDir = string.strip(stdout, os.linesep)
	opusPackageDir = os.path.abspath(os.path.join(opusBinDir, os.pardir, os.pardir, "packages"))
	opusTestPackageDir = os.path.abspath(os.path.join(opusBinDir, os.pardir, os.pardir, "testpackages"))
	opusCodeGeneratorExe = os.path.join(opusBinDir, "OpusOptionCodeGenerator.exe")
	return (opusPackageDir, opusTestPackageDir, opusCodeGeneratorExe)
