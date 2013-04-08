#!/usr/bin/python

import os
import sys
from optparse import OptionParser

sys.path.append("../../../../python")
from executeprocess import ExecuteProcess
from getpaths import GetOpusPaths
from standardoptions import StandardOptions

parser = OptionParser()
(options, args, extra_args) = StandardOptions(parser)

opusPackageDir, opusTestPackageDir, opusCodeGeneratorExe = GetOpusPaths()

# CodeGenTest tool options
codegentest_options = [
    opusCodeGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(opusTestPackageDir, "CodeGenTest", "dev", "Scripts", "ICodeGenOptions.cs")),
    "-n=CodeGenTest",
    "-c=CodeGenOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")),
    "-pv=PrivateData"
]
codegentest_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(codegentest_options, True, True)
print stdout
