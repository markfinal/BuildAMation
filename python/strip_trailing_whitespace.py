#!/usr/bin/python

from convert_line_endings import convert_line_endings
import os
import re
import sys

def strip_trailing_whitespace(file):
  with open(file, mode='rt') as infile:
    lines = infile.readlines()
  with open(file, mode='wt') as outfile:
    for line in lines:
      stripped = re.sub('[ \t]+$', '', line)
      outfile.write(stripped)
  if sys.platform.startswith("win"):
    convert_line_endings(file)

def main():
  if len(sys.argv) > 1:
    strip_trailing_whitespace(sys.argv[1])
    return
  for dirpath, dirnames, filenames in os.walk('.'):
    for file in filenames:
      if os.path.splitext(file)[1] == '.cs':
        csPath = os.path.join(dirpath, file)
        strip_trailing_whitespace(csPath)

if __name__ == "__main__":
  main()
