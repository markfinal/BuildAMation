#!/usr/bin/python
import copy
import os
import subprocess
import sys
import traceback


class Builder(object):
    """Class that represents the actions for a builder"""
    def __init__(self, name, pre_action, post_action):
        self.name = name
        self.pre_action = pre_action
        self.post_action = post_action

# the version of MSBuild.exe to use, depends on which version of VisualStudio
# was used to build the solution and projects
# by default, VS2013 is assumed
defaultVCVersion = "12.0"
msBuildVersionToNetMapping = {
    "9.0": "v3.5",
    "10.0": "v4.0.30319",
    "11.0": "v4.0.30319",
    "12.0": "v4.0.30319",
    "14.0": "14.0",
    "15.0": "15.0"
}


def vssolution_post(package, options, flavour, output_messages, error_messages):
    """Post action for testing the VSSolution builder
    Args:
        package:
        options:
        flavour:
        output_messages:
        error_messages:
    """
    exit_code = 0
    build_root = os.path.join(package.get_path(), options.buildRoot)
    solution_path = os.path.join(build_root, package.get_id() + ".sln")
    if not os.path.exists(solution_path):
        # TODO: really need something different here - an invalid test result, rather than a failure
        output_messages.write("VisualStudio solution expected at %s did not exist" % solution_path)
        return 0
    try:
        try:
            for f in options.Flavours:
                if f.startswith("--VisualC.version"):
                    visualc_version = f.split("=")[1]
                    break
        except TypeError:  # Flavours can be None
            pass
        try:
            visualc_version_split = visualc_version.split('.')
        except UnboundLocalError:
            visualc_version = defaultVCVersion
            visualc_version_split = visualc_version.split('.')
        visualc_major_version = int(visualc_version_split[0])
        # location of MSBuild changed in VS2013, and VS2017
        if visualc_major_version >= 15:
            if os.environ.has_key("ProgramFiles(x86)"):
                ms_build_path = r"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\%s\Bin\MSBuild.exe" % visualc_version
            else:
                ms_build_path = r"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\%s\Bin\amd64\MSBuild.exe" % visualc_version
        elif visualc_major_version >= 12:
            # VS2013 onwards path for MSBuild
            if os.environ.has_key("ProgramFiles(x86)"):
                ms_build_path = r"C:\Program Files (x86)\MSBuild\%s\bin\MSBuild.exe" % visualc_version
            else:
                ms_build_path = r"C:\Program Files\MSBuild\%s\bin\MSBuild.exe" % visualc_version
        else:
            ms_build_path = r"C:\Windows\Microsoft.NET\Framework\%s\MSBuild.exe" % msBuildVersionToNetMapping[visualc_version]
        for config in options.configurations:
            arg_list = [
                ms_build_path,
                "/verbosity:normal",
                solution_path
            ]
            # capitalize the first letter of the configuration
            config = config[0].upper() + config[1:]
            arg_list.append("/p:Configuration=%s" % config)
            for platform in flavour.platforms():
                this_arg_list = copy.deepcopy(arg_list)
                this_arg_list.append("/p:Platform=%s" % platform)
                print "Running '%s'\n" % ' '.join(this_arg_list)
                p = subprocess.Popen(this_arg_list, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
                (output_stream, error_stream) = p.communicate()  # this should WAIT
                exit_code |= p.returncode
                if output_stream:
                    output_messages.write(output_stream)
                if error_stream:
                    error_messages.write(error_stream)
    except Exception, e:
        import traceback
        error_messages.write(str(e) + '\n' + traceback.format_exc())
        return -1
    return exit_code


def makefile_post(package, options, flavour, output_messages, error_messages):
    """Post action for testing the MakeFile builder
    Args:
        package:
        options:
        flavour:
        output_messages:
        error_messages:
    """
    if sys.platform.startswith("win"):
        # TODO: allow configuring where make is
        return 0
    exit_code = 0
    makefile_dir = os.path.join(package.get_path(), options.buildRoot)
    if not os.path.exists(makefile_dir):
        # TODO: really need something different here - an invalid test result, rather than a failure
        output_messages.write("Expected folder containing MakeFile %s did not exist" % makefile_dir)
        return 0
    try:
        # currently do not support building configurations separately
        arg_list = [
            "make"
        ]
        print "Running '%s' in %s\n" % (' '.join(arg_list), makefile_dir)
        p = subprocess.Popen(arg_list, stdout=subprocess.PIPE, stderr=subprocess.PIPE, cwd=makefile_dir)
        (output_stream, error_stream) = p.communicate()  # this should WAIT
        exit_code |= p.returncode
        if output_stream:
            output_messages.write(output_stream)
        if error_stream:
            error_messages.write(error_stream)
    except Exception, e:
        error_messages.write(str(e))
        return -1
    return exit_code


def xcode_post(package, options, flavour, output_messages, error_messages):
    """Post action for testing the Xcode builder
    Args:
        package:
        options:
        flavour:
        output_messages:
        error_messages:
    """
    exit_code = 0
    build_root = os.path.join(package.get_path(), options.buildRoot)
    xcode_workspace_path = os.path.join(build_root, "*.xcworkspace")
    import glob
    workspaces = glob.glob(xcode_workspace_path)
    if not workspaces:
        # TODO: really need something different here - an invalid test result, rather than a failure
        output_messages.write("Xcode workspace expected at %s did not exist" % xcode_workspace_path)
        return 0
    if len(workspaces) > 1:
        output_messages.write("More than one Xcode workspace was found")
        return -1
    try:
        # first, list all the schemes available
        arg_list = [
            "xcodebuild",
            "-workspace",
            workspaces[0],
            "-list"
        ]
        print "Running '%s'\n" % ' '.join(arg_list)
        p = subprocess.Popen(arg_list, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        (output_stream, error_stream) = p.communicate()  # this should WAIT
        output_messages.write(output_stream)
        error_messages.write(error_stream)
        # parse the output to get the schemes
        lines = output_stream.split('\n')
        if len(lines) < 3:
            raise RuntimeError("Unable to parse workspace for schemes. \
                               Was --Xcode.generateSchemes passed to the Bam build?")
        schemes = []
        has_schemes = False
        for line in lines:
            trimmed = line.strip()
            if has_schemes:
                if trimmed:
                    schemes.append(trimmed)
            elif trimmed.startswith('Schemes:'):
                has_schemes = True
        if not has_schemes or len(schemes) == 0:
            raise RuntimeError("No schemes were extracted from the workspace. \
                            Has the project scheme cache been warmed?")
        # iterate over all the schemes and configurations
        for scheme in schemes:
            for config in options.configurations:
                arg_list = [
                    "xcodebuild",
                    "-workspace",
                    workspaces[0],
                    "-scheme",
                    scheme,
                    "-configuration"
                ]
                # capitalize the first letter of the configuration
                config = config[0].upper() + config[1:]
                arg_list.append(config)
                print "Running '%s' in %s" % (" ".join(arg_list), build_root)
                output_messages.write("Running '%s' in %s" % (" ".join(arg_list), build_root))
                p = subprocess.Popen(arg_list, stdout=subprocess.PIPE, stderr=subprocess.PIPE, cwd=build_root)
                (output_stream, error_stream) = p.communicate()  # this should WAIT
                exit_code |= p.returncode
                if output_stream:
                    output_messages.write(output_stream)
                if error_stream:
                    error_messages.write(error_stream)
    except Exception, e:
        error_messages.write("%s\n" % str(e))
        error_messages.write(traceback.format_exc())
        return -1
    return exit_code

builder = {
    "Native": Builder("Native", None, None),
    "VSSolution": Builder("VSSolution", None, vssolution_post),
    "MakeFile": Builder("MakeFile", None, makefile_post),
    "QMake": Builder("QMake", None, None),
    "Xcode": Builder("Xcode", None, xcode_post)
}


def get_builder_details(builder_name):
    """Return the Builder associated with the name passed in
    Args:
        builder_name:
    """
    return builder[builder_name]
