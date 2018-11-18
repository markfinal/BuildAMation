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


def run_dotnet_test(output_dir, configuration='Release', framework='netcoreapp2.1'):
    if os.path.isdir(output_dir):
        log('Deleting folder, %s' % output_dir)
        shutil.rmtree(output_dir)
    os.makedirs(output_dir)
    os.makedirs(os.path.join(output_dir, 'packages')) # to identify as an empty repository
    output_dir = os.path.join(output_dir, 'bin', configuration, framework)
    cur_dir = os.getcwd()
    os.chdir(g_bam_dir)
    try:
        args = []
        args.append('dotnet')
        args.append('test')
        args.append(os.path.join('Bam.Core.Test', 'Bam.Core.Test.csproj'))
        args.append('-c')
        args.append(configuration)
        args.append('-f')
        args.append(framework)
        args.append('-o')
        args.append(output_dir)
        args.append('-v')
        args.append('Minimal')
        args.append('--results-directory')
        args.append(output_dir)
        args.append('--logger')
        args.append('"trx;LogFileName=TestResults.trx"')
        run_process(args)
    finally:
        os.chdir(cur_dir)


def main(options):
    output_dir = os.path.join(g_bam_dir, 'bam_unittests')
    run_dotnet_test(
        output_dir,
        configuration='Release',
        framework='netcoreapp2.1'
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
