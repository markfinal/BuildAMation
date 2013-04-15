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
symlink_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(symlink_options, True, True)
print stdout
