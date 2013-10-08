#!/usr/bin/python

import os
import sys
from optparse import OptionParser

sys.path.append("../../../../python")
from executeprocess import ExecuteProcess
from getpaths import GetOpusPaths
from standardoptions import StandardOptions

parser = OptionParser()
(options, args, extra_args) = StandardOptions(parser)

opusPackageDir, opusTestPackageDir, opusCodeGeneratorExe = GetOpusPaths()

# C compiler options
cCompiler_options = [
    opusCodeGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "C", "dev", "Scripts", "ICCompilerOptions.cs")) \
          + os.path.pathsep + os.path.relpath(os.path.join(opusPackageDir, "ClangCommon", "dev", "Scripts", "ICCompilerOptions.cs")),
    "-n=ClangCommon",
    "-c=CCompilerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")) \
           + os.path.pathsep + os.path.relpath(os.path.join(opusPackageDir, "XcodeProjectProcessor", "dev", "Scripts", "Delegate.cs")),
    "-pv=PrivateData"
]
cCompiler_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(cCompiler_options, True, True)
print stdout

# C++ compiler options
cxxCompiler_options = [
    opusCodeGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "C", "dev", "Scripts", "ICxxCompilerOptions.cs")),
    "-n=ClangCommon",
    "-c=CxxCompilerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")) \
           + os.path.pathsep + os.path.relpath(os.path.join(opusPackageDir, "XcodeProjectProcessor", "dev", "Scripts", "Delegate.cs")),
    "-pv=PrivateData",
    "-e" # this option set derives from the C option set
]
cxxCompiler_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(cxxCompiler_options, True, True)
print stdout

# ObjectiveC compiler options
# there are no options at present

# ObjectiveC++ compiler options
objCxxCompiler_options = [
    opusCodeGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "C", "dev", "Scripts", "ICxxCompilerOptions.cs")),
    "-n=ClangCommon",
    "-c=ObjCxxCompilerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")) + os.pathsep + \
             os.path.relpath(os.path.join(opusPackageDir, "XcodeProjectProcessor", "dev", "Scripts", "Delegate.cs")),
    "-pv=PrivateData",
    "-e", # this option set derives from the C option set
    "-b" # used as a base class
]
objCxxCompiler_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(objCxxCompiler_options, True, True)
print stdout
