#!/usr/bin/python

from distutils.spawn import find_executable
import os
import sys
from optparse import OptionParser

bam_path = find_executable('bam')
if not bam_path:
  raise RuntimeError('Unable to locate bam')
bam_dir = os.path.dirname(bam_path)
package_tools_dir = os.path.join(bam_dir, 'packagetools')
sys.path.append(package_tools_dir)

from executeprocess import ExecuteProcess
from getpaths import GetBuildAMationPaths
from standardoptions import StandardOptions

parser = OptionParser()
(options, args, extra_args) = StandardOptions(parser)

stdPackageDir, testPackageDir, optionGeneratorExe = GetBuildAMationPaths(bam_dir)

licenseHeaderFile = os.path.relpath(os.path.join(os.path.dirname(optionGeneratorExe), "licenseheader.txt"))

# C compiler options
cCompiler_options = [
    optionGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(stdPackageDir, "Gcc", "4.0", "Scripts", "ICCompilerOptions.cs")),
    "-n=Gcc",
    "-c=CCompilerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(stdPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")),
    "-pv=GccCommon.PrivateData",
    "-e", # this option set derives from the GccCommon option set
    "-l=" + licenseHeaderFile
]
cCompiler_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(cCompiler_options, True, True)
print stdout

# C++ compiler options
cxxCompiler_options = [
    optionGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(stdPackageDir, "C", "dev", "Scripts", "ICxxCompilerOptions.cs")),
    "-n=Gcc",
    "-c=CxxCompilerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(stdPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")),
    "-pv=GccCommon.PrivateData",
    "-e", # this option set derives from the C option set
    "-l=" + licenseHeaderFile
]
cxxCompiler_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(cxxCompiler_options, True, True)
print stdout

# ObjC compiler options
objcCompiler_options = [
    optionGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(stdPackageDir, "Gcc", "4.0", "Scripts", "ICCompilerOptions.cs")),
    "-n=Gcc",
    "-c=ObjCCompilerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(stdPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")),
    "-pv=GccCommon.PrivateData",
    "-e", # this option set derives from the GccCommon option set
    "-l=" + licenseHeaderFile
]
objcCompiler_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(objcCompiler_options, True, True)
print stdout

# C++ compiler options
objcxxCompiler_options = [
    optionGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(stdPackageDir, "C", "dev", "Scripts", "ICxxCompilerOptions.cs")),
    "-n=Gcc",
    "-c=ObjCxxCompilerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(stdPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")),
    "-pv=GccCommon.PrivateData",
    "-e", # this option set derives from the C option set
    "-l=" + licenseHeaderFile
]
objcxxCompiler_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(objcxxCompiler_options, True, True)
print stdout
