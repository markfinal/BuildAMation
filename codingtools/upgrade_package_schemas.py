#!/usr/bin/python

import glob
import os
import subprocess
import sys


def upgradeSchema(packageDir, failed_packages):
    current_dir = os.getcwd()
    try:
        print >>sys.stdout, "Updating '%s'" % packageDir
        sys.stdout.flush()
        os.chdir(packageDir)
        args = ['bam', '--showdefinition', '--forceupdates']
        process = subprocess.Popen(args)
        process.wait()
        if process.returncode:
            failed_packages.append((packageDir, process.returncode))
    finally:
        os.chdir(current_dir)


def getPackagePathsFromRoot(packageRoot):
    files = glob.glob('%s/*/*/bam' % packageRoot)
    dirs = [os.path.split(f)[0] for f in files if os.path.isdir(f)]
    return dirs


def processPath(packageRootList):
    failed_packages = []
    for packageRoot in packageRootList:
        packagePaths = getPackagePathsFromRoot(packageRoot)
        for package in packagePaths:
            upgradeSchema(package, failed_packages)
    if failed_packages:
        print >>sys.stdout, "Failed to upgrade some packages:"
        sys.stdout.flush()
        for packageDir, exitCode in failed_packages:
            print >>sys.stdout, packageDir
            sys.stdout.flush()

if __name__ == "__main__":
    if len(sys.argv) > 1:
        processPath(sys.argv[1:])
    else:
        processPath(['.'])
