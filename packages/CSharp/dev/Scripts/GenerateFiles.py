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

# C# compiler options
csCompiler_options = [
    opusCodeGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "CSharp", "dev", "Scripts", "IOptions.cs")),
    "-n=CSharp",
    "-c=OptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")) + os.pathsep + os.path.relpath(os.path.join(opusPackageDir, "VisualStudioProcessor", "dev", "Scripts", "VisualStudioDelegate.cs")),
    "-pv=PrivateData"
]
csCompiler_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(csCompiler_options, True, True)
print stdout
