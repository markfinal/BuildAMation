#!/usr/bin/python

from optparse import OptionParser
import os
import shutil
import subprocess
import sys


g_script_dir = os.path.dirname(os.path.realpath(__file__))
g_bam_dir = os.path.dirname(g_script_dir)


def log(msg):
    print >>sys.stdout, msg
    sys.stdout.flush()


def run_process(args):
    log('Running: %s' % ' '.join(args))
    subprocess.check_call(args)


def run_dotnet_publish(output_dir, configuration='Release', framework='netcoreapp2.1', force=True, standalone_platform=None):
    if os.path.isdir(output_dir):
        log('Deleting folder, %s' % output_dir)
        shutil.rmtree(output_dir)
    os.makedirs(output_dir)
    output_dir = os.path.join(output_dir, 'bin', configuration, framework)
    cur_dir = os.getcwd()
    os.chdir(g_bam_dir)
    try:
        args = []
        args.append('dotnet')
        args.append('publish')
        args.append(os.path.join('Bam', 'Bam.csproj')) # specifically build this, so that the unit test dependencies don't get dragged in
        args.append('-c')
        args.append(configuration)
        args.append('-f')
        args.append(framework)
        if force:
            args.append('--force')
        args.append('-o')
        args.append(output_dir)
        args.append('-v')
        #args.append('minimal')
        args.append('normal')
        if standalone_platform:
            args.append('--self-contained')
            args.append('-r')
            args.append(standalone_platform)
        run_process(args)
    finally:
        os.chdir(cur_dir)


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


def main(options):
    output_dir = os.path.join(g_bam_dir, 'bam_publish')
    run_dotnet_publish(
        output_dir,
        configuration='Release',
        framework='netcoreapp2.1',
        force=True
    )
    copy_support_files(output_dir)
    list_files(output_dir)

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
            list_files(platform_output_dir)


if __name__ == '__main__':
    parser = OptionParser()
    parser.add_option('-s', '--standalone', action='store_true', dest='standalone')
    (options, args) = parser.parse_args()

    try:
        main(options)
    except Exception, e:
        log('*** Failure reason: %s' % str(e))
    finally:
        pass
    log('Done')
