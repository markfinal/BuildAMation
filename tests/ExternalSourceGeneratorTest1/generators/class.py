#!/usr/bin/python

import argparse
import xml.etree.ElementTree as ET
import os
import sys


def generate_class(name, headerpath, sourcepath):
    with open(headerpath, 'wt') as header:
        guard = os.path.splitext(os.path.basename(headerpath))[0].upper()
        print >>header, '#ifndef %s_H' % guard
        print >>header, '#define %s_H' % guard
        print >>header, 'class %s' % name
        print >>header, '{'
        print >>header, '  public:'
        print >>header, '    int exit_code() const;'
        print >>header, '};'
        print >>header, '#endif /* %s_H */"' % guard
    with open(sourcepath, 'wt') as source:
        print >>source, '#include "%s"' % os.path.basename(headerpath)
        print >>source, 'int %s::exit_code() const' % name
        print >>source, '{'
        print >>source, '  return -1;'
        print >>source, '}'


def parse_spec_and_generate(args):
    tree = ET.parse(args.specfile)
    root = tree.getroot()
    if root.tag != 'spec':
        raise RuntimeError('Expected a root element of "spec"')
    for child in root:
        if child.tag != 'class':
            raise RuntimeError('Expected a child of "spec" to be "class"')
        name = child.attrib['name']
        generate_class(name, args.output_header, args.output_source)


def main():
    try:
        parser = argparse.ArgumentParser(description='C++ class generator')
        parser.add_argument('--specfile', required=True, help='Input specification file for the class')
        parser.add_argument('--output-header', required=True, help='Path to the output header')
        parser.add_argument('--output-source', required=True, help='Path to the output source')
        args = parser.parse_args()
        parse_spec_and_generate(args)
    except Exception as e:
        import traceback
        print "ERROR: %s" % str(e)
        traceback.print_exc()
        sys.exit(-1)


if __name__ == '__main__':
    main()
