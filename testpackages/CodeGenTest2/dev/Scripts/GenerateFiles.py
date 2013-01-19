#!/usr/bin/python

import os
import string
import sys

sys.path.append("../../../../python")
from executeprocess import ExecuteProcess
from getpaths import GetOpusPaths

opusPackageDir, opusTestPackageDir, opusCodeGeneratorExe = GetOpusPaths()

# CodeGenTest2 tool options
codegentest2_options = [
    opusCodeGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(opusTestPackageDir, "CodeGenTest2", "dev", "Scripts", "ICodeGenOptions.cs")),
    "-n=CodeGenTest2",
    "-c=CodeGenOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")),
    "-pv=PrivateData"
]
(stdout,stderr) = ExecuteProcess(codegentest2_options, True, True)
print stdout
