#!/usr/bin/python

from optparse import OptionParser
import os
import subprocess
import sys

def build_documentation(build_dir, doxygenpath):
    if not doxygenpath:
        raise RuntimeError("Path to doxygen is required")
    args = [doxygenpath, "docsrc/BuildAMationDoxy"]
    print "Running: %s" % ' '.join(args)
    current_dir = os.getcwd()
    try:
        os.chdir(build_dir)
        subprocess.check_call(args)
    finally:
        os.chdir(current_dir)


if __name__ == "__main__":
    parser = OptionParser()
    parser.add_option("-d", "--doxygen", dest="doxygenpath", default=None, help="Path to the doxygen executable.")
    (options, args) = parser.parse_args()

    try:
        build_documentation(os.getcwd(), options.doxygenpath)
    except Exception, e:
        print >>sys.stdout, "*** Failure reason: %s" % str(e)
        sys.stdout.flush()
    print >>sys.stdout, "Done"
    sys.stdout.flush()
