#!/usr/bin/python

import argparse
import os
import platform
import subprocess
import sys

system = platform.system()
if system == 'Windows':
    bam_shell = 'bam.bat'
else:
    bam_shell = 'bam'


def _log(message):
    print >>sys.stdout, message
    sys.stdout.flush()


def get_bam_installdir():
    try:
        return subprocess.check_output([bam_shell, '--installdir']).rstrip()
    except WindowsError:
        raise RuntimeError('Unable to locate BAM on the PATH')


def get_bam_rootdir():
    install_dir = get_bam_installdir()
    root_dir = os.path.realpath(os.path.join(install_dir, os.pardir, os.pardir, os.pardir))
    return root_dir


def get_bam_testsdir():
    root_dir = get_bam_rootdir()
    tests_dir = os.path.join(root_dir, 'tests')
    return tests_dir


def set_dependent_package_default_version(package_dir, name, version):
    current_dir = os.getcwd()
    try:
        _log("Updating '%s'" % package_dir)
        os.chdir(package_dir)
        args = [bam_shell, '--setdependentdefaultversion', '--pkgname=%s' % name, '--pkgversion=%s' % version]
        process = subprocess.Popen(args)
        process.wait()
    finally:
        os.chdir(current_dir)


def process_path(tests_dir, name, version):
    for test_name in os.walk(tests_dir).next()[1]:
        test_package = os.path.join(tests_dir, test_name)
        set_dependent_package_default_version(test_package, name, version)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='Make a version of a dependent package the default')
    parser.add_argument('--name', required=True, help="Name of package to change default version of")
    parser.add_argument('--version', required=True, help="Version of the package to make default")
    parser.add_argument('--repository', default=None, required=False, help="Optional path to the repository to affect. Default is the BAM default repository.")
    args = parser.parse_args()
    if not args.name:
        parser.error('Package name is required')
    if args.repository:
        tests_dir = os.path.abspath(args.repository)
        if not os.path.isdir(tests_dir):
            parser.error('Repository does not exist')
        tests_dir = os.path.join(tests_dir, "tests")
        if not os.path.isdir(tests_dir):
            parser.error("No tests subdirectory of this repository, %s" % tests_dir)
    else:
        tests_dir = get_bam_testsdir()
    process_path(tests_dir, args.name, args.version)
