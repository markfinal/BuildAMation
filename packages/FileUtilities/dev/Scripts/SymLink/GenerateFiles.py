#!/usr/bin/python

import os
import string
import sys

sys.path.append("../../../../../python")
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

# FileUtilities symlink options
symlink_options = [
    generatorPath,
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
