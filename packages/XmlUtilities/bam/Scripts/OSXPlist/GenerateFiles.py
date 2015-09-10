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

# OSXPlist options
plist_options = [
    optionGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(stdPackageDir, "XmlUtilities", "dev", "Scripts", "OSXPlist", "IOSXPlistOptions.cs")),
    "-n=XmlUtilities",
    "-c=OSXPlistWriterOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-pv=PrivateData",
    "-l=" + licenseHeaderFile
]
plist_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(plist_options, True, True)
print stdout
