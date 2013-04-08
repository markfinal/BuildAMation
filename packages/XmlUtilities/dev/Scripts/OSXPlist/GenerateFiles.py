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

# OSXPlist options
plist_options = [
    opusCodeGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "XmlUtilities", "dev", "Scripts", "OSXPlist", "IOSXPlistOptions.cs")),
    "-n=XmlUtilities",
    "-c=OSXPlistWriterOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-pv=PrivateData"
]
plist_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(plist_options, True, True)
print stdout
