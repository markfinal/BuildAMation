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

# TODO: need to get the current package root
currentPackageRoot = os.path.join(os.getcwd(), "..", "..", "..", "..")
licenseTextFile = os.path.relpath(os.path.join(currentPackageRoot, "..", "LicenseText.txt"))

# Qt Moc options
moc_options = [
    optionGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(currentPackageRoot, "QtCommon5", "dev", "Scripts", "Moc", "IMocOptions.cs")),
    "-n=QtCommon",
    "-c=MocOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.join(stdPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs"),
    "-pv=MocPrivateData",
    "-l=" + licenseTextFile
]
moc_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(moc_options, True, True)
print stdout

