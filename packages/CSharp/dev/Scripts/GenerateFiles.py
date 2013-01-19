#!/usr/bin/python

import os
import string
import sys

sys.path.append("../../../../python")
from executeprocess import ExecuteProcess
from getpaths import GetOpusPaths

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
(stdout,stderr) = ExecuteProcess(csCompiler_options, True, True)
print stdout
