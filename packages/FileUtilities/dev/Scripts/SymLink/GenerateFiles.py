#!/usr/bin/python

import os
import string
import sys

sys.path.append("../../../../../python")
from executeprocess import ExecuteProcess
from getpaths import GetOpusPaths

opusPackageDir, opusTestPackageDir, opusCodeGeneratorExe = GetOpusPaths()

# FileUtilities symlink options
symlink_options = [
    opusCodeGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "FileUtilities", "dev", "Scripts", "SymLink", "ISymLinkOptions.cs")),
    "-n=FileUtilities",
    "-c=SymLinkOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")),
    "-pv=SymLinkPrivateData"
]
(stdout,stderr) = ExecuteProcess(symlink_options, True, True)
print stdout
