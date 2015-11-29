#!/usr/bin/python

import fileinput
import fnmatch
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

filesToDelete=[\
".gitignore"
]

dirsToDelete=[\
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
"packages/VisualC-11.0",
"packages/WindowsSDK-6.0A",
"packages/WindowsSDK-7.1",
"packages/XmlUtilities"
]
# TODO remove .git


def CloneBuildAMation(dir, options):
    # TODO: specify the explicit tag too
    branch = options.branch
    if not branch:
        branch = "v%s" % options.version
    args = ["git", "clone", "--depth", "1", "--branch", branch, "https://github.com/markfinal/BuildAMation", dir]
    print "Running: %s" % ' '.join(args)
    subprocess.check_call(args)
    print >>sys.stdout, "Cloning complete"
    sys.stdout.flush()


def CleanClone():
    for file in filesToDelete:
        print >>sys.stdout, "Deleting file %s" % file
        sys.stdout.flush()
        os.remove(file)
    for directory in dirsToDelete:
        print >>sys.stdout, "Deleting directory %s" % directory
        sys.stdout.flush()
        shutil.rmtree(directory)


def UpdateVersionNumbers(options):
    commonAssemblyInfoPath = os.path.join(os.getcwd(), "Common", "CommonAssemblyInfo.cs")
    for line in fileinput.input(commonAssemblyInfoPath, inplace=1):#, backup='.bk'):
        line = re.sub('AssemblyInformationalVersion\("[0-9.]+"\)', 'AssemblyInformationalVersion("%s")'%options.version, line.rstrip())
        print line


def Build():
    print >>sys.stdout, "Starting build in %s" % os.getcwd()
    sys.stdout.flush()
    if platform.system() == "Windows":
        # assume Visual Studio 2013
        buildtool = r"C:\Program Files (x86)\MSBuild\12.0\bin\MSBuild.exe"
        if not os.path.isfile(buildtool):
           raise RuntimeError("Unable to locate msbuild at '%s'" % buildtool)
    elif platform.system() == "Darwin" or platform.system() == "Linux":
        buildtool = "xbuild"
    else:
        raise RuntimeError("Unrecognized platform, %s" % platform.system())
    subprocess.check_call([buildtool, "/property:Configuration=Release", "/nologo", "BuildAMation.sln"])
    print >>sys.stdout, "Finished build"
    sys.stdout.flush()


def BuildDocumentation(options):
    args = [options.doxygenpath, "docsrc/BuildAMationDoxy"]
    print "Running: %s" % ' '.join(args)
    subprocess.check_call(args)


def MakeTarDistribution(options):
    cwd = os.getcwd()
    try:
        coDir, bamDir = os.path.split(cwd)
        tarPath = os.path.join(coDir, "BuildAMation-%s.tgz"%options.version)
        print >>sys.stdout, "Writing tar file %s" % tarPath
        sys.stdout.flush()
        os.chdir(coDir)
        def filter(tarinfo):
            if platform.system() != "Windows":
                return tarinfo
            # attempt to fix up the permissions that are lost during tarring on Windows
            if tarinfo.name.endswith(".exe") or tarinfo.name.endswith(".dll") or tarinfo.name.endswith(".py") or tarinfo.name.endswith(".sh") or tarinfo.name.endswith("bam"):
                tarinfo.mode = stat.S_IRUSR | stat.S_IXUSR | stat.S_IRGRP | stat.S_IXGRP | stat.S_IROTH | stat.S_IXOTH
            return tarinfo
        with tarfile.open(tarPath, "w:gz") as tar:
            if os.path.isdir(os.path.join(bamDir, "bin")):
                tar.add(os.path.join(bamDir, "bin"), filter=filter)
            tar.add(os.path.join(bamDir, "Changelog.txt"))
            tar.add(os.path.join(bamDir, "env.bat"))
            tar.add(os.path.join(bamDir, "env.sh"))
            tar.add(os.path.join(bamDir, "License.md"))
            tar.add(os.path.join(bamDir, "packages"))
            tar.add(os.path.join(bamDir, "tests"), filter=filter)
        print >>sys.stdout, "Finished writing tar file %s" % tarPath
        sys.stdout.flush()
    finally:
        os.chdir(cwd)


def MakeZipDistribution(options):
    cwd = os.getcwd()
    try:
        coDir, bamDir = os.path.split(cwd)
        zipPath = os.path.join(coDir, "BuildAMation-%s.zip"%options.version)
        print >>sys.stdout, "Writing zip file %s" % zipPath
        sys.stdout.flush()
        os.chdir(coDir)
        def RecursiveWrite(zip, dirToAdd):
            for root, dirs, files in os.walk(dirToAdd):
                for file in files:
                    zip.write(os.path.join(root, file))
        with zipfile.ZipFile(zipPath, "w", zipfile.ZIP_DEFLATED) as zip:
            if os.path.isdir(os.path.join(bamDir, "bin")):
              RecursiveWrite(zip, os.path.join(bamDir, "bin"))
            zip.write(os.path.join(bamDir, "Changelog.txt"))
            zip.write(os.path.join(bamDir, "env.bat"))
            zip.write(os.path.join(bamDir, "env.sh"))
            zip.write(os.path.join(bamDir, "License.md"))
            RecursiveWrite(zip, os.path.join(bamDir, "packages"))
            RecursiveWrite(zip, os.path.join(bamDir, "tests"))
        print >>sys.stdout, "Finished writing zip file %s" % zipPath
        sys.stdout.flush()
    finally:
        os.chdir(cwd)


def MakeTarDocsDistribution(options):
    cwd = os.getcwd()
    try:
        coDir, bamDir = os.path.split(cwd)
        tarPath = os.path.join(coDir, "BuildAMation-%s-docs.tgz"%options.version)
        print >>sys.stdout, "Writing tar file %s" % tarPath
        sys.stdout.flush()
        os.chdir(coDir)
        def filter(tarinfo):
            if platform.system() != "Windows":
                return tarinfo
            # attempt to fix up the permissions that are lost during tarring on Windows
            if tarinfo.name.endswith(".exe") or tarinfo.name.endswith(".dll") or tarinfo.name.endswith(".py") or tarinfo.name.endswith(".sh") or tarinfo.name.endswith("bam"):
                tarinfo.mode = stat.S_IRUSR | stat.S_IXUSR | stat.S_IRGRP | stat.S_IXGRP | stat.S_IROTH | stat.S_IXOTH
            return tarinfo
        with tarfile.open(tarPath, "w:gz") as tar:
            tar.add(os.path.join(bamDir, "docs"))
        print >>sys.stdout, "Finished writing tar file %s" % tarPath
        sys.stdout.flush()
    finally:
        os.chdir(cwd)


def MakeZipDocsDistribution(options):
    cwd = os.getcwd()
    try:
        coDir, bamDir = os.path.split(cwd)
        zipPath = os.path.join(coDir, "BuildAMation-%s-docs.zip"%options.version)
        print >>sys.stdout, "Writing zip file %s" % zipPath
        sys.stdout.flush()
        os.chdir(coDir)
        def RecursiveWrite(zip, dirToAdd):
            for root, dirs, files in os.walk(dirToAdd):
                for file in files:
                    zip.write(os.path.join(root, file))
        with zipfile.ZipFile(zipPath, "w", zipfile.ZIP_DEFLATED) as zip:
            RecursiveWrite(zip, os.path.join(bamDir, "docs"))
        print >>sys.stdout, "Finished writing zip file %s" % zipPath
        sys.stdout.flush()
    finally:
        os.chdir(cwd)


def Main(dir, options):
    print >>sys.stdout, "Creating BuildAMation version %s" % options.version
    sys.stdout.flush()
    CloneBuildAMation(dir, options)
    cwd = os.getcwd()
    try:
        os.chdir(dir)
        CleanClone()
        UpdateVersionNumbers(options)
        Build()
        BuildDocumentation(options)
        MakeTarDistribution(options)
        MakeZipDistribution(options)
        MakeTarDocsDistribution(options)
        MakeZipDocsDistribution(options)
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
    #cloningDir = r"c:\users\mark\appdata\local\temp\tmpg4tul0"
    try:
        Main(cloningDir, options)
    except Exception, e:
        print >>sys.stdout, "*** Failure reason: %s" % str(e)
        sys.stdout.flush()
    finally:
        print >>sys.stdout, "Deleting clone"
        sys.stdout.flush()
        #shutil.rmtree(cloningDir)
    print >>sys.stdout, "Done"
    sys.stdout.flush()
