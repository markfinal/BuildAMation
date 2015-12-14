#!/usr/bin/python

from convert_line_endings import convert_line_endings
import codecs
import os
import sys

boms = [(codecs.BOM_UTF32, 4),
        (codecs.BOM_UTF16, 2),
        (codecs.BOM_UTF8, 3)]


def remove_bom(file_path):
    with open(file_path, mode='rt') as infile:
        lines = infile.readlines()
    if len(lines) == 0:
        return
    with open(file_path, mode='wt') as outfile:
        first_line = lines[0]
        remaining_lines = lines[1:]
        first_line_offset = 0
        for bom, length in boms:
            if first_line.startswith(bom):
                first_line_offset = length
                print "%s has BOM" % file_path
                break
        outfile.write(first_line[first_line_offset:])
        for line in remaining_lines:
            outfile.write(line)
    if sys.platform.startswith("win"):
        convert_line_endings(file_path)


def process_path(path, extension_list):
    if os.path.isfile(path):
        remove_bom(path)
    else:
        for root, dirs, files in os.walk(path):
            for file_path in files:
                file_ext = os.path.splitext(file_path)[1]
                if file_ext in extension_list:
                    full_path = os.path.join(root, file_path)
                    remove_bom(full_path)

if __name__ == "__main__":
    if len(sys.argv) > 1:
        extensions = sys.argv[2:]
        if not extensions:
            extensions = ['.cs']
        process_path(sys.argv[1], extensions)
    else:
        process_path('.', ['.cs'])
        process_path('tests', ['.h', '.c', '.cpp', '.m', '.mm'])
