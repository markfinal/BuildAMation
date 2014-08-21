#!/usr/bin/python

import glob
import os
import subprocess
import sys

def upgradeSchema(packageDir):
    current_dir = os.getcwd()
    try:
        print "Update '%s'" % packageDir
        os.chdir(packageDir)
        args = [ 'bam', '-showdefinition', '-forcedefinitionupdate' ]
        process = subprocess.Popen(args)
        process.wait()
        if process.returncode:
            raise RuntimeError("Failed to upgrade package '%s' with exit status %d" % (packageDir, process.returncode))
    finally:
        os.chdir(current_dir)

def getPackagePathsFromRoot(packageRoot):
    files = glob.glob('%s/*/*' % packageRoot)
    dirs = filter(lambda f : os.path.isdir(f), files)
    return dirs

def processPath(packageRootList):
    for packageRoot in packageRootList:
        packagePaths = getPackagePathsFromRoot(packageRoot)
        for package in packagePaths:
            upgradeSchema(package)

if __name__ == "__main__":
    if len(sys.argv) > 1:
        upgradeSchema(sys.argv[1])
    else:
        processPath(['packages', 'testpackages'])
