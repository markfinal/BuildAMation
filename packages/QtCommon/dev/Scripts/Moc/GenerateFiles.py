#!/usr/bin/python

import os
import string
import sys

sys.path.append("../../../../../python")
from executeprocess import ExecuteProcess
from getpaths import GetOpusPaths

opusPackageDir, opusTestPackageDir, opusCodeGeneratorExe = GetOpusPaths()

# Qt Moc options
moc_options = [
    opusCodeGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "QtCommon", "dev", "Scripts", "Moc", "IMocOptions.cs")),
    "-n=QtCommon",
    "-c=MocOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")),
    "-pv=MocPrivateData"
]
(stdout,stderr) = ExecuteProcess(moc_options, True, True)
print stdout

