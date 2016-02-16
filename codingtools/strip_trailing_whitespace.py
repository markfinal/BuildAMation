#!/usr/bin/python

from convert_line_endings import convert_line_endings
import os
import re
import sys


def strip_trailing_whitespace(file_path):
    with open(file_path, mode='rt') as infile:
        lines = infile.readlines()
    with open(file_path, mode='wt') as outfile:
        for line in lines:
            stripped = re.sub('[ \t]+$', '', line)
            outfile.write(stripped)
    if sys.platform.startswith("win"):
        convert_line_endings(file_path)


def process_path(path, extension_list):
    if os.path.isfile(path):
        strip_trailing_whitespace(path)
    else:
        for root, dirs, files in os.walk(path):
            # ignore hidden files and directories
            files = [f for f in files if not f[0] == '.']
            dirs[:] = [d for d in dirs if not d[0] == '.']
            for file_path in files:
                file_ext = os.path.splitext(file_path)[1]
                if file_ext in extension_list:
                    full_path = os.path.join(root, file_path)
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
