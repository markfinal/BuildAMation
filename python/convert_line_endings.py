#!/usr/bin/python

import os

def convert_line_endings(file):
  if '\r\n' in open(file, 'rb').read():
    print '%s contains DOS line endings. Converting' % file
    with open(file, 'rb') as infile:
      text = infile.read()
      text = text.replace('\r\n', '\n')
    with open(file, 'wb') as outfile:
      outfile.write(text)

def main():
  for dirpath, dirnames, filenames in os.walk('.'):
    for file in filenames:
      if os.path.splitext(file)[1] == '.cs':
        csPath = os.path.join(dirpath, file)
        convert_line_endings(csPath)

if __name__ == "__main__":
  main()
