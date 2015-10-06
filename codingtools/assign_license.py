#!/usr/bin/python

from distutils.spawn import find_executable
from convert_line_endings import convert_line_endings
import os
import sys

licenseText = []

def readLicenseText():
  bam_path = find_executable('bam')
  if not bam_path:
      raise RuntimeError('Unable to locate bam')
  bam_dir = os.path.dirname(bam_path)
  licenseHeaderFile = os.path.join(bam_dir, 'licenseheader.txt')
  with open(licenseHeaderFile, 'rt') as licenseFile:
      original_license_text = licenseFile.readlines()
  global licenseText
  for line in original_license_text:
    licenseText.append(line.replace('\n', ''))

def write_license_text(outfile, file):
    ext = os.path.splitext(file)[1]
    if ext == '.cs':
        outfile.write('#region License\n')
        for line in licenseText:
            if not line:
                outfile.write('//\n')
            else:
                outfile.write('// %s\n' % line)
        outfile.write('#endregion // License\n')
    elif ext == '.cpp' or ext == '.mm':
        for line in licenseText:
            if not line:
                outfile.write('//\n')
            else:
                outfile.write('// %s\n' % line)
    elif ext == '.c' or ext == '.h' or ext == '.m':
        outfile.write('/*\n')
        for line in licenseText:
            outfile.write('%s\n' % line)
        outfile.write('*/\n')
    else:
        raise RuntimeError('Unsupported file extension for appending license, %s' % file)

def assign_license(file):
    with open(file, mode='rt') as infile:
        lines = infile.readlines()
    first_code_line = 0
    has_region = False
    has_c_style_comment = False
    is_python = os.path.splitext(file)[1] == '*.py'
    shebang = ''
    for index,line in enumerate(lines):
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
    with open(file, mode='wt') as outfile:
        if is_python and shebang:
            outfile.write(shebang)
        write_license_text(outfile, file)
        for line in code_lines:
            outfile.write(line)
    if sys.platform.startswith("win"):
        convert_line_endings(file)

def processPath(path, extensionList):
    if os.path.isfile(path):
        assign_license(path)
    else:
      for root, dirs, files in os.walk(path):
          for dir in dirs:
              processPath(os.path.join(root, dir), extensionList)
          for file in files:
              fileExt = os.path.splitext(file)[1]
              if fileExt in extensionList:
                  fullPath = os.path.join(root, file)
                  assign_license(fullPath)

if __name__ == "__main__":
    readLicenseText()
    if len(sys.argv) > 1:
        extensions = sys.argv[2:]
        if not extensions:
            extensions = ['.cs']
        processPath(sys.argv[1], extensions)
    else:
        processPath('.', ['.cs'])
        processPath('tests', ['.h', '.c', '.cpp', '.m', '.mm'])
