#!/usr/bin/python

import os
import string
import sys

sys.path.append("../../../../python")
from executeprocess import ExecuteProcess

get_opus_dir_command = [
    "Opus",
    "-verbosity=0",
    "-showdirectory"
]
(stdout,stderr) = ExecuteProcess(get_opus_dir_command)
opusBinDir = string.strip(stdout, os.linesep)
generatorPath = os.path.join(opusBinDir, "OpusOptionInterfacePropertyGenerator.exe")
opusPackageDir = os.path.abspath(os.path.join(opusBinDir, os.pardir, os.pardir, "packages"))
opusTestPackageDir = os.path.abspath(os.path.join(opusBinDir, os.pardir, os.pardir, "testpackages"))

# CodeGenTest2 tool options
codegentest2_options = [
    generatorPath,
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
