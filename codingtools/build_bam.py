#!/usr/bin/python

import os
import platform
import subprocess
import sys

def build_bam(build_dir, coveritypath=None, rebuild=False):
    current_dir = os.getcwd()
    try:
        os.chdir(build_dir)
        print >>sys.stdout, "Starting build in %s" % build_dir
        sys.stdout.flush()
        build_args = []
        if coveritypath:
            os.environ["PATH"] += os.pathsep + coveritypath
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
        if rebuild or coveritypath:
            build_args.append("/t:Rebuild")
        print >>sys.stdout, "Running command: %s" % ' '.join(build_args)
        sys.stdout.flush()
        subprocess.check_call(build_args)
        print >>sys.stdout, "Finished build"
        sys.stdout.flush()
    finally:
        os.chdir(current_dir)


if __name__ == "__main__":
    try:
        build_bam(os.getcwd())
    except Exception, e:
        print >>sys.stdout, "*** Build failure reason: %s" % str(e)
        sys.stdout.flush()
