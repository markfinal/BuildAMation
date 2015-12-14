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


def process_path(path, extension_list):
    if os.path.isfile(path):
        strip_trailing_whitespace(path)
    else:
        for root, dirs, files in os.walk(path):
            for file in files:
                file_ext = os.path.splitext(file)[1]
                if file_ext in extension_list:
                    full_path = os.path.join(root, file)
                    strip_trailing_whitespace(full_path)

if __name__ == "__main__":
    if len(sys.argv) > 1:
        extensions = sys.argv[2:]
        if not extensions:
            extensions = ['.cs']
        process_path(sys.argv[1], extensions)
    else:
        process_path('.', ['.cs'])
        process_path('tests', ['.h', '.c', '.cpp', '.m', '.mm'])
