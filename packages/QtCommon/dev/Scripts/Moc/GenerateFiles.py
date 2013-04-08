#!/usr/bin/python

import os
import sys
from optparse import OptionParser

sys.path.append("../../../../../python")
from executeprocess import ExecuteProcess
from getpaths import GetOpusPaths
from standardoptions import StandardOptions

parser = OptionParser()
(options, args, extra_args) = StandardOptions(parser)

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
moc_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(moc_options, True, True)
print stdout

