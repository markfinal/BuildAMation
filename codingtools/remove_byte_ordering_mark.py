#!/usr/bin/python

from convert_line_endings import convert_line_endings
import codecs
import os
import sys

boms = [(codecs.BOM_UTF32, 4),
        (codecs.BOM_UTF16, 2),
        (codecs.BOM_UTF8, 3)]


def remove_bom(file):
    with open(file, mode='rt') as infile:
        lines = infile.readlines()
    if len(lines) == 0:
        return
    with open(file, mode='wt') as outfile:
        first_line = lines[0]
        remaining_lines = lines[1:]
        first_line_offset = 0
        for bom, length in boms:
            if first_line.startswith(bom):
                first_line_offset = length
                print "%s has BOM" % file
                break
        outfile.write(first_line[first_line_offset:])
        for line in remaining_lines:
            outfile.write(line)
    if sys.platform.startswith("win"):
        convert_line_endings(file)


def processPath(path, extensionList):
    if os.path.isfile(path):
        remove_bom(path)
    else:
        for root, dirs, files in os.walk(path):
            for file in files:
                fileExt = os.path.splitext(file)[1]
                if fileExt in extensionList:
                    fullPath = os.path.join(root, file)
                    remove_bom(fullPath)

if __name__ == "__main__":
    if len(sys.argv) > 1:
        extensions = sys.argv[2:]
        if not extensions:
            extensions = ['.cs']
        processPath(sys.argv[1], extensions)
    else:
        processPath('.', ['.cs'])
        processPath('tests', ['.h', '.c', '.cpp', '.m', '.mm'])
