#!/usr/bin/python
import os
import subprocess

class Builder(object):
    """Class that represents the actions for a builder"""
    def __init__(self, name, preAction, postAction):
        self.name = name
        self.preAction = preAction
        self.postAction = postAction

def MakeFilePost(package, options, outputMessages, errorMessages):
    """Post action for testing the MakeFile builder"""
    exitCode = 0
    buildRoot = os.path.join(package.GetPath(), options.buildRoot)
    makeFileDir = os.path.join(buildRoot, package.GetId())
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
    try:
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
