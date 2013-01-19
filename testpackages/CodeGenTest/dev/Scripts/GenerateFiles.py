#!/usr/bin/python

import os
import sys

sys.path.append("../../../../python")
from executeprocess import ExecuteProcess
from getpaths import GetOpusPaths

opusPackageDir, opusTestPackageDir, opusCodeGeneratorExe = GetOpusPaths()

# CodeGenTest tool options
codegentest_options = [
    opusCodeGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(opusTestPackageDir, "CodeGenTest", "dev", "Scripts", "ICodeGenOptions.cs")),
    "-n=CodeGenTest",
    "-c=CodeGenOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")),
    "-pv=PrivateData"
]
(stdout,stderr) = ExecuteProcess(codegentest_options, True, True)
print stdout
