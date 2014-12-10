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

def processPath(dirPath, extensionList):
    for dirpath, dirnames, filenames in os.walk(dirPath):
        for file in filenames:
            fileExt = os.path.splitext(file)[1]
            if fileExt in extensionList:
                csPath = os.path.join(dirpath, file)
                strip_trailing_whitespace(csPath)

if __name__ == "__main__":
    if len(sys.argv) > 1:
        strip_trailing_whitespace(sys.argv[1])
    else:
        processPath('.', ['.cs'])
        processPath('testpackages', ['.h', '.c', '.cpp', '.m', '.mm'])
