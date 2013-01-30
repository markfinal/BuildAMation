#!/usr/bin/python

import os
import sys
from optparse import OptionParser

parser = OptionParser()
parser.add_option("-f", "--force", dest="force", action="store_true", default=False, help="Force writing")
parser.add_option("-u", "--updateheader", dest="updateheader", action="store_true", default=False, help="Update headers")
(options,args) = parser.parse_args()

extra_args = []
if options.force:
  extra_args.append("-f")
if options.updateheader:
  extra_args.append("-uh")

sys.path.append("../../../../python")
from executeprocess import ExecuteProcess
from getpaths import GetOpusPaths

opusPackageDir, opusTestPackageDir, opusCodeGeneratorExe = GetOpusPaths()

# C compiler options
cCompiler_options = [
    opusCodeGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "Mingw", "4.5.0", "Scripts", "ICCompilerOptions.cs")),
    "-n=Mingw",
    "-c=CCompilerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")),
    "-pv=MingwCommon.PrivateData",
    "-e" # this option set derives from the MingwCommon option set
]
cCompiler_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(cCompiler_options, True, True)
print stdout

# C++ compiler options
cxxCompiler_options = [
    opusCodeGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "C", "dev", "Scripts", "ICxxCompilerOptions.cs")),
    "-n=Mingw",
    "-c=CxxCompilerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")),
    "-pv=MingwCommon.PrivateData",
    "-e" # this option set derives from the C option set
]
cxxCompiler_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(cxxCompiler_options, True, True)
print stdout
