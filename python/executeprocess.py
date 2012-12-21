import subprocess
import sys

def ExecuteProcess(args, verbose=False, isCSharp=False):
    if isCSharp and sys.platform.startswith("darwin"):
      newArgs = ["mono"]
      newArgs.extend(args)
      args = newArgs
    if verbose:
        print "Executing: '%s'" % " ".join(args)
    process = subprocess.Popen(args, stdout=subprocess.PIPE)
    output = process.communicate()
    if process.returncode != 0:
        raise RuntimeError("Command '%s' failed" % (" ".join(args)))
    return output
