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

# Publisher Product options
publishproduct_options = [
    opusCodeGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "Publisher", "dev", "Scripts", "Product", "Interfaces", "IPublishOptions.cs")),
    "-n=Publisher",
    "-c=OptionSet",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")),
    "-pv=PrivateData"
]
publishproduct_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(publishproduct_options, True, True)
print stdout
