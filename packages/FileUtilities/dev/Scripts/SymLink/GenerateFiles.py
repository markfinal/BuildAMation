#!/usr/bin/python

import os
import sys
from optparse import OptionParser

parser = OptionParser()
parser.add_option("-f", "--force", dest="force", action="store_true", default=False, help="Force writing")
parser.add_option("-u", "--updateheader", dest="updateheader", action="store_true", default=False, help="Update headers")
(options,args) = parser.parse_args()

extra_args = []
if options.force:
  extra_args.append("-f")
if options.updateheader:
  extra_args.append("-uh")

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
symlink_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(symlink_options, True, True)
print stdout
