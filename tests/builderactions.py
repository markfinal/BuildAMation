#!/usr/bin/python
import copy
import multiprocessing
import os
import subprocess
import sys
import tempfile
import time
import traceback


class Builder(object):
    """Class that represents the actions for a builder"""
    def __init__(self, repeat_no_clean):
        self.repeat_no_clean = repeat_no_clean
        self.num_threads = multiprocessing.cpu_count()

    def init(self, options):
        pass

    def pre_action(self):
        pass

    def post_action(self, instance, options, output_messages, error_messages):
        return 0

    def dump_generated_files(self, instance, options):
        pass

    def _execute_and_capture(self, arg_list, working_dir, output_messages, error_messages):
        locallog("Running '%s' in %s" % (' '.join(arg_list), working_dir))
        return_code = 0
        out_fd, out_path = tempfile.mkstemp()
        err_fd, err_path = tempfile.mkstemp()
        try:
            with os.fdopen(out_fd, 'w') as out:
                with os.fdopen(err_fd, 'w') as err:
                    p = subprocess.Popen(arg_list, stdout=out_fd, stderr=err_fd, cwd=working_dir)
                    while p.poll() is None:
                        sys.stdout.write('+') # keep something alive on the console
                        sys.stdout.flush()
                        time.sleep(1)
                    p.wait()
                    locallog('')
                    return_code = p.returncode
        except Exception:
            error_messages.write("Failed to run '%s' in %s" % (' '.join(arg_list), working_dir))
            raise
        finally:
            with open(out_path) as out:
                output_messages.write(out.read())
            with open(err_path) as err:
                error_messages.write(err.read())
            os.remove(out_path)
            os.remove(err_path)
        return return_code


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
    "15.0": "15.0",
    "16"  : "Current"
}
visualStudioVersionMapping = {
    "15.0": "2017",
    "16"  : "2019"
}


def locallog(message):
    sys.stdout.write(message)
    sys.stdout.write('\n')
    sys.stdout.flush()


class NativeBuilder(Builder):
    def __init__(self):
        super(NativeBuilder, self).__init__(True)


class VSSolutionBuilder(Builder):
    def __init__(self):
        super(VSSolutionBuilder, self).__init__(False)
        self._ms_build_path = None

    def _get_visualc_version(self, options):
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
        return visualc_version, visualc_version_split

    def _get_visualc_ispreview(self, options):
        try:
            for f in options.Flavours:
                if f.startswith("--VisualC.discoverprereleases"):
                    return True
        except:
            return False

    def init(self, options):
        visualc_version, visualc_version_split = self._get_visualc_version(options)
        visualc_major_version = int(visualc_version_split[0])
        # location of MSBuild changed in VS2013, and VS2017
        if visualc_major_version >= 15:
            visualStudioVersion = visualStudioVersionMapping[visualc_version]
            msbuild_version = msBuildVersionToNetMapping[visualc_version]
            edition = "Preview" if self._get_visualc_ispreview(options) else "Community"
            if os.environ.has_key("ProgramFiles(x86)"):
                self._ms_build_path = r"C:\Program Files (x86)\Microsoft Visual Studio\%s\%s\MSBuild\%s\Bin\MSBuild.exe" % (visualStudioVersion, edition, msbuild_version)
            else:
                self._ms_build_path = r"C:\Program Files (x86)\Microsoft Visual Studio\%s\%s\MSBuild\%s\Bin\amd64\MSBuild.exe" % (visualStudioVersion, edition, msbuild_version)
        elif visualc_major_version >= 12:
            # VS2013 onwards path for MSBuild
            if os.environ.has_key("ProgramFiles(x86)"):
                self._ms_build_path = r"C:\Program Files (x86)\MSBuild\%s\bin\MSBuild.exe" % visualc_version
            else:
                self._ms_build_path = r"C:\Program Files\MSBuild\%s\bin\MSBuild.exe" % visualc_version
        else:
            self._ms_build_path = r"C:\Windows\Microsoft.NET\Framework\%s\MSBuild.exe" % msBuildVersionToNetMapping[visualc_version]

    def post_action(self, instance, options, output_messages, error_messages):
        exit_code = 0
        build_root = os.path.join(instance.package_path(), options.buildRoot)
        solution_path = os.path.join(build_root, instance.package_name() + ".sln")
        if not os.path.exists(solution_path):
            # TODO: really need something different here - an invalid test result, rather than a failure
            output_messages.write("VisualStudio solution expected at %s did not exist" % solution_path)
            return 0
        for config in options.configurations:
            arg_list = [
                self._ms_build_path,
                "/m:%d" % self.num_threads,
                "/verbosity:normal",
                solution_path
            ]
            # capitalize the first letter of the configuration
            config = config[0].upper() + config[1:]
            arg_list.append("/p:Configuration=%s" % config)
            for platform in instance.platforms():
                this_arg_list = copy.deepcopy(arg_list)
                this_arg_list.append("/p:Platform=%s" % platform)
                exit_code |= self._execute_and_capture(this_arg_list, build_root, output_messages, error_messages)
        return exit_code

    def dump_generated_files(self, instance, options):
        build_root = os.path.join(instance.package_path(), options.buildRoot)
        vcxproj_project_path = os.path.join(build_root, "*.vcxproj")
        import glob
        projects = glob.glob(xcode_project_path)
        for project in projects:
            f = open(project, 'r')
            file_contents = f.read()
            f.close()
            locallog("------------------------------")
            locallog("VisualStudio project '%s': " % path)
            locallog(file_contents)


class MakeFileBuilder(Builder):
    def __init__(self):
        super(MakeFileBuilder, self).__init__(False)
        self._make_executable = 'make'
        self._make_args = []

    def init(self, options):
        if sys.platform.startswith("win"):
            arg_list = [
                'where',
                '/R',
                os.path.expandvars('%ProgramFiles(x86)%'),
                'nmake.exe'
            ]
            p = subprocess.Popen(arg_list, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
            (output_stream, error_stream) = p.communicate()  # this should WAIT
            if output_stream:
                split = output_stream.splitlines()
                self._make_executable = split[0].strip()
                self._make_args.append('-NOLOGO')
        else:
            self._make_args.append("-j")
            self._make_args.append(str(self.num_threads))

    def post_action(self, instance, options, output_messages, error_messages):
        makefile_dir = os.path.join(instance.package_path(), options.buildRoot)
        if not os.path.exists(makefile_dir):
            # TODO: really need something different here - an invalid test result, rather than a failure
            output_messages.write("Expected folder containing MakeFile %s did not exist" % makefile_dir)
            return 0
        # currently do not support building configurations separately
        arg_list = [
            self._make_executable
        ]
        arg_list.extend(self._make_args)
        return self._execute_and_capture(arg_list, makefile_dir, output_messages, error_messages)

    def dump_generated_files(self, instance, options):
        makefile_dir = os.path.join(instance.package_path(), options.buildRoot)
        path = os.path.join(makefile_dir, 'Makefile')
        f = open(path, 'r')
        file_contents = f.read()
        f.close()
        locallog("------------------------------")
        locallog("Makefile '%s': " % path)
        locallog(file_contents)


class XcodeBuilder(Builder):
    def __init__(self):
        super(XcodeBuilder, self).__init__(False)

    def post_action(self, instance, options, output_messages, error_messages):
        exit_code = 0
        build_root = os.path.join(instance.package_path(), options.buildRoot)
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
        # first, list all the schemes available
        arg_list = [
            "xcodebuild",
            "-workspace",
            workspaces[0],
            "-list"
        ]
        locallog("Running '%s'\n" % ' '.join(arg_list))
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
                    "-jobs",
                    str(self.num_threads),
                    "-workspace",
                    workspaces[0],
                    "-scheme",
                    scheme,
                    "-configuration"
                ]
                # capitalize the first letter of the configuration
                config = config[0].upper() + config[1:]
                arg_list.append(config)
                exit_code |= self._execute_and_capture(arg_list, build_root, output_messages, error_messages)
        return exit_code

    def dump_generated_files(self, instance, options):
        build_root = os.path.join(instance.package_path(), options.buildRoot)
        xcode_project_path = os.path.join(build_root, "*.xcodeproj")
        import glob
        projects = glob.glob(xcode_project_path)
        for project in projects:
            path = os.path.join(project, 'project.pbxproj')
            f = open(path, 'r')
            file_contents = f.read()
            f.close()
            locallog("------------------------------")
            locallog("Xcode project '%s': " % path)
            locallog(file_contents)


builder = {
    "Native": NativeBuilder(),
    "VSSolution": VSSolutionBuilder(),
    "MakeFile": MakeFileBuilder(),
    "Xcode": XcodeBuilder()
}


def get_builder_details(builder_name):
    """Return the Builder associated with the name passed in
    Args:
        builder_name:
    """
    return builder[builder_name]
