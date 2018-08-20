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
    if not os.path.isdir(output_dir):
        os.makedirs(output_dir)
    else:
        log('Deleting folder, %s' % output_dir)
        shutil.rmtree(output_dir)
    cur_dir = os.getcwd()
    os.chdir(g_bam_dir)
    try:
        args = []
        args.append('dotnet')
        args.append('publish')
        args.append('Bam/Bam.csproj') # specifically build this, so that the unit test dependencies don't get dragged in
        args.append('-c')
        args.append(configuration)
        args.append('-f')
        args.append(framework)
        if force:
            args.append('--force')
        args.append('-o')
        args.append(output_dir)
        args.append('-v')
        args.append('minimal')
        if standalone_platform:
            args.append('--self-contained')
            args.append('-r')
            args.append(standalone_platform)
        run_process(args)
    finally:
        os.chdir(cur_dir)


def main(options):
    output_dir = os.path.join(g_bam_dir, 'bam_publish')
    run_dotnet_publish(
        output_dir,
        configuration='Release',
        framework='netcoreapp2.1',
        force=True
    )

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


if __name__ == '__main__':
    parser = OptionParser()
    (options, args) = parser.parse_args()

    try:
        main(options)
    except Exception, e:
        log('*** Failure reason: %s' % str(e))
    finally:
        pass
    log('Done')
