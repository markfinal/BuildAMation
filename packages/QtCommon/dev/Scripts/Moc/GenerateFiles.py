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

# Qt Moc options
moc_options = [
    generatorPath,
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

