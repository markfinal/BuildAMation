#!/usr/bin/python

import os
import string
import subprocess

def ExecuteProcess(args, verbose=False):
    if verbose:
        print "Executing: '%s'" % " ".join(args)
    process = subprocess.Popen(get_opus_dir_command, stdout=subprocess.PIPE)
    output = process.communicate()
    if process.returncode != 0:
      raise RuntimeError("Command '%s' failed", " ".join(get_opus_dir_command))
    return output

get_opus_dir_command = [
    "Opus",
    "-verbosity=0",
    "-showdirectory"
]
(stdout,stderr) = ExecuteProcess(get_opus_dir_command)
opusBinDir = string.strip(stdout, '\n')
opusPackageDir = os.path.abspath(os.path.join(opusBinDir, os.pardir, os.pardir, "packages"))

moc_options = [
    os.path.join(opusBinDir, "OpusOptionInterfacePropertyGenerator.exe"),
    "-i=IMocOptions.cs",
    "-o=MocOptionProperties.cs",
    "-n=QtCommon",
    "-c=MocOptionCollection"
]
(stdout,stderr) = ExecuteProcess(moc_options, True)

