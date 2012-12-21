#!/usr/bin/python

import os
import string
import sys

sys.path.append("../../../../python")
from executeprocess import ExecuteProcess

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
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "C", "dev", "Scripts", "ICCompilerOptions.cs")) + os.pathsep + os.path.relpath(os.path.join(opusPackageDir, "VisualCCommon", "dev", "Scripts", "ICCompilerOptions.cs")),
    "-n=VisualCCommon",
    "-c=CCompilerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")) + os.pathsep + os.path.relpath(os.path.join(opusPackageDir, "VisualStudioProcessor", "dev", "Scripts", "VisualStudioDelegate.cs")),
    "-pv=PrivateData"
]
(stdout,stderr) = ExecuteProcess(cCompiler_options, True, True)
print stdout

# C++ compiler options
cxxCompiler_options = [
    os.path.join(opusBinDir, "OpusOptionInterfacePropertyGenerator.exe"),
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "C", "dev", "Scripts", "ICxxCompilerOptions.cs")),
    "-n=VisualCCommon",
    "-c=CxxCompilerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")) + os.pathsep + os.path.relpath(os.path.join(opusPackageDir, "VisualStudioProcessor", "dev", "Scripts", "VisualStudioDelegate.cs")),
    "-pv=PrivateData",
    "-e" # this option set derives from the C option set
]
(stdout,stderr) = ExecuteProcess(cxxCompiler_options, True, True)
print stdout

# Linker options
linker_options = [
    os.path.join(opusBinDir, "OpusOptionInterfacePropertyGenerator.exe"),
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "C", "dev", "Scripts", "ILinkerOptions.cs")) + os.pathsep + os.path.relpath(os.path.join(opusPackageDir, "VisualCCommon", "dev", "Scripts", "ILinkerOptions.cs")),
    "-n=VisualCCommon",
    "-c=LinkerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")) + os.pathsep + os.path.relpath(os.path.join(opusPackageDir, "VisualStudioProcessor", "dev", "Scripts", "VisualStudioDelegate.cs")),
    "-pv=PrivateData"
]
(stdout,stderr) = ExecuteProcess(linker_options, True, True)
print stdout

# Archiver options
archiver_options = [
    os.path.join(opusBinDir, "OpusOptionInterfacePropertyGenerator.exe"),
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "C", "dev", "Scripts", "IArchiverOptions.cs")) + os.pathsep + os.path.relpath(os.path.join(opusPackageDir, "VisualCCommon", "dev", "Scripts", "IArchiverOptions.cs")),
    "-n=VisualCCommon",
    "-c=ArchiverOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")) + os.pathsep + os.path.relpath(os.path.join(opusPackageDir, "VisualStudioProcessor", "dev", "Scripts", "VisualStudioDelegate.cs")),
    "-pv=PrivateData"
]
(stdout,stderr) = ExecuteProcess(archiver_options, True, True)
print stdout
