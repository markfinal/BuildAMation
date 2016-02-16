#!/usr/bin/python

from distutils.spawn import find_executable
from convert_line_endings import convert_line_endings
import os
import sys

licenseText = []


def read_license_text():
    bam_path = find_executable('bam')
    if not bam_path:
        raise RuntimeError('Unable to locate bam')
    bam_dir = os.path.dirname(bam_path)
    license_header_file = os.path.join(bam_dir, 'licenseheader.txt')
    with open(license_header_file, 'rt') as licenseFile:
        original_license_text = licenseFile.readlines()
    global licenseText
    for line in original_license_text:
        licenseText.append(line.replace('\n', ''))


def write_license_text(outfile, file_path):
    ext = os.path.splitext(file_path)[1]
    if ext == '.cs':
        outfile.write('#region License\n')
        for line in licenseText:
            if not line:
                outfile.write('//\n')
            else:
                outfile.write('// %s\n' % line)
        outfile.write('#endregion // License\n')
    elif ext == '.c' or ext == '.h' or ext == '.m' or ext == '.cpp' or ext == '.mm':
        # always write C style comments, in case files are compiled on different flags/compilers
        outfile.write('/*\n')
        for line in licenseText:
            outfile.write('%s\n' % line)
        outfile.write('*/\n')
    else:
        raise RuntimeError('Unsupported file extension for appending license, %s' % file_path)


def assign_license(file_path):
    with open(file_path, mode='rt') as infile:
        lines = infile.readlines()
    first_code_line = 0
    has_region = False
    has_c_style_comment = False
    is_python = os.path.splitext(file_path)[1] == '*.py'
    shebang = ''
    for index, line in enumerate(lines):
        if not line:
            continue
        if is_python and line.startswith('#!'):
            shebang = line
            continue
        if line.startswith('#region License'):
            has_region = True
        if line.startswith('/*'):
            has_c_style_comment = True
        if has_region:
            if line.startswith('#endregion'):
                first_code_line = index + 1
                break
        elif has_c_style_comment:
            if line.strip().endswith('*/'):
                first_code_line = index + 1
                break
        else:
            if not line.startswith('//'):
                first_code_line = index
                break
    code_lines = lines[first_code_line:]
    with open(file_path, mode='wt') as outfile:
        if is_python and shebang:
            outfile.write(shebang)
        write_license_text(outfile, file_path)
        for line in code_lines:
            outfile.write(line)
    if sys.platform.startswith("win"):
        convert_line_endings(file_path)


def process_path(path, extension_list, excluded_files_list=[]):
    if os.path.isfile(path):
        assign_license(path)
    else:
        for root, dirs, files in os.walk(path):
            for file_path in files:
                full_path = os.path.join(root, file_path)
                if full_path in excluded_files_list:
                    continue
                file_ext = os.path.splitext(file_path)[1]
                if file_ext in extension_list:
                    assign_license(full_path)

if __name__ == "__main__":
    read_license_text()
    if len(sys.argv) > 1:
        extensions = sys.argv[2:]
        if not extensions:
            extensions = ['.cs']
        process_path(sys.argv[1], extensions)
    else:
        process_path('.', ['.cs'], ['./Bam.Core/LimitedConcurrencyLevelTaskScheduler.cs'])
        process_path('tests', ['.h', '.c', '.cpp', '.m', '.mm'])
