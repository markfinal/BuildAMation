#!/usr/bin/python

import glob
import os
import subprocess
import sys


def upgrade_schema(package_dir, failed_packages):
    current_dir = os.getcwd()
    try:
        print >>sys.stdout, "Updating '%s'" % package_dir
        sys.stdout.flush()
        os.chdir(package_dir)
        args = ['bam', '--showdefinition', '--forceupdates']
        process = subprocess.Popen(args)
        process.wait()
        if process.returncode:
            failed_packages.append((package_dir, process.returncode))
    finally:
        os.chdir(current_dir)


def get_package_paths_from_root(package_root):
    files = glob.glob('%s/*/*/bam' % package_root)
    dirs = [os.path.split(f)[0] for f in files if os.path.isdir(f)]
    return dirs


def process_path(package_root_list):
    failed_packages = []
    for package_root in package_root_list:
        package_paths = get_package_paths_from_root(package_root)
        for package in package_paths:
            upgrade_schema(package, failed_packages)
    if failed_packages:
        print >>sys.stdout, "Failed to upgrade some packages:"
        sys.stdout.flush()
        for package_dir, exitCode in failed_packages:
            print >>sys.stdout, package_dir
            sys.stdout.flush()

if __name__ == "__main__":
    if len(sys.argv) > 1:
        process_path(sys.argv[1:])
    else:
        process_path(['.'])
