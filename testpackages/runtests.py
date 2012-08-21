#!/usr/bin/python

import sys
import os
import subprocess
import StringIO
from testconfigurations import TestConfiguration, GetTestConfig
from optparse import OptionParser

# ----------

class Package:
    root = None
    name = None
    version = None
  
    def __init__(self, root, name, version):
        self.root = root
        self.name = name
        self.version = version

    def GetDescription(self):
        return "%s-%s in %s" % (self.name, self.version, self.root)

    def GetPath(self):
        return os.path.join(self.root, self.name, self.version)

    def GetId(self):
        return "-".join([self.name, self.version]) 

# ----------

def FindAllPackagesToTest(root):
    """Locate packages that can be tested"""
    tests = []
    for packageName in os.listdir(root):
        if packageName.startswith("."):
            continue
        packageDir = os.path.join(root, packageName)
        if os.path.isdir(packageDir):
            for packageVersion in os.listdir(packageDir):
                if packageVersion.startswith("."):
                    continue
                versionDir = os.path.join(packageDir, packageVersion)
                if os.path.isdir(versionDir):
                    package = Package(root, packageName, packageVersion)
                    tests.append(package)
    return tests

def ExecuteTests(package, configuration, options, outputBuffer):
    if options.verbose:
        print "Package: ", package.GetDescription()
        print "Builders: ", configuration.GetBuilders()
        print "Response files: ", configuration.GetResponseFiles()
    if not options.builder in configuration.GetBuilders():
        outputBuffer.write("Package '%s' does not support the builder, '%s'" % (package.GetDescription(),options.builder))
        return 0
    for config in configuration.GetResponseFiles():
        argList = []
        argList.append("Opus")
        argList.append("@" + os.path.join(os.getcwd(), config))
        argList.append("-buildroot=" + options.buildRoot)
        argList.append("-builder=" + options.builder)
        if sys.platform.startswith("win"):
            argList.append("-platforms=" + ";".join(options.platforms))
            argList.append("-configurations=" + ";".join(options.configurations))
        else:
            argList.append("-platforms=" + ":".join(options.platforms))
            argList.append("-configurations=" + ":".join(options.configurations))
        argList.append("-j=" + str(options.numJobs))
        if options.debugSymbols:
            argList.append("-debugsymbols")
        argList.append("-verbosity=0")
        if options.verbose:
            print "\tExecuting: %s" % " ".join(argList)
        currentDir = os.getcwd()
        try:
            p = subprocess.Popen(argList, stdout=subprocess.PIPE, stderr=sys.stdout, cwd=package.GetPath())
            (output, error) = p.communicate() # this should WAIT
        except Exception, e:
            print "Popen exception: '%s'" % str(e)
            raise
        finally:
            os.chdir(currentDir)
            if not p.returncode:
                outputBuffer.write("Package '%s' succeeded in config '%s'\n" % (package.GetDescription(), config))
                return 0
            else:
                outputBuffer.write("Package '%s' failed in config '%s'\n" % (package.GetDescription(), config))
                outputBuffer.write("Command was: '%s'" % " ".join(argList))
                outputBuffer.write(output)
                return -1

def CleanUp(options):
    argList = []
    if sys.platform.startswith("win"):
        argList.append(os.path.join(os.getcwd(), "removedebugprojects.bat"))
        argList.append("-nopause")
    else:
        argList.append(os.path.join(os.getcwd(), "removedebugprojects.sh"))
    if options.verbose:
        print "Executing: ", argList
    p = subprocess.Popen(argList)
    p.wait()
    
# ----------
    
if __name__ == "__main__":
    optParser = OptionParser(description="Opus unittests")
    optParser.add_option("--platform", "-p", dest="platforms", action="append", default=None, help="Platforms to test")
    optParser.add_option("--configuration", "-c", dest="configurations", action="append", default=None, help="Configurations to test")
    optParser.add_option("--test", "-t", dest="tests", action="append", default=None, help="Tests to run")
    optParser.add_option("--buildroot", "-o", dest="buildRoot", action="store", default="build", help="Opus build root")
    optParser.add_option("--builder", "-b", dest="builder", action="store", default="Native", help="Opus builder to test")
    optParser.add_option("--keepfiles", "-k", dest="keepFiles", action="store_true", default=False, help="Keep the Opus build files around")
    optParser.add_option("--jobs", "-j", dest="numJobs", action="store", type="int", default=1, help="Number of jobs to use with Opus builds")
    optParser.add_option("--verbose", "-v", dest="verbose", action="store_true", default=False, help="Verbose output")
    optParser.add_option("--debug", "-d", dest="debugSymbols", action="store_true", default=False, help="Build Opus packages with debug information")
    (options,args) = optParser.parse_args()
    
    if options.verbose:
        print "Options are ", options
        print "Args    are ", args
        
    if not options.platforms:
        raise RuntimeError("No platforms were specified")
        
    if not options.configurations:
        raise RuntimeError("No configurations were specified")

    tests = FindAllPackagesToTest(os.getcwd())
    if not options.tests:
        if options.verbose:
            print "All tests will run"
    else:
        if options.verbose:
            print "Tests to run are: ", options.tests
        filteredTests = []
        for test in options.tests:
            found = False
            for package in tests:
                if package.GetId() == test:
                    filteredTests.append(package)
                    found = True
                    break
            if not found:
                raise RuntimeError("Unrecognized package '%s'" % test)
        tests = filteredTests

    outputBuffer = StringIO.StringIO()
    exitCode = 0
    for package in tests:
        config = GetTestConfig(package.GetId(), options)
        if not config:
            continue
        exitCode += ExecuteTests(package, config, options, outputBuffer)

    print "----------------------------------------"
    print "Results summary"
    print "----------------------------------------"
    print outputBuffer.getvalue()
    if not options.keepFiles:
        # TODO: consider keeping track of all directories created instead
        CleanUp(options)

    sys.exit(exitCode)
