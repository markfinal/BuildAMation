#!/usr/bin/python

import os
import sys

def convert_line_endings(file):
    if '\r\n' in open(file, 'rb').read():
        print '%s contains DOS line endings. Converting' % file
        with open(file, 'rb') as infile:
            text = infile.read()
            text = text.replace('\r\n', '\n')
        with open(file, 'wb') as outfile:
            outfile.write(text)

def processPath(path, extensionList):
    if os.path.isfile(path):
        convert_line_endings(path)
    else:
      for root, dirs, files in os.walk(path):
          for dir in dirs:
              processPath(os.path.join(root, dir), extensionList)
          for file in files:
              fileExt = os.path.splitext(file)[1]
              if fileExt in extensionList:
                  fullPath = os.path.join(root, file)
                  convert_line_endings(fullPath)

if __name__ == "__main__":
    if len(sys.argv) > 1:
        try:
            extensions = [sys.argv[2:]]
        except IndexError:
            extensions = ['.cs']
        processPath(sys.argv[1], extensions)
    else:
        processPath('.', ['.cs'])
        processPath('tests', ['.h', '.c', '.cpp', '.m', '.mm', '.py', '.sh', '.bat'])
