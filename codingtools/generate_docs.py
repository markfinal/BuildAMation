#!/usr/bin/python

from optparse import OptionParser
import os
import subprocess
import sys


def NoDoxygenError(Exception):
    def __init__(self, message):
        self._message = message;

    def __str__(self):
        return self._message


def build_documentation(source_dir, doxygenpath):
    if not doxygenpath:
        raise RuntimeError("Path to doxygen is required")
    args = [doxygenpath, os.path.join(source_dir, 'docsrc', 'BuildAMationDoxy')]
    print "Running: %s" % (' '.join(args))
    current_dir = os.getcwd()
    try:
        os.chdir(source_dir)
        subprocess.check_call(args)
    except OSError:
        raise NoDoxygenError('Unable to run doxygen executable "%s"' % doxygenpath)
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
