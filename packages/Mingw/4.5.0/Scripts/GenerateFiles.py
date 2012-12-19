#!/usr/bin/python

import os
import string
import subprocess

def ExecuteProcess(args, verbose=False):
    if verbose:
        print "Executing: '%s'" % " ".join(args)
    process = subprocess.Popen(args, stdout=subprocess.PIPE)
    output = process.communicate()
    if process.returncode != 0:
        raise RuntimeError("Command '%s' failed" % (" ".join(args)))
    return output

get_opus_dir_command = [
    "Opus",
    "-verbosity=0",
    "-showdirectory"
]
(stdout,stderr) = ExecuteProcess(get_opus_dir_command)
opusBinDir = string.strip(stdout, os.linesep)
opusPackageDir = os.path.abspath(os.path.join(opusBinDir, os.pardir, os.pardir, "packages"))

# C compiler options
cCompiler_options = [
    os.path.join(opusBinDir, "OpusOptionInterfacePropertyGenerator.exe"),
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "Mingw", "4.5.0", "Scripts", "ICCompilerOptions.cs")),
    "-n=Mingw",
    "-c=CCompilerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")),
    "-pv=MingwCommon.PrivateData",
    "-e" # this option set derives from the MingwCommon option set
]
(stdout,stderr) = ExecuteProcess(cCompiler_options, True)
print stdout

# C++ compiler options
cxxCompiler_options = [
    os.path.join(opusBinDir, "OpusOptionInterfacePropertyGenerator.exe"),
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "C", "dev", "Scripts", "ICxxCompilerOptions.cs")),
    "-n=Mingw",
    "-c=CxxCompilerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")),
    "-pv=MingwCommon.PrivateData",
    "-e" # this option set derives from the C option set
]
(stdout,stderr) = ExecuteProcess(cxxCompiler_options, True)
print stdout
