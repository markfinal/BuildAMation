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
    "packages/VisualC-10.0",
    "packages/WindowsSDK-6.0A",
    "packages/WindowsSDK-7.1",
    "packages/XmlUtilities"
]
# TODO remove .git


def clone_buildamation(directory_path, options):
    branch = options.branch
    if not branch:
        # this is a tag
        branch = "v%s" % options.version
    args = [
        "git",
        "clone",
        "--depth",
        "1",
        "--branch",
        branch,
        "https://github.com/markfinal/BuildAMation",
        directory_path
    ]
    print "Running: %s" % ' '.join(args)
    subprocess.check_call(args)
    print >>sys.stdout, "Cloning complete"
    sys.stdout.flush()


def clean_clone():
    for file_path in filesToDelete:
        print >>sys.stdout, "Deleting file_path %s" % file_path
        sys.stdout.flush()
        os.remove(file_path)
    for directory in dirsToDelete:
        print >>sys.stdout, "Deleting directory %s" % directory
        sys.stdout.flush()
        shutil.rmtree(directory)


def update_version_numbers(options):
    common_assembly_info_path = os.path.join(os.getcwd(), "Common", "CommonAssemblyInfo.cs")
    for line in fileinput.input(common_assembly_info_path, inplace=1):  # , backup='.bk'):
        line = re.sub('AssemblyInformationalVersion\("[0-9.]+"\)',
                      'AssemblyInformationalVersion("%s")' % options.version,
                      line.rstrip())
        print line


def build():
    print >>sys.stdout, "Starting build in %s" % os.getcwd()
    sys.stdout.flush()
    if platform.system() == "Windows":
        # assume Visual Studio 2013
        if os.environ.has_key("ProgramFiles(x86)"):
            buildtool = r"C:\Program Files (x86)\MSBuild\12.0\bin\MSBuild.exe"
        else:
            buildtool = r"C:\Program Files\MSBuild\12.0\bin\MSBuild.exe"
        if not os.path.isfile(buildtool):
            raise RuntimeError("Unable to locate msbuild at '%s'" % buildtool)
    elif platform.system() == "Darwin" or platform.system() == "Linux":
        buildtool = "xbuild"
    else:
        raise RuntimeError("Unrecognized platform, %s" % platform.system())
    subprocess.check_call([buildtool, "/property:Configuration=Release", "/nologo", "BuildAMation.sln"])
    print >>sys.stdout, "Finished build"
    sys.stdout.flush()


def build_documentation(options):
    args = [options.doxygenpath, "docsrc/BuildAMationDoxy"]
    print "Running: %s" % ' '.join(args)
    subprocess.check_call(args)


def make_tar_distribution(options):
    cwd = os.getcwd()
    try:
        checkout_dir, bam_dir = os.path.split(cwd)
        tar_path = os.path.join(checkout_dir, "BuildAMation-%s.tgz" % options.version)
        print >>sys.stdout, "Writing tar file_path %s" % tar_path
        sys.stdout.flush()
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
            tar.add(os.path.join(bam_dir, "env.bat"))
            tar.add(os.path.join(bam_dir, "env.sh"))
            tar.add(os.path.join(bam_dir, "MS-PL.md"))
            tar.add(os.path.join(bam_dir, "License.md"))
            tar.add(os.path.join(bam_dir, "packages"))
            tar.add(os.path.join(bam_dir, "tests"), filter=windows_executable_filter)
        print >>sys.stdout, "Finished writing tar file_path %s" % tar_path
        sys.stdout.flush()
    finally:
        os.chdir(cwd)


def make_zip_distribution(options):
    cwd = os.getcwd()
    try:
        checkout_dir, bam_dir = os.path.split(cwd)
        zip_path = os.path.join(checkout_dir, "BuildAMation-%s.zip" % options.version)
        print >>sys.stdout, "Writing zip file_path %s" % zip_path
        sys.stdout.flush()
        os.chdir(checkout_dir)

        def recursive_write(zip_object, dir_to_add):
            for root, dirs, files in os.walk(dir_to_add):
                for file_path in files:
                    zip_object.write(os.path.join(root, file_path))
        with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as zip_object:
            if os.path.isdir(os.path.join(bam_dir, "bin")):
                recursive_write(zip_object, os.path.join(bam_dir, "bin"))
            zip_object.write(os.path.join(bam_dir, "Changelog.txt"))
            zip_object.write(os.path.join(bam_dir, "env.bat"))
            zip_object.write(os.path.join(bam_dir, "env.sh"))
            zip_object.write(os.path.join(bam_dir, "License.md"))
            zip_object.write(os.path.join(bam_dir, "MS-PL.md"))
            recursive_write(zip_object, os.path.join(bam_dir, "packages"))
            recursive_write(zip_object, os.path.join(bam_dir, "tests"))
        print >>sys.stdout, "Finished writing zip file_path %s" % zip_path
        sys.stdout.flush()
    finally:
        os.chdir(cwd)


def make_tar_docs_distribution(options):
    cwd = os.getcwd()
    try:
        checkout_dir, bam_dir = os.path.split(cwd)
        tar_path = os.path.join(checkout_dir, "BuildAMation-%s-docs.tgz" % options.version)
        print >>sys.stdout, "Writing tar file_path %s" % tar_path
        sys.stdout.flush()
        os.chdir(checkout_dir)
        with tarfile.open(tar_path, "w:gz") as tar:
            tar.add(os.path.join(bam_dir, "docs"))
        print >>sys.stdout, "Finished writing tar file_path %s" % tar_path
        sys.stdout.flush()
    finally:
        os.chdir(cwd)


def make_zip_docs_distribution(options):
    cwd = os.getcwd()
    try:
        checkout_dir, bam_dir = os.path.split(cwd)
        zip_path = os.path.join(checkout_dir, "BuildAMation-%s-docs.zip" % options.version)
        print >>sys.stdout, "Writing zip file_path %s" % zip_path
        sys.stdout.flush()
        os.chdir(checkout_dir)

        def recursive_write(zip_object, dir_to_add):
            for root, dirs, files in os.walk(dir_to_add):
                for file_path in files:
                    zip_object.write(os.path.join(root, file_path))
        with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as zip_object:
            recursive_write(zip_object, os.path.join(bam_dir, "docs"))
        print >>sys.stdout, "Finished writing zip file_path %s" % zip_path
        sys.stdout.flush()
    finally:
        os.chdir(cwd)


def main(directory_path, options):
    print >>sys.stdout, "Creating BuildAMation version %s" % options.version
    sys.stdout.flush()
    clone_buildamation(directory_path, options)
    cwd = os.getcwd()
    try:
        os.chdir(directory_path)
        clean_clone()
        update_version_numbers(options)
        build()
        build_documentation(options)
        make_tar_distribution(options)
        make_zip_distribution(options)
        make_tar_docs_distribution(options)
        make_zip_docs_distribution(options)
    finally:
        os.chdir(cwd)


if __name__ == "__main__":
    parser = OptionParser()
    parser.add_option("-v", "--version", dest="version", help="Version to create")
    parser.add_option("-b", "--branch", dest="branch", default=None, help="Override for branch to clone")
    parser.add_option("-d", "--doxygen", dest="doxygenpath", default=None, help="Path to the doxygen executable")
    (options, args) = parser.parse_args()
    if not options.version:
        parser.error("Must supply a version")
    if not options.doxygenpath:
        parser.error("Must supply the path to the doxygen executable")
        
    tempDir = tempfile.mkdtemp()
    cloningDir = os.path.join(tempDir, "BuildAMation-%s" % options.version)
    os.makedirs(cloningDir)
    try:
        main(cloningDir, options)
    except Exception, e:
        print >>sys.stdout, "*** Failure reason: %s" % str(e)
        sys.stdout.flush()
    finally:
        print >>sys.stdout, "Deleting clone"
        sys.stdout.flush()
        # shutil.rmtree(cloningDir)
    print >>sys.stdout, "Done"
    sys.stdout.flush()
