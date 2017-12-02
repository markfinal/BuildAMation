#!/usr/bin/python

import os
import platform
import subprocess
import sys

def test_bam(build_dir):
    current_dir = os.getcwd()
    try:
        os.chdir(build_dir)
        print >>sys.stdout, "Starting tests in %s" % build_dir
        sys.stdout.flush()
        test_args = []
        test_args.append('./NuGetPackages/NUnit.ConsoleRunner.3.7.0/tools/nunit3-console.exe')
        test_args.append('--stoponerror')
        test_args.append('--noresult')
        test_args.append('Bam.Core.Test/bin/Debug/Bam.Core.Test.dll')
        if platform.system() != "Windows":
            test_args.insert(0, 'mono')
        print >>sys.stdout, "Running command: %s" % ' '.join(test_args)
        sys.stdout.flush()
        subprocess.check_call(test_args)
        print >>sys.stdout, "Finished tests"
        sys.stdout.flush()
    finally:
        os.chdir(current_dir)


if __name__ == "__main__":
    try:
        test_bam(os.getcwd())
    except Exception, e:
        print >>sys.stdout, "*** Test failure reason: %s" % str(e)
        sys.stdout.flush()
    print >>sys.stdout, "Done"
    sys.stdout.flush()
