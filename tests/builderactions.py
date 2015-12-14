#!/usr/bin/python
import copy
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

# the version of MSBuild.exe to use, depends on which version of VisualStudio
# was used to build the solution and projects
# by default, VS2013 is assumed
defaultVCVersion = "12.0"
msBuildVersionToNetMapping = {
    "9.0": "v3.5",
    "10.0": "v4.0.30319",
    "11.0": "v4.0.30319",
    "12.0": "v4.0.30319",
    "14.0": "14.0"
}


def VSSolutionPost(package, options, flavour, outputMessages, errorMessages):
    """Post action for testing the VSSolution builder
    Args:
        package:
        options:
        flavour:
        outputMessages:
        errorMessages:
    """
    exitCode = 0
    buildRoot = os.path.join(package.GetPath(), options.buildRoot)
    slnPath = os.path.join(buildRoot, package.GetId() + ".sln")
    if not os.path.exists(slnPath):
        # TODO: really need something different here - an invalid test result, rather than a failure
        outputMessages.write("VisualStudio solution expected at %s did not exist" % slnPath)
        return 0
    try:
        try:
            for f in options.Flavours:
                if f.startswith("--VisualC.version"):
                    vcVersion = f.split("=")[1]
                    break
        except:
            pass
        finally:
            try:
                vcVersion
            except:
                vcVersion = defaultVCVersion
        vcVersionSplit = vcVersion.split('.')
        vcMajorVersion = int(vcVersionSplit[0])
        # location of MSBuild changed in VS2013
        if vcMajorVersion >= 12:
            # VS2013 onwards path for MSBuild
            msBuildPath = r"C:\Program Files (x86)\MSBuild\%s\bin\MSBuild.exe" % vcVersion
        else:
            msBuildPath = r"C:\Windows\Microsoft.NET\Framework\%s\MSBuild.exe" % msBuildVersionToNetMapping[vcVersion]
        for config in options.configurations:
            argList = [
                msBuildPath,
                "/verbosity:normal",
                slnPath
            ]
            # capitalize the first letter of the configuration
            config = config[0].upper() + config[1:]
            argList.append("/p:Configuration=%s" % config)
            for platform in flavour.platforms():
                thisArgList = copy.deepcopy(argList)
                thisArgList.append("/p:Platform=%s" % platform)
                print "Running '%s'\n" % ' '.join(thisArgList)
                p = subprocess.Popen(thisArgList, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
                (outputStream, errorStream) = p.communicate()  # this should WAIT
                exitCode |= p.returncode
                if outputStream:
                    outputMessages.write(outputStream)
                if errorStream:
                    errorMessages.write(errorStream)
    except Exception, e:
        import traceback
        errorMessages.write(str(e) + '\n' + traceback.format_exc())
        return -1
    return exitCode


def MakeFilePost(package, options, flavour, outputMessages, errorMessages):
    """Post action for testing the MakeFile builder
    Args:
        package:
        options:
        flavour:
        outputMessages:
        errorMessages:
    """
    if sys.platform.startswith("win"):
        # TODO: allow configuring where make is
        return 0
    exitCode = 0
    makeFileDir = os.path.join(package.GetPath(), options.buildRoot)
    if not os.path.exists(makeFileDir):
        # TODO: really need something different here - an invalid test result, rather than a failure
        outputMessages.write("Expected folder containing MakeFile %s did not exist" % makeFileDir)
        return 0
    try:
        # currently do not support building configurations separately
        argList = [
            "make"
        ]
        print "Running '%s' in %s\n" % (' '.join(argList), makeFileDir)
        p = subprocess.Popen(argList, stdout=subprocess.PIPE, stderr=subprocess.PIPE, cwd=makeFileDir)
        (outputStream, errorStream) = p.communicate()  # this should WAIT
        exitCode |= p.returncode
        if outputStream:
            outputMessages.write(outputStream)
        if errorStream:
            errorMessages.write(errorStream)
    except Exception, e:
        errorMessages.write(str(e))
        return -1
    return exitCode


def XcodePost(package, options, flavour, outputMessages, errorMessages):
    """Post action for testing the Xcode builder
    Args:
        package:
        options:
        flavour:
        outputMessages:
        errorMessages:
    """
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
        # first, list all the schemes available
        argList = [
            "xcodebuild",
            "-workspace",
            workspaces[0],
            "-list"
        ]
        print "Running '%s'\n" % ' '.join(argList)
        p = subprocess.Popen(argList, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        (outputStream, errorStream) = p.communicate()  # this should WAIT
        outputMessages.write(outputStream)
        errorMessages.write(errorStream)
        # parse the output to get the schemes
        lines = outputStream.split('\n')
        if len(lines) < 3:
            raise RuntimeError("Unable to parse workspace for schemes. \
                               Was --Xcode.generateSchemes passed to the Bam build?")
        schemes = []
        hasSchemes = False
        for line in lines:
            trimmed = line.strip()
            if hasSchemes:
                if trimmed:
                    schemes.append(trimmed)
            elif trimmed.startswith('Schemes:'):
                hasSchemes = True
        if not hasSchemes or len(schemes) == 0:
            raise RuntimeError("No schemes were extracted from the workspace. \
                            Has the project scheme cache been warmed?")
        # iterate over all the schemes and configurations
        for scheme in schemes:
            for config in options.configurations:
                argList = [
                    "xcodebuild",
                    "-workspace",
                    workspaces[0],
                    "-scheme",
                    scheme,
                    "-configuration"
                ]
                # capitalize the first letter of the configuration
                config = config[0].upper() + config[1:]
                argList.append(config)
                outputMessages.write("Running '%s' in %s" % (" ".join(argList), buildRoot))
                p = subprocess.Popen(argList, stdout=subprocess.PIPE, stderr=subprocess.PIPE, cwd=buildRoot)
                (outputStream, errorStream) = p.communicate()  # this should WAIT
                exitCode |= p.returncode
                if outputStream:
                    outputMessages.write(outputStream)
                if errorStream:
                    errorMessages.write(errorStream)
    except Exception, e:
        errorMessages.write("%s\n" % str(e))
        errorMessages.write(traceback.format_exc())
        return -1
    return exitCode

builder = {
    "Native": Builder("Native", None, None),
    "VSSolution": Builder("VSSolution", None, VSSolutionPost),
    "MakeFile": Builder("MakeFile", None, MakeFilePost),
    "QMake": Builder("QMake", None, None),
    "Xcode": Builder("Xcode", None, XcodePost)
}


def GetBuilderDetails(builderName):
    """Return the Builder associated with the name passed in
    Args:
        builderName:
    """
    return builder[builderName]
