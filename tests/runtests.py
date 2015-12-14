#!/usr/bin/python

from builderactions import GetBuilderDetails
import glob
import imp
from optparse import OptionParser
import os
import StringIO
import subprocess
import sys
from testconfigurations import TestOptionSetup
import time
import xml.etree.ElementTree as ET

# ----------

def PrintMessage(message):
    print >>sys.stdout, message
    sys.stdout.flush()

class Package:
    def __init__(self, root, name, version):
        self.root = root
        self.name = name
        self.version = version
        self.repo = None
        self.packageDir = None

    @classmethod
    def from_xml(cls, xml_filename):
        document = ET.parse(xml_filename)
        root = document.getroot()
        instance = cls(None, root.attrib["name"], root.attrib.get("version", None))
        instance.packageDir = os.path.normpath(os.path.join(xml_filename, os.pardir, os.pardir))
        instance.repo = os.path.normpath(os.path.join(instance.packageDir, os.pardir))
        return instance

    def GetDescription(self):
        if self.version:
            return "%s-%s in %s" % (self.name, self.version, self.repo)
        else:
            return "%s in %s" % (self.name, self.repo)

    def GetPath(self):
        return self.packageDir

    def GetId(self):
        if self.version:
            return "-".join([self.name, self.version])
        else:
            return self.name

    def GetName(self):
        return self.name

# ----------

def FindAllPackagesToTest(root, options):
    """Locate packages that can be tested
    Args:
        root:
        options:
    """
    if options.verbose:
        PrintMessage("Locating packages under '%s'" % root)
    tests = []
    dirs = os.listdir(root)
    dirs.sort()
    for packageName in dirs:
        if packageName.startswith("."):
            continue
        packageDir = os.path.join(root, packageName)
        if not os.path.isdir(packageDir):
            continue
        bamDir = os.path.join(packageDir, 'bam')
        if not os.path.isdir(bamDir):
            continue
        xmlFiles = glob.glob(os.path.join(bamDir, "*.xml"))
        if len(xmlFiles) == 0:
          continue
        if len(xmlFiles) > 1:
          raise RuntimeError("Too many XML files found in %s to identify a package definition file" % bamDir)
        package = Package.from_xml(xmlFiles[0])
        if options.verbose:
            PrintMessage("\t%s" % package.GetId())
        tests.append(package)
    return tests

def _preExecute(builder, options, flavour):
    if builder.preAction:
        builder.preAction()

def _runBuildAMation(options, package, extraArgs, outputMessages, errorMessages):
    argList = [
        "bam",
        "-o=%s" % options.buildRoot,
        "-b=%s" % options.buildmode
    ]
    for config in options.configurations:
        argList.append("--config=%s" % config)
    argList.append("-j=" + str(options.numJobs))
    if options.debugSymbols:
        argList.append("-d")
    if options.verbose:
        argList.append("-v=2")
    else:
        argList.append("-v=0")
    if options.forceDefinitionUpdate:
        argList.append("--forceupdates")
    if not options.noInitialClean:
        argList.append("--clean")
    if extraArgs:
        argList.extend(extraArgs)
    PrintMessage(" ".join(argList))
    p = subprocess.Popen(argList, stdout=subprocess.PIPE, stderr=subprocess.PIPE, cwd=package.GetPath())
    (outputStream, errorStream) = p.communicate() # this should WAIT
    if outputStream:
        outputMessages.write(outputStream)
    if errorStream:
        errorMessages.write(errorStream)
    return p.returncode, argList

def _postExecute(builder, options, flavour, package, outputMessages, errorMessages):
    if builder.postAction:
        exitCode = builder.postAction(package, options, flavour, outputMessages, errorMessages)
        return exitCode
    return 0

def ExecuteTests(package, configuration, options, args, outputBuffer):
    PrintMessage("Package           : %s" % package.GetId())
    if options.verbose:
        PrintMessage("Description          : %s" % package.GetDescription())
        PrintMessage("Available build modes: %s" % configuration.GetBuildModes())
    if not options.buildmode in configuration.GetBuildModes():
        outputBuffer.write("IGNORED: Package '%s' does not support build mode '%s' in the test configuration\n" % (package.GetDescription(),options.buildmode))
        PrintMessage("\tIgnored")
        return 0
    variationArgs = configuration.GetVariations(options.buildmode, options.excludeResponseFiles)
    if len(variationArgs) == 0:
        outputBuffer.write("IGNORED: Package '%s' has no response file with the current options\n" % package.GetDescription())
        PrintMessage("\tIgnored")
        return 0
    if options.verbose:
        PrintMessage("Response filenames: %s" % variationArgs)
        if options.excludeResponseFiles:
          PrintMessage(" (excluding %s)" % options.excludeResponseFiles)
    nonKWArgs = []
    theBuilder = GetBuilderDetails(options.buildmode)
    exitCode = 0
    for variation in variationArgs:
        iterations = 1
        versionArgs = None

        for it in range(0,iterations):
            extraArgs = nonKWArgs[:]
            if options.Flavours:
                extraArgs.extend(options.Flavours)
            if variation:
                extraArgs.extend(variation.GetArguments())
            try:
              outputMessages = StringIO.StringIO()
              errorMessages = StringIO.StringIO()
              _preExecute(theBuilder, options, variation)
              returncode, argList = _runBuildAMation(options, package, extraArgs, outputMessages, errorMessages)
              if returncode == 0:
                returncode = _postExecute(theBuilder, options, variation, package, outputMessages, errorMessages)
            except Exception, e:
                PrintMessage("Popen exception: '%s'" % str(e))
                raise
            finally:
                message = "Package '%s'" % package.GetDescription()
                if extraArgs:
                  message += " with extra arguments '%s'" % " ".join(extraArgs)
                if returncode == 0:
                    outputBuffer.write("SUCCESS: %s\n" % message)
                    if options.verbose:
                        if len(outputMessages.getvalue()) > 0:
                            outputBuffer.write("Messages:\n")
                            outputBuffer.write(outputMessages.getvalue())
                        if len(errorMessages.getvalue()) > 0:
                            outputBuffer.write("Errors:\n")
                            outputBuffer.write(errorMessages.getvalue())
                else:
                    outputBuffer.write("* FAILURE *: %s\n" % message)
                    outputBuffer.write("Command was: %s\n" % " ".join(argList))
                    outputBuffer.write("Executed in: %s\n" % package.GetPath())
                    if len(outputMessages.getvalue()) > 0:
                        outputBuffer.write("Messages:\n")
                        outputBuffer.write(outputMessages.getvalue())
                    if len(errorMessages.getvalue()) > 0:
                        outputBuffer.write("Errors:\n")
                        outputBuffer.write(errorMessages.getvalue())
                    outputBuffer.write("\n")
                    exitCode -= 1
    return exitCode

def CleanUp(options):
    argList = []
    if sys.platform.startswith("win"):
        argList.append(os.path.join(os.getcwd(), "removedebugprojects.bat"))
        argList.append("-nopause")
    else:
        argList.append(os.path.join(os.getcwd(), "removedebugprojects.sh"))
    if options.verbose:
        PrintMessage("Executing: %s" % argList)
    p = subprocess.Popen(argList)
    p.wait()

def FindBamDefaultRepository():
    for path in os.environ["PATH"].split(os.pathsep):
        candidatePath = os.path.join(path, "bam")
        if os.path.isfile(candidatePath) and os.access(candidatePath, os.X_OK):
            return os.path.abspath(os.path.join(os.path.join(path, os.pardir), os.pardir))
    raise RuntimeError("Unable to locate bam on the PATH")

# ----------

if __name__ == "__main__":
    bamDir = FindBamDefaultRepository()

    optParser = OptionParser(description="BuildAMation unittests")
    #optParser.add_option("--platform", "-p", dest="platforms", action="append", default=None, help="Platforms to test")
    optParser.add_option("--configuration", "-c", dest="configurations", action="append", default=None, help="Configurations to test")
    optParser.add_option("--test", "-t", dest="tests", action="append", default=None, help="Tests to run")
    optParser.add_option("--buildroot", "-o", dest="buildRoot", action="store", default="build", help="BuildAMation build root")
    optParser.add_option("--buildmode", "-b", dest="buildmode", action="store", default="Native", help="BuildAMation build mode to test")
    optParser.add_option("--keepfiles", "-k", dest="keepFiles", action="store_true", default=False, help="Keep the BuildAMation build files around")
    optParser.add_option("--jobs", "-j", dest="numJobs", action="store", type="int", default=1, help="Number of jobs to use with BuildAMation builds")
    optParser.add_option("--verbose", "-v", dest="verbose", action="store_true", default=False, help="Verbose output")
    optParser.add_option("--debug", "-d", dest="debugSymbols", action="store_true", default=False, help="Build BuildAMation packages with debug information")
    optParser.add_option("--noinitialclean", "-i", dest="noInitialClean", action="store_true", default=False, help="Disable cleaning packages before running tests")
    optParser.add_option("--forcedefinitionupdate", "-f", dest="forceDefinitionUpdate", action="store_true", default=False, help="Force definition file updates")
    optParser.add_option("--excluderesponsefiles", "-x", dest="excludeResponseFiles", action="append", default=None, help="Exclude response files")
    optParser.add_option("--repo", "-r", dest="repos", action="append", default=[bamDir], help="Add a package repository to test")
    optParser.add_option("--nodefaultrepo", dest="nodefaultrepo", action="store_true", default=False, help="Do not test the default repository")
    TestOptionSetup(optParser)
    (options,args) = optParser.parse_args()

    if options.nodefaultrepo:
        options.repos.remove(bamDir)
        if not options.repos:
            raise RuntimeError("No package repositories to test")

    if options.verbose:
        PrintMessage("Options are %s" % options)
        PrintMessage("Args    are %s" % args)

    #if not options.platforms:
    #    raise RuntimeError("No platforms were specified")

    if not options.configurations:
        raise RuntimeError("No configurations were specified")

    #if not options.noInitialClean:
    #    CleanUp(options)

    exitCode = 0
    for repo in options.repos:
        repoTestDir = os.path.join(repo, "tests")
        bamTestsConfigPathname = os.path.join(repoTestDir, 'bamtests.py')
        if not os.path.isfile(bamTestsConfigPathname):
            PrintMessage("Package repository %s has no bamtests.py file" % repo)
            continue
        bamtests = imp.load_source('bamtests', bamTestsConfigPathname)
        testConfigs = bamtests.ConfigureRepository()
        tests = FindAllPackagesToTest(repoTestDir, options)
        if not options.tests:
            if options.verbose:
                PrintMessage("All tests will run")
        else:
            if options.verbose:
                PrintMessage("Tests to run are: %s" % options.tests)
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
        for package in tests:
            try:
                config = testConfigs[package.GetId()]
            except KeyError, e:
                if options.verbose:
                    PrintMessage("No configuration for package: '%s'" % str(e))
                continue
            exitCode += ExecuteTests(package, config, options, args, outputBuffer)

        if not options.keepFiles:
            # TODO: consider keeping track of all directories created instead
            CleanUp(options)

        PrintMessage("--------------------")
        PrintMessage("| Results summary  |")
        PrintMessage("--------------------")
        PrintMessage(outputBuffer.getvalue())

        logsDir = os.path.join(repoTestDir, "Logs")
        if not os.path.isdir(logsDir):
            os.makedirs(logsDir)
        logFileName = os.path.join(logsDir, "tests_" + time.strftime("%d-%m-%YT%H-%M-%S") + ".log")
        logFile = open(logFileName, "w")
        logFile.write(outputBuffer.getvalue())
        logFile.close()
        outputBuffer.close()

    sys.exit(exitCode)
