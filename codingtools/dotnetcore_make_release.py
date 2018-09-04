#!/usr/bin/python

from generate_docs import build_documentation
from optparse import OptionParser
import os
import shutil
import subprocess
import sys
import tempfile


g_script_dir = os.path.dirname(os.path.realpath(__file__))
g_bam_dir = os.path.dirname(g_script_dir)


def log(msg):
    print >>sys.stdout, msg
    sys.stdout.flush()


def run_process(args):
    log('Running: %s' % ' '.join(args))
    subprocess.check_call(args)


def run_dotnet(target, project_path, output_dir, configuration='Release', framework='netcoreapp2.1', force=True, standalone_platform=None, verbosity='normal', extra_properties=None):
    output_dir = os.path.join(output_dir, 'bin', configuration, framework)
    cur_dir = os.getcwd()
    os.chdir(g_bam_dir)
    try:
        args = []
        args.append('dotnet')
        args.append(target)
        args.append(project_path)
        args.append('-c')
        args.append(configuration)
        args.append('-f')
        args.append(framework)
        if force:
            args.append('--force')
        args.append('-o')
        args.append(output_dir)
        args.append('-v')
        args.append(verbosity)
        if standalone_platform:
            args.append('--self-contained')
            args.append('-r')
            args.append(standalone_platform)
        if extra_properties:
            args.append(extra_properties)
        run_process(args)
    finally:
        os.chdir(cur_dir)


def run_dotnet_publish(output_dir, configuration='Release', framework='netcoreapp2.1', force=True, standalone_platform=None, verbosity='normal'):
    if os.path.isdir(output_dir):
        log('Deleting folder, %s' % output_dir)
        shutil.rmtree(output_dir)
    os.makedirs(output_dir)
    project = os.path.join('Bam', 'Bam.csproj') # specifically build the Bam executable, so that the unit test dependencies don't get dragged in
    run_dotnet('clean', project, output_dir, configuration=configuration, framework=framework, force=False, standalone_platform=None, verbosity=verbosity)
    run_dotnet('publish', project, output_dir, configuration=configuration, framework=framework, force=force, standalone_platform=standalone_platform, verbosity=verbosity, extra_properties='/p:DebugType=None')


def copy_directory_to_directory(srcdir,destdir):
    log('\tCopying directory ' + srcdir)
    shutil.copytree(srcdir, destdir)


def copy_file_to_directory(srcfile,destdir):
    log('\tCopying file ' + srcfile)
    shutil.copy(srcfile, destdir)


def copy_support_files(output_dir):
    cur_dir = os.getcwd()
    os.chdir(g_bam_dir)
    log('Copying support files...')
    try:
        copy_directory_to_directory('packages', os.path.join(output_dir, 'packages'))
        copy_directory_to_directory('tests', os.path.join(output_dir, 'tests'))
        copy_file_to_directory('env.sh', output_dir)
        copy_file_to_directory('env.bat', output_dir)
        copy_file_to_directory('Changelog.txt', output_dir)
        copy_file_to_directory('License.md', output_dir)
        copy_file_to_directory('MS-PL.md', output_dir)
    finally:
        os.chdir(cur_dir)


def list_files(base_dir):
    log('Listing files in ' + base_dir)
    starting_depth = base_dir.count(os.sep)
    for root, dirs, files in os.walk(base_dir):
        depth = root.count(os.sep) - starting_depth
        log(' ' * depth + os.path.basename(root))
        for f in files:
            log(' ' * (depth + 1) + f)


def main(options, root_dir):
    if options.doxygen:
        build_documentation(root_dir, options.doxygen)

    output_dir = os.path.join(root_dir, 'bam_publish')
    run_dotnet_publish(
        output_dir,
        configuration='Release',
        framework='netcoreapp2.1',
        force=True,
        verbosity='Minimal'
    )
    copy_support_files(output_dir)
    #list_files(output_dir)

    if options.standalone:
        platforms = []
        platforms.append('win-x64')
        platforms.append('osx-x64')
        platforms.append('linux-x64')
        for platform in platforms:
            platform_output_dir = output_dir + '_' + platform
            run_dotnet_publish(
                platform_output_dir,
                configuration='Release',
                framework='netcoreapp2.1',
                force=True,
                standalone_platform=platform
            )
            copy_support_files(platform_output_dir)
            #list_files(platform_output_dir)


def clone_repo(gittag):
    checkout_dir = os.path.join(tempfile.mkdtemp(), "BuildAMation-%s" % gittag)
    os.makedirs(checkout_dir)
    args = [
        "git",
        "clone",
        "--depth",
        "1",
        "--branch",
        gittag,
        "https://github.com/markfinal/BuildAMation",
        checkout_dir
    ]
    log('Running: %s' % ' '.join(args))
    subprocess.check_call(args)
    log('Cloning complete')
    return checkout_dir


if __name__ == '__main__':
    parser = OptionParser()
    parser.add_option('-s', '--standalone', action='store_true', dest='standalone')
    parser.add_option('-d', '--doxygen', dest='doxygen', default=None)
    parser.add_option('-t', '--tag', dest='gittag', default=None)
    (options, args) = parser.parse_args()

    root_dir = g_bam_dir
    if options.gittag:
        root_dir = clone_repo(options.gittag)
    try:
        main(options, root_dir)
    except Exception, e:
        log('*** Failure reason: %s' % str(e))
    finally:
        pass
    log('Done')
