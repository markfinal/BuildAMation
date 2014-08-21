#!/usr/bin/python

import glob
import os
import subprocess
import sys

def upgradeSchema(packageDir, failed_packages):
    current_dir = os.getcwd()
    try:
        print "Update '%s'" % packageDir
        os.chdir(packageDir)
        args = [ 'bam', '-showdefinition', '-forcedefinitionupdate' ]
        process = subprocess.Popen(args)
        process.wait()
        if process.returncode:
            failed_packages.append((packageDir, process.returncode))
    finally:
        os.chdir(current_dir)

def getPackagePathsFromRoot(packageRoot):
    files = glob.glob('%s/*/*' % packageRoot)
    dirs = filter(lambda f : os.path.isdir(f), files)
    return dirs

def processPath(packageRootList):
    failed_packages = []
    for packageRoot in packageRootList:
        packagePaths = getPackagePathsFromRoot(packageRoot)
        for package in packagePaths:
            upgradeSchema(package, failed_packages)
    if failed_packages:
        print "Failed to upgrade some packages:"
        for packageDir,exitCode in failed_packages:
            print packageDir

if __name__ == "__main__":
    if len(sys.argv) > 1:
        processPath([sys.argv[1]])
    else:
        processPath(['packages', 'testpackages'])
