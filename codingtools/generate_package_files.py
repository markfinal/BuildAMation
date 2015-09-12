#!/usr/bin/python

import fnmatch
import glob
import os
import subprocess
import sys

def generatePackageFiles(packageDir, failed_packages):
    for root, dirnames, filenames in os.walk(packageDir):
        for filename in fnmatch.filter(filenames, 'GenerateFiles.py'):
            pathToRun = os.path.join(root, filename)
            pathDir = os.path.dirname(pathToRun)
            curDir = os.getcwd()
            try:
                os.chdir(pathDir)
                args = []
                args.append(sys.executable)
                args.append('GenerateFiles.py')
                process = subprocess.Popen(args)
                process.wait()
                if process.returncode:
                    failed_packages.append((packageDir, process.returncode))
            finally:
                os.chdir(curDir)

def getPackagePathsFromRoot(packageRoot):
    files = glob.glob('%s/*/*' % packageRoot)
    dirs = filter(lambda f : os.path.isdir(f), files)
    return dirs

def processPath(packageRootList):
    failed_packages = []
    for packageRoot in packageRootList:
        packagePaths = getPackagePathsFromRoot(packageRoot)
        for package in packagePaths:
            generatePackageFiles(package, failed_packages)
    if failed_packages:
        print "Failed to upgrade some packages:"
        for packageDir,exitCode in failed_packages:
            print packageDir

if __name__ == "__main__":
    if len(sys.argv) > 1:
        processPath([sys.argv[1]])
    else:
        processPath(['packages', 'tests'])
