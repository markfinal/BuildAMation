#!/usr/bin/python
import os
import subprocess
import sys
import traceback

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

def _openXcodeWorkspaceInIDE(workspacePath, outputMessages, errorMessages):
    """Open the Xcode workspace into the IDE. This will cache all of the project data, allowing xcodebuild command line invocations to function."""
    argList = []
    argList.append("open")
    argList.append("-a")
    argList.append("xcode")
    argList.append(workspacePath)
    # Note: the pid of this is not the pid of Xcode! Sigh!
    p = subprocess.Popen(argList, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    import time
    time.sleep(5) # TODO: this is bad, need to find a better way - Popen only waits for Xcode to start up, not for it to finish parsing the project
    (outputStream, errorStream) = p.communicate() # this should WAIT
    outputMessages.write(outputStream)
    errorMessages.write(errorStream)

def _writeXcodeWorkspaceSettings(workspacePath, outputMessages, errorMessages):
    """Write the default settings for the current user for the workspace."""
    realSettingsFilePath = os.path.join(workspacePath, "xcuserdata", "%s.xcuserdatad"%os.environ["USER"],"WorkspaceSettings.xcsettings")
    tempSettingsFilePath = realSettingsFilePath + ".plist"
    argList = []
    argList.append("defaults")
    argList.append("write")
    argList.append(tempSettingsFilePath)
    # build location style must use legacy settings currently, in order to locate dependents
    argList.append("BuildLocationStyle")
    argList.append("UseTargetSettings")
    p = subprocess.Popen(argList, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    (outputStream, errorStream) = p.communicate() # this should WAIT
    outputMessages.write(outputStream)
    errorMessages.write(errorStream)
    import shutil
    shutil.copyfile(tempSettingsFilePath, realSettingsFilePath)
    # TODO: this next statement causes failures with subsequent builds of the same package
    #os.remove(tempSettingsFilePath)

def XcodePost(package, options, outputMessages, errorMessages):
    """Post action for testing the Xcode builder"""
    exitCode = 0
    buildRoot = os.path.join(package.GetPath(), options.buildRoot)
    xcodeWorkspacePath = os.path.join(buildRoot, "*.xcworkspace")
    import glob
    workspaces = glob.glob(xcodeWorkspacePath)
    if not workspaces:
        # TODO: really need something different here - an invalid test result, rather than a failure
        outputMessages.write("Xcode workspace expected at %s did not exist" % xcodeWorkspacePath)
        return 0
    if len(workspaces) > 1:
        outputMessages.write("More than one Xcode workspace was found")
        return -1
    try:
        _writeXcodeWorkspaceSettings(workspaces[0], outputMessages, errorMessages)
        _openXcodeWorkspaceInIDE(workspaces[0], outputMessages, errorMessages)
        # first, list all the schemes available
        argList = []
        argList.append("xcodebuild")
        argList.append("-workspace")
        argList.append(workspaces[0])
        argList.append("-list")
        p = subprocess.Popen(argList, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        (outputStream, errorStream) = p.communicate() # this should WAIT
        outputMessages.write("executing %s\n" % ' '.join(argList))
        outputMessages.write(outputStream)
        errorMessages.write(errorStream)
        # parse the output to get the schemes
        lines = outputStream.split()
        schemes = []
        hasSchemes = False
        for line in lines:
            trimmed = line.strip()
            if hasSchemes:
                schemes.append(trimmed)
            elif trimmed.startswith('Schemes:'):
                hasSchemes = True
        # iterate over all the schemes and configurations
        for scheme in schemes:
            for config in options.configurations:
                argList = []
                argList.append("xcodebuild")
                argList.append("-workspace")
                argList.append(workspaces[0])
                argList.append("-scheme")
                argList.append(scheme)
                argList.append("-configuration")
                # capitalize the first letter of the configuration
                config = config[0].upper() + config[1:]
                argList.append(config)
                outputMessages.write("Running '%s' in %s" % (" ".join(argList), buildRoot))
                p = subprocess.Popen(argList, stdout=subprocess.PIPE, stderr=subprocess.PIPE, cwd=buildRoot)
                (outputStream, errorStream) = p.communicate() # this should WAIT
                exitCode |= p.returncode
                if outputStream:
                    outputMessages.write(outputStream)
                if errorStream:
                    errorMessages.write(errorStream)
    except Exception, e:
        errorMessages.write(str(e))
        errorMessages.write(traceback.format_exc())
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
