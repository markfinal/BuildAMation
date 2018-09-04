#!/usr/bin/python

from generate_docs import build_documentation
from optparse import OptionParser
import os
import platform
import shutil
import stat
import subprocess
import sys
import tarfile
import tempfile
import zipfile


g_script_dir = os.path.dirname(os.path.realpath(__file__))
g_bam_dir = os.path.dirname(g_script_dir)


def log(msg):
    print >>sys.stdout, msg
    sys.stdout.flush()


def run_process(args):
    log('Running: %s' % ' '.join(args))
    subprocess.check_call(args)


def _run_git(arguments):
    args = []
    args.append('git')
    args.extend(arguments)
    log('Running: %s' % ' '.join(args))
    result = subprocess.check_output(args)
    return result.rstrip()


def get_branch_name():
    return _run_git(['rev-parse', '--abbrev-ref', 'HEAD'])


def get_hash():
    return _run_git(['rev-parse', '--short', 'HEAD'])


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


def delete_directory(dir):
    if os.path.isdir(dir):
        log('Deleting folder, %s' % dir)
        shutil.rmtree(dir)


def run_dotnet_publish(output_dir, configuration='Release', framework='netcoreapp2.1', force=True, standalone_platform=None, verbosity='normal'):
    delete_directory(output_dir)
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


def zip_dir(zip_path, dir):
    log('Zipping directory %s to %s' % (dir, zip_path))
    base_dir, leaf = os.path.split(dir)
    cwd = os.getcwd()
    try:
        os.chdir(base_dir)
        with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as zip_object:
            for root, dirs, files in os.walk(leaf):
                for file_path in files:
                    zip_object.write(os.path.join(root, file_path))
    finally:
        os.chdir(cwd)


def tar_dir(tar_path, dir):
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
    log('Tarring directory %s to %s' % (dir, tar_path))
    base_dir, leaf = os.path.split(dir)
    cwd = os.getcwd()
    try:
        os.chdir(base_dir)
        with tarfile.open(tar_path, "w:gz") as tar:
            tar.add(leaf, filter=windows_executable_filter)
    finally:
        os.chdir(cwd)


def main(options, root_dir):
    _,bam_version_dir = os.path.split(root_dir)

    if options.doxygen:
        generated_docs_dir = os.path.join(g_bam_dir, 'docs')
        delete_directory(generated_docs_dir)
        build_documentation(g_bam_dir, options.doxygen)
        if options.make_distribution:
            zip_dir(os.path.join(g_bam_dir, '%s-docs' % bam_version_dir) + '.zip', generated_docs_dir)
            tar_dir(os.path.join(g_bam_dir, '%s-docs' % bam_version_dir) + '.tgz', generated_docs_dir)

    run_dotnet_publish(
        root_dir,
        configuration='Release',
        framework='netcoreapp2.1',
        force=True,
        verbosity='Minimal'
    )
    copy_support_files(root_dir)
    #list_files(root_dir)
    if options.make_distribution:
        zip_dir(os.path.join(g_bam_dir, '%s-AnyCPU' % bam_version_dir) + '.zip', root_dir)
        tar_dir(os.path.join(g_bam_dir, '%s-AnyCPU' % bam_version_dir) + '.tgz', root_dir)

    if options.standalone:
        platforms = []
        platforms.append('win-x64')
        platforms.append('osx-x64')
        platforms.append('linux-x64')
        for platform in platforms:
            platform_output_dir = root_dir + '-' + platform
            run_dotnet_publish(
                platform_output_dir,
                configuration='Release',
                framework='netcoreapp2.1',
                force=True,
                standalone_platform=platform
            )
            copy_support_files(platform_output_dir)
            #list_files(platform_output_dir)


def clone_repo(checkout_dir, gittag):
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


if __name__ == '__main__':
    parser = OptionParser()
    parser.add_option('-s', '--standalone', action='store_true', dest='standalone')
    parser.add_option('-d', '--doxygen', dest='doxygen', default=None)
    parser.add_option('-t', '--tag', dest='gittag', default=None)
    parser.add_option('-x', '--distribution', action='store_true', dest='make_distribution')
    parser.add_option('-l', '--local', action='store_true', dest='local')
    (options, args) = parser.parse_args()

    if options.gittag:
        root_dir = os.path.join(tempfile.mkdtemp(), "BuildAMation-%s" % options.gittag)
        clone_repo(root_dir, options.gittag)
    elif options.local:
        root_dir = os.path.join(g_bam_dir, 'bam_pubish')
    else:
        branch = get_branch_name()
        hash = get_hash()
        root_dir = os.path.join(tempfile.mkdtemp(), "BuildAMation-%s-%s" % (hash,branch))
    try:
        main(options, root_dir)
    except Exception, e:
        log('*** Failure reason: %s' % str(e))
    finally:
        pass
    log('Done')
