from executeprocess import ExecuteProcess
import os
import string

def GetBuildAMationPaths(bam_dir):
	standardPackageDir = os.path.abspath(os.path.join(bam_dir, os.pardir, os.pardir, "packages"))
	testPackageDir = os.path.abspath(os.path.join(bam_dir, os.pardir, os.pardir, "testpackages"))
	optionGeneratorExe = os.path.join(bam_dir, "BamOptionGenerator.exe")
	return (standardPackageDir, testPackageDir, optionGeneratorExe)
