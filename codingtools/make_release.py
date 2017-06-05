#!/usr/bin/python

import fileinput
from optparse import OptionParser
import os
import platform
import re
import shutil
import stat
import subprocess
import sys
import tarfile
import tempfile
import zipfile

filesToDelete = [
    ".gitignore"
]

# these are folders that will never appear in a distribution
# either because they have no place there
# or they are much older packages that have not been upgraded
dirsToDelete = [
    "codingtools",
    "packages/Clang-3.1",
    "packages/Clang-3.3",
    "packages/ComposerXE-12",
    "packages/ComposerXECommon",
    "packages/Gcc-4.0",
    "packages/Gcc-4.1",
    "packages/Gcc-4.4",
    "packages/Gcc-4.5",
    "packages/Gcc-4.6",
    "packages/Mingw-3.4.5",
    "packages/Mingw-4.5.0",
    "packages/QMakeBuilder",
    "packages/VisualC-8.0",
    "packages/VisualC-9.0",
    "packages/WindowsSDK-6.0A",
    "packages/WindowsSDK-7.1",
    "packages/XmlUtilities"
]


def clone_buildamation(path_to_clone_at, options):
    args = [
        "git",
        "clone",
        "--depth",
        "1",
        "--branch",
        options.tag,
        "https://github.com/markfinal/BuildAMation",
        path_to_clone_at
    ]
    print "Running: %s" % ' '.join(args)
    subprocess.check_call(args)
    print >>sys.stdout, "Cloning complete"
    sys.stdout.flush()


def remove_unnecessary_files_from_clone():
    for file_path in filesToDelete:
        print >>sys.stdout, "Deleting file_path %s" % file_path
        sys.stdout.flush()
        os.remove(file_path)
    for directory in dirsToDelete:
        print >>sys.stdout, "Deleting directory %s" % directory
        sys.stdout.flush()
        shutil.rmtree(directory)


def update_version_numbers_in_files(options):
    # fileinput redirects sys.stdout, so be sure that any issues result in fileinput close() being called
    # can't use 'with' as "FileInput instance has no attribute '__exit__'"
    common_assembly_info_path = os.path.join(os.getcwd(), "Common", "CommonAssemblyInfo.cs")
    try:
        for line in fileinput.input(common_assembly_info_path, inplace=1):  # , backup='.bk'):
            line = re.sub('AssemblyInformationalVersion\("[0-9.]+"\)',
                          'AssemblyInformationalVersion("%s")' % options.version,
                          line.rstrip())
            print line
    finally:
        fileinput.close()
    doxyconfig_path = os.path.join(os.getcwd(), "docsrc", "BuildAMationDoxy")
    try:
        for line in fileinput.input(doxyconfig_path, inplace=1):  # , backup='.bk'):
            if line.startswith('PROJECT_NUMBER'):
                print "PROJECT_NUMBER = %s" % options.version,
            else:
                print line,
    finally:
        fileinput.close()


def build_software(options):
    print >>sys.stdout, "Starting build in %s" % os.getcwd()
    sys.stdout.flush()
    build_args = []
    if options.coveritypath:
        os.environ["PATH"] += os.pathsep + options.coveritypath
        build_args.extend(["cov-build", "--dir", "cov-int"])
        if os.path.isdir('cov-int'):
            shutil.rmtree('cov-int')
    if platform.system() == "Windows":
        # assume Visual Studio 2013
        if os.environ.has_key("ProgramFiles(x86)"):
            buildtool = r"C:\Program Files (x86)\MSBuild\12.0\bin\MSBuild.exe"
        else:
            buildtool = r"C:\Program Files\MSBuild\12.0\bin\MSBuild.exe"
        if not os.path.isfile(buildtool):
            raise RuntimeError("Unable to locate msbuild at '%s'" % buildtool)
        build_args.append(buildtool)
    elif platform.system() == "Darwin" or platform.system() == "Linux":
        # xbuild is now redundant
        build_args.append("msbuild")
    else:
        raise RuntimeError("Unrecognized platform, %s" % platform.system())
    build_args.extend(["/property:Configuration=Release", "/nologo", "BuildAMation.sln"])
    if options.coveritypath:
        build_args.append("/t:Rebuild")
    print >>sys.stdout, "Running command: %s" % ' '.join(build_args)
    sys.stdout.flush()
    subprocess.check_call(build_args)
    print >>sys.stdout, "Finished build"
    sys.stdout.flush()


def build_documentation(options):
    if not options.doxygenpath:
        return
    args = [options.doxygenpath, "docsrc/BuildAMationDoxy"]
    print "Running: %s" % ' '.join(args)
    subprocess.check_call(args)


def make_coverity_distribution(options):
    if not options.coveritypath:
        return
    cwd = os.getcwd()
    try:
        checkout_dir, bam_dir = os.path.split(cwd)
        tar_path = os.path.join(checkout_dir, "BuildAMation.tgz")

        with tarfile.open(tar_path, "w:gz") as tar:
            tar.add("cov-int")
        print >>sys.stdout, "-> Coverity scan: %s" % tar_path
        sys.stdout.flush()
    finally:
        os.chdir(cwd)
        shutil.rmtree('cov-int')


def make_tar_distribution(options):
    cwd = os.getcwd()
    try:
        checkout_dir, bam_dir = os.path.split(cwd)
        tar_path = os.path.join(checkout_dir, "BuildAMation-%s-AnyCPU.tgz" % options.version)
        os.chdir(checkout_dir)

        def windows_executable_filter(tarinfo):
            if platform.system() != "Windows":
                return tarinfo
            # attempt to fix up the permissions that are lost during tarring on Windows
            if tarinfo.name.endswith(".exe") or\
               tarinfo.name.endswith(".dll") or\
               tarinfo.name.endswith(".py") or\
               tarinfo.name.endswith(".sh") or\
               tarinfo.name.endswith("bam"):
                tarinfo.mode = stat.S_IRUSR | stat.S_IXUSR | stat.S_IRGRP | stat.S_IXGRP | stat.S_IROTH | stat.S_IXOTH
            return tarinfo
        with tarfile.open(tar_path, "w:gz") as tar:
            if os.path.isdir(os.path.join(bam_dir, "bin")):
                tar.add(os.path.join(bam_dir, "bin"), filter=windows_executable_filter)
            tar.add(os.path.join(bam_dir, "Changelog.txt"))
            tar.add(os.path.join(bam_dir, "CONTRIBUTING.md"))
            tar.add(os.path.join(bam_dir, "env.bat"))
            tar.add(os.path.join(bam_dir, "env.sh"))
            tar.add(os.path.join(bam_dir, "MS-PL.md"))
            tar.add(os.path.join(bam_dir, "License.md"))
            tar.add(os.path.join(bam_dir, "packages"))
            tar.add(os.path.join(bam_dir, "tests"), filter=windows_executable_filter)
        print >>sys.stdout, "-> Tar distribution: %s" % tar_path
        sys.stdout.flush()
    finally:
        os.chdir(cwd)


def make_zip_distribution(options):
    cwd = os.getcwd()
    try:
        checkout_dir, bam_dir = os.path.split(cwd)
        zip_path = os.path.join(checkout_dir, "BuildAMation-%s-AnyCPU.zip" % options.version)
        os.chdir(checkout_dir)

        def recursive_write(zip_object, dir_to_add):
            for root, dirs, files in os.walk(dir_to_add):
                for file_path in files:
                    zip_object.write(os.path.join(root, file_path))
        with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as zip_object:
            if os.path.isdir(os.path.join(bam_dir, "bin")):
                recursive_write(zip_object, os.path.join(bam_dir, "bin"))
            zip_object.write(os.path.join(bam_dir, "Changelog.txt"))
            zip_object.write(os.path.join(bam_dir, "CONTRIBUTING.md"))
            zip_object.write(os.path.join(bam_dir, "env.bat"))
            zip_object.write(os.path.join(bam_dir, "env.sh"))
            zip_object.write(os.path.join(bam_dir, "License.md"))
            zip_object.write(os.path.join(bam_dir, "MS-PL.md"))
            recursive_write(zip_object, os.path.join(bam_dir, "packages"))
            recursive_write(zip_object, os.path.join(bam_dir, "tests"))
        print >>sys.stdout, "-> Zip distribution: %s" % zip_path
        sys.stdout.flush()
    finally:
        os.chdir(cwd)


def make_tar_docs_distribution(options):
    if not options.doxygenpath:
        return
    cwd = os.getcwd()
    try:
        checkout_dir, bam_dir = os.path.split(cwd)
        tar_path = os.path.join(checkout_dir, "BuildAMation-%s-apidocs.tgz" % options.version)
        os.chdir(checkout_dir)
        with tarfile.open(tar_path, "w:gz") as tar:
            tar.add(os.path.join(bam_dir, "docs"))
        print >>sys.stdout, "-> Tar API documentation: %s" % tar_path
        sys.stdout.flush()
    finally:
        os.chdir(cwd)


def make_zip_docs_distribution(options):
    if not options.doxygenpath:
        return
    cwd = os.getcwd()
    try:
        checkout_dir, bam_dir = os.path.split(cwd)
        zip_path = os.path.join(checkout_dir, "BuildAMation-%s-apidocs.zip" % options.version)
        os.chdir(checkout_dir)

        def recursive_write(zip_object, dir_to_add):
            for root, dirs, files in os.walk(dir_to_add):
                for file_path in files:
                    zip_object.write(os.path.join(root, file_path))
        with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as zip_object:
            recursive_write(zip_object, os.path.join(bam_dir, "docs"))
        print >>sys.stdout, "-> Zip API documentation %s" % zip_path
        sys.stdout.flush()
    finally:
        os.chdir(cwd)


def main(options):
    print >>sys.stdout, "Creating BuildAMation version %s" % options.version
    sys.stdout.flush()
    cwd = os.getcwd()
    if options.tag:
        cloningDir = os.path.join(tempfile.mkdtemp(), "BuildAMation-%s" % options.version)
        os.makedirs(cloningDir)
        clone_buildamation(cloningDir, options)
        os.chdir(cloningDir)
        remove_unnecessary_files_from_clone()
        update_version_numbers_in_files(options)
    try:
        build_software(options)
        build_documentation(options)
        make_coverity_distribution(options)
        make_tar_distribution(options)
        make_zip_distribution(options)
        make_tar_docs_distribution(options)
        make_zip_docs_distribution(options)
    finally:
        os.chdir(cwd)


if __name__ == "__main__":
    parser = OptionParser()
    parser.add_option("-t", "--tag", dest="tag", default=None, help="Git tag/branch to clone and build. Default is to use the existing clone, but version numbers are not modified")
    parser.add_option("-v", "--version", dest="version", default=None, help="Override version to build. Required if the existing clone is used")
    parser.add_option("-d", "--doxygen", dest="doxygenpath", default=None, help="Path to the doxygen executable. If not supplied, the documentation is not generated")
    parser.add_option("-c", "--coverity", dest="coveritypath", default=None, help="Path to the Coverity bin directory to use Coverity during a build")
    (options, args) = parser.parse_args()
    if not options.tag and not options.version:
        parser.error("Building the current clone requires the version to be specified")
    if not options.version:
        options.version = options.tag[1:] # tags and branches use the format 'vmajor.minor.patch[phase]'

    try:
        main(options)
    except Exception, e:
        print >>sys.stdout, "*** Failure reason: %s" % str(e)
        sys.stdout.flush()
    finally:
        pass
        # shutil.rmtree(cloningDir)
    print >>sys.stdout, "Done"
    sys.stdout.flush()
