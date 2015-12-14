#!/usr/bin/python

from builderactions import get_builder_details
import glob
import imp
from optparse import OptionParser
import os
import StringIO
import subprocess
import sys
from testconfigurations import test_option_setup
import time
import xml.etree.ElementTree as ET

# ----------


def print_message(message):
    print >>sys.stdout, message
    sys.stdout.flush()


class Package:
    def __init__(self, root, name, version):
        self.root = root
        self.name = name
        self.version = version
        self.repo = None
        self.package_dir = None

    @classmethod
    def from_xml(cls, xml_filename):
        document = ET.parse(xml_filename)
        root = document.getroot()
        instance = cls(None, root.attrib["name"], root.attrib.get("version", None))
        instance.package_dir = os.path.normpath(os.path.join(xml_filename, os.pardir, os.pardir))
        instance.repo = os.path.normpath(os.path.join(instance.package_dir, os.pardir))
        return instance

    def get_description(self):
        if self.version:
            return "%s-%s in %s" % (self.name, self.version, self.repo)
        else:
            return "%s in %s" % (self.name, self.repo)

    def get_path(self):
        return self.package_dir

    def get_id(self):
        if self.version:
            return "-".join([self.name, self.version])
        else:
            return self.name

    def get_name(self):
        return self.name

# ----------


def find_all_packages_to_test(root, options):
    """Locate packages that can be tested
    Args:
        root:
        options:
    """
    if options.verbose:
        print_message("Locating packages under '%s'" % root)
    tests = []
    dirs = os.listdir(root)
    dirs.sort()
    for packageName in dirs:
        if packageName.startswith("."):
            continue
        package_dir = os.path.join(root, packageName)
        if not os.path.isdir(package_dir):
            continue
        bam_dir = os.path.join(package_dir, 'bam')
        if not os.path.isdir(bam_dir):
            continue
        xml_files = glob.glob(os.path.join(bam_dir, "*.xml"))
        if len(xml_files) == 0:
            continue
        if len(xml_files) > 1:
            raise RuntimeError("Too many XML files found in %s to identify a package definition file" % bam_dir)
        package = Package.from_xml(xml_files[0])
        if options.verbose:
            print_message("\t%s" % package.get_id())
        tests.append(package)
    return tests


def _pre_execute(builder):
    if builder.pre_action:
        builder.pre_action()


def _run_buildamation(options, package, extra_args, output_messages, error_messages):
    arg_list = [
        "bam",
        "-o=%s" % options.buildRoot,
        "-b=%s" % options.buildmode
    ]
    for config in options.configurations:
        arg_list.append("--config=%s" % config)
    arg_list.append("-j=" + str(options.numJobs))
    if options.debugSymbols:
        arg_list.append("-d")
    if options.verbose:
        arg_list.append("-v=2")
    else:
        arg_list.append("-v=0")
    if options.forceDefinitionUpdate:
        arg_list.append("--forceupdates")
    if not options.noInitialClean:
        arg_list.append("--clean")
    if extra_args:
        arg_list.extend(extra_args)
    print_message(" ".join(arg_list))
    p = subprocess.Popen(arg_list, stdout=subprocess.PIPE, stderr=subprocess.PIPE, cwd=package.get_path())
    (output_stream, error_stream) = p.communicate()  # this should WAIT
    if output_stream:
        output_messages.write(output_stream)
    if error_stream:
        error_messages.write(error_stream)
    return p.returncode, arg_list


def _post_execute(builder, options, flavour, package, output_messages, error_messages):
    if builder.post_action:
        exit_code = builder.post_action(package, options, flavour, output_messages, error_messages)
        return exit_code
    return 0


def execute_tests(package, configuration, options, output_buffer):
    print_message("Package           : %s" % package.get_id())
    if options.verbose:
        print_message("Description          : %s" % package.get_description())
        print_message("Available build modes: %s" % configuration.get_build_modes())
    if options.buildmode not in configuration.get_build_modes():
        output_buffer.write("IGNORED: Package '%s' does not support build mode '%s' in the test configuration\n" %
                            (package.get_description(), options.buildmode))
        print_message("\tIgnored")
        return 0
    variation_args = configuration.get_variations(options.buildmode, options.excludedVariations)
    if len(variation_args) == 0:
        output_buffer.write("IGNORED: Package '%s' has no configuration with the current options\n" %
                            package.get_description())
        print_message("\tIgnored")
        return 0
    if options.verbose:
        print_message("Test configurations: %s" % variation_args)
        if options.excludedVariations:
            print_message(" (excluding %s)" % options.excludedVariations)
    non_kwargs = []
    the_builder = get_builder_details(options.buildmode)
    exit_code = 0
    for variation in variation_args:
        iterations = 1

        for it in range(0, iterations):
            extra_args = non_kwargs[:]
            if options.Flavours:
                extra_args.extend(options.Flavours)
            if variation:
                extra_args.extend(variation.get_arguments())
            try:
                output_messages = StringIO.StringIO()
                error_messages = StringIO.StringIO()
                _pre_execute(the_builder)
                returncode, arg_list = _run_buildamation(options, package, extra_args, output_messages, error_messages)
                if returncode == 0:
                    returncode = _post_execute(the_builder, options, variation, package, output_messages, error_messages)
            except Exception, e:
                print_message("Popen exception: '%s'" % str(e))
                raise
            finally:
                message = "Package '%s'" % package.get_description()
                if extra_args:
                    message += " with extra arguments '%s'" % " ".join(extra_args)
                try:
                    if returncode == 0:
                        output_buffer.write("SUCCESS: %s\n" % message)
                        if options.verbose:
                            if len(output_messages.getvalue()) > 0:
                                output_buffer.write("Messages:\n")
                                output_buffer.write(output_messages.getvalue())
                            if len(error_messages.getvalue()) > 0:
                                output_buffer.write("Errors:\n")
                                output_buffer.write(error_messages.getvalue())
                    else:
                        output_buffer.write("* FAILURE *: %s\n" % message)
                        output_buffer.write("Command was: %s\n" % " ".join(arg_list))
                        output_buffer.write("Executed in: %s\n" % package.get_path())
                        if len(output_messages.getvalue()) > 0:
                            output_buffer.write("Messages:\n")
                            output_buffer.write(output_messages.getvalue())
                        if len(error_messages.getvalue()) > 0:
                            output_buffer.write("Errors:\n")
                            output_buffer.write(error_messages.getvalue())
                        output_buffer.write("\n")
                        exit_code -= 1
                except UnboundLocalError:  # for returncode
                    message += "... did not complete due to earlier errors"
    return exit_code


def clean_up(options):
    arg_list = []
    if sys.platform.startswith("win"):
        arg_list.append(os.path.join(os.getcwd(), "removedebugprojects.bat"))
        arg_list.append("-nopause")
    else:
        arg_list.append(os.path.join(os.getcwd(), "removedebugprojects.sh"))
    if options.verbose:
        print_message("Executing: %s" % arg_list)
    p = subprocess.Popen(arg_list)
    p.wait()


def find_bam_default_repository():
    for path in os.environ["PATH"].split(os.pathsep):
        candidate_path = os.path.join(path, "bam")
        if os.path.isfile(candidate_path) and os.access(candidate_path, os.X_OK):
            return os.path.abspath(os.path.join(os.path.join(path, os.pardir), os.pardir))
    raise RuntimeError("Unable to locate bam on the PATH")

# ----------

if __name__ == "__main__":
    bam_dir = find_bam_default_repository()

    optParser = OptionParser(description="BuildAMation unittests")
    # optParser.add_option("--platform", "-p", dest="platforms", action="append", default=None, help="Platforms to test")
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
    optParser.add_option("--excludevariation", "-x", dest="excludedVariations", action="append", default=None, help="Exclude a variation from the test configurations")
    optParser.add_option("--repo", "-r", dest="repos", action="append", default=[bam_dir], help="Add a package repository to test")
    optParser.add_option("--nodefaultrepo", dest="nodefaultrepo", action="store_true", default=False, help="Do not test the default repository")
    test_option_setup(optParser)
    (options, args) = optParser.parse_args()

    if options.nodefaultrepo:
        options.repos.remove(bam_dir)
        if not options.repos:
            raise RuntimeError("No package repositories to test")

    if options.verbose:
        print_message("Options are %s" % options)
        print_message("Args    are %s" % args)

    # if not options.platforms:
    #    raise RuntimeError("No platforms were specified")

    if not options.configurations:
        raise RuntimeError("No configurations were specified")

    # if not options.noInitialClean:
    #    clean_up(options)

    exit_code = 0
    for repo in options.repos:
        repoTestDir = os.path.join(repo, "tests")
        bamTestsConfigPathname = os.path.join(repoTestDir, 'bamtests.py')
        if not os.path.isfile(bamTestsConfigPathname):
            print_message("Package repository %s has no bamtests.py file" % repo)
            continue
        bamtests = imp.load_source('bamtests', bamTestsConfigPathname)
        testConfigs = bamtests.configure_repository()
        tests = find_all_packages_to_test(repoTestDir, options)
        if not options.tests:
            if options.verbose:
                print_message("All tests will run")
        else:
            if options.verbose:
                print_message("Tests to run are: %s" % options.tests)
            filteredTests = []
            for test in options.tests:
                found = False
                for package in tests:
                    if package.get_id() == test:
                        filteredTests.append(package)
                        found = True
                        break
                if not found:
                    raise RuntimeError("Unrecognized package '%s'" % test)
            tests = filteredTests

        output_buffer = StringIO.StringIO()
        for package in tests:
            try:
                config = testConfigs[package.get_id()]
            except KeyError, e:
                if options.verbose:
                    print_message("No configuration for package: '%s'" % str(e))
                continue
            exit_code += execute_tests(package, config, options, output_buffer)

        if not options.keepFiles:
            # TODO: consider keeping track of all directories created instead
            clean_up(options)

        print_message("--------------------")
        print_message("| Results summary  |")
        print_message("--------------------")
        print_message(output_buffer.getvalue())

        logsDir = os.path.join(repoTestDir, "Logs")
        if not os.path.isdir(logsDir):
            os.makedirs(logsDir)
        logFileName = os.path.join(logsDir, "tests_" + time.strftime("%d-%m-%YT%H-%M-%S") + ".log")
        logFile = open(logFileName, "w")
        logFile.write(output_buffer.getvalue())
        logFile.close()
        output_buffer.close()

    sys.exit(exit_code)
