#!/usr/bin/python

from distutils.spawn import find_executable
import os
import sys
from optparse import OptionParser

bam_path = find_executable('bam')
if not bam_path:
  raise RuntimeError('Unable to locate bam')
bam_dir = os.path.dirname(bam_path)
package_tools_dir = os.path.join(bam_dir, 'packagetools')
sys.path.append(package_tools_dir)

from executeprocess import ExecuteProcess
from getpaths import GetBuildAMationPaths
from standardoptions import StandardOptions

parser = OptionParser()
(options, args, extra_args) = StandardOptions(parser)

stdPackageDir, testPackageDir, optionGeneratorExe = GetBuildAMationPaths(bam_dir)

licenseHeaderFile = os.path.relpath(os.path.join(os.path.dirname(optionGeneratorExe), "licenseheader.txt"))

# C# compiler options
csCompiler_options = [
    optionGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(stdPackageDir, "CSharp", "dev", "Scripts", "IOptions.cs")),
    "-n=CSharp",
    "-c=OptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(stdPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")) + os.pathsep + os.path.relpath(os.path.join(stdPackageDir, "VisualStudioProcessor", "dev", "Scripts", "VisualStudioDelegate.cs")),
    "-pv=PrivateData",
    "-l=" + licenseHeaderFile
]
csCompiler_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(csCompiler_options, True, True)
print stdout
