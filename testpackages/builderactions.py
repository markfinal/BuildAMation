#!/usr/bin/python
import os
import subprocess
import sys

class Builder(object):
    """Class that represents the actions for a builder"""
    def __init__(self, name, preAction, postAction):
        self.name = name
        self.preAction = preAction
        self.postAction = postAction

def VSSolutionPost(package, options, outputMessages, errorMessages):
    """Post action for testing the VSSolution builder"""
    exitCode = 0
    buildRoot = os.path.join(package.GetPath(), options.buildRoot)
    slnPath = os.path.join(buildRoot, package.GetId(), package.GetId() + ".sln")
    if not os.path.exists(slnPath):
        # TODO: really need something different here - an invalid test result, rather than a failure
        outputMessages.write("ViisualStudio solution expected at %s did not exist" % slnPath)
        return 0
    try:
        for config in options.configurations:
            argList = []
            # TODO: Version of MSBuild.exe (which .NET framework) differs as to which version is needed to build a specific version of a .sln
            # TODO: this also builds relative to the solution, whereas it should build relative to the projects referenced
            argList.append(r"C:\Windows\Microsoft.NET\Framework\v3.5\MSBuild.exe")
            argList.append(slnPath)
            # capitalize the first letter of the configuration
            config = config[0].upper() + config[1:]
            argList.append("/p:Configuration=%s" % config)
            print "Running '%s' in %s" % (" ".join(argList), buildRoot)
            p = subprocess.Popen(argList, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
            (outputStream, errorStream) = p.communicate() # this should WAIT
            exitCode |= p.returncode
            if outputStream:
                outputMessages.write(outputStream)
            if errorStream:
                errorMessages.write(errorStream)
    except Exception, e:
        errorMessages.write(str(e))
        return -1
    return exitCode

def MakeFilePost(package, options, outputMessages, errorMessages):
    """Post action for testing the MakeFile builder"""
    if sys.platform.startswith("win"):
        # TODO: allow configuring where make is
        return 0
    exitCode = 0
    buildRoot = os.path.join(package.GetPath(), options.buildRoot)
    makeFileDir = os.path.join(buildRoot, package.GetId())
    if not os.path.exists(makeFileDir):
        # TODO: really need something different here - an invalid test result, rather than a failure
        outputMessages.write("Expected folder containing MakeFile %s did not exist" % makeFileDir)
        return 0
    try:
        # currently do not support building configurations separately
        argList = []
        argList.append("make")
        print "Running '%s' in %s" % (" ".join(argList), makeFileDir)
        p = subprocess.Popen(argList, stdout=subprocess.PIPE, stderr=subprocess.PIPE, cwd=makeFileDir)
        (outputStream, errorStream) = p.communicate() # this should WAIT
        exitCode |= p.returncode
        if outputStream:
            outputMessages.write(outputStream)
        if errorStream:
            errorMessages.write(errorStream)
    except Exception, e:
        errorMessages.write(str(e))
        return -1
    return exitCode

def XcodePost(package, options, outputMessages, errorMessages):
    """Post action for testing the Xcode builder"""
    exitCode = 0
    buildRoot = os.path.join(package.GetPath(), options.buildRoot)
    xcodeProjectPath = os.path.join(buildRoot, package.GetName() + ".xcodeproj")
    if not os.path.exists(xcodeProjectPath):
        # TODO: really need something different here - an invalid test result, rather than a failure
        outputMessages.write("Xcode project expected at %s did not exist" % xcodeProjectPath)
        return 0
    try:
        # TODO: to execute this on a workspace, do something like this
        # This is because Opus generated projects do not build their "default" setup and must resort to legacy (which doesn't really sound that legacy)
        # xcodebuild -workspace Test5.xcworkspace -scheme MyDynamicLibTestApp BuildLocationStyle=UseTargetSettings
        for config in options.configurations:
            argList = []
            argList.append("xcodebuild")
            argList.append("-alltargets")
            argList.append("-configuration")
            # capitalize the first letter of the configuration
            config = config[0].upper() + config[1:]
            argList.append(config)
            print "Running '%s' in %s" % (" ".join(argList), buildRoot)
            p = subprocess.Popen(argList, stdout=subprocess.PIPE, stderr=subprocess.PIPE, cwd=buildRoot)
            (outputStream, errorStream) = p.communicate() # this should WAIT
            exitCode |= p.returncode
            if outputStream:
                outputMessages.write(outputStream)
            if errorStream:
                errorMessages.write(errorStream)
    except Exception, e:
        errorMessages.write(str(e))
        return -1
    return exitCode

builder = {}
builder["Native"] = Builder("Native", None, None)
builder["VSSolution"] = Builder("VSSolution", None, None)
builder["MakeFile"] = Builder("MakeFile", None, MakeFilePost)
builder["QMake"] = Builder("QMake", None, None)
builder["Xcode"] = Builder("Xcode", None, XcodePost)

def GetBuilderDetails(builderName):
    """Return the Builder associated with the name passed in"""
    return builder[builderName]
