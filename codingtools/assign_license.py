#!/usr/bin/python

from convert_line_endings import convert_line_endings
import os
import re
import sys

licenseText = [\
'Copyright 2010-2014 Mark Final',
'',
'This file is part of BuildAMation.',
'',
'BuildAMation is free software: you can redistribute it and/or modify',
'it under the terms of the GNU Lesser General Public License as published by',
'the Free Software Foundation, either version 3 of the License, or',
'(at your option) any later version.',
'',
'BuildAMation is distributed in the hope that it will be useful,',
'but WITHOUT ANY WARRANTY; without even the implied warranty of',
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the',
'GNU Lesser General Public License for more details.',
'',
'You should have received a copy of the GNU Lesser General Public License',
'along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.',
]

def write_license_text(outfile, file):
  ext = os.path.splitext(file)[1]
  if ext == '.cs':
    outfile.write('#region License\n')
    for line in licenseText:
      if not line:
        outfile.write('//\n')
      else:
        outfile.write('// %s\n' % line)
    outfile.write('#endregion\n')
  elif ext == '.c' or ext == '.h' or ext == '.cpp' or ext == '.m' or ext == '.mm':
    for line in licenseText:
      if not line:
        outfile.write('//\n')
      else:
        outfile.write('// %s\n' % line)
  else:
    raise RuntimeError('Unsupported file extension for appending license, %s' % file)

def assign_license(file):
  with open(file, mode='rt') as infile:
    lines = infile.readlines()
  first_code_line = 0
  has_region = False
  is_python = os.path.splitext(file)[1] == '*.py'
  shebang = ''
  for line in lines:
    if not line:
      first_code_line += 1
      continue
    if is_python and line.startswith('#!'):
      shebang = line
      first_code_line += 1
      continue
    if line.startswith('#region License'):
      has_region = True
    if has_region:
      if line.startswith('#endregion'):
        first_code_line += 1
        break
    else:
      if not line.startswith('//'):
        break
    if has_region and line.startswith('#endregion'):
      break
    first_code_line += 1
  code_lines = lines[first_code_line:]
  with open(file, mode='wt') as outfile:
    if is_python and shebang:
      outfile.write(shebang)
    write_license_text(outfile, file)
    for line in code_lines:
      outfile.write(line)
  if sys.platform.startswith("win"):
    convert_line_endings(file)

def processPath(dirPath, ext):
  for dirpath, dirnames, filenames in os.walk(dirPath):
    for file in filenames:
      if os.path.splitext(file)[1] == ext:
        csPath = os.path.join(dirpath, file)
        assign_license(csPath)

if __name__ == "__main__":
    if len(sys.argv) > 1:
        assign_license(sys.argv[1])
    else:
        processPath('.', '.cs')
        processPath('testpackages', '.h')
        processPath('testpackages', '.c')
        processPath('testpackages', '.cpp')
        processPath('testpackages', '.m')
        processPath('testpackages', '.mm')
