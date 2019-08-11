#!/usr/bin/python

import argparse
import os
import StringIO
import subprocess
import sys
import tempfile
import time


class NoDoxygenError(Exception):
    pass


class DoxygenFailedError(Exception):
    def __init__(self, errcode):
        self._errcode = errcode

    def __str__(self):
        return "Doxygen failed, with error code %d" % self._errcode


class IncompleteDocumentationError(Exception):
    def __init__(self, count):
        self._count = count

    def __str__(self):
        return "Incomplete documentation, with %d errors" % self._count


def _execute_and_capture(arg_list, working_dir, output_messages, error_messages):
    return_code = 0
    out_fd, out_path = tempfile.mkstemp()
    err_fd, err_path = tempfile.mkstemp()
    try:
        with os.fdopen(out_fd, 'w') as out:
            with os.fdopen(err_fd, 'w') as err:
                sys.stdout.write("Running: %s\n" % (' '.join(arg_list)))
                sys.stdout.flush()
                p = subprocess.Popen(arg_list, stdout=out_fd, stderr=err_fd, cwd=working_dir)
                while p.poll() is None:
                    sys.stdout.write('+') # keep something alive on the console
                    sys.stdout.flush()
                    time.sleep(1)
                p.wait()
                sys.stdout.write('\n')
                sys.stdout.flush()
                return_code = p.returncode
    except Exception:
        error_messages.write("Failed to run '%s' in %s" % (' '.join(arg_list), working_dir))
        raise
    finally:
        with open(out_path) as out:
            output_messages.write(out.read())
        with open(err_path) as err:
            error_messages.write(err.read())
        os.remove(out_path)
        os.remove(err_path)
    return return_code


def build_documentation(source_dir, doxygenpath, update_config):
    if not doxygenpath:
        raise RuntimeError("Path to doxygen is required")
    args = [doxygenpath]
    if update_config:
        args.append('-u')
    args.append(os.path.join(source_dir, 'docsrc', 'BuildAMationDoxy'))
    try:
        output_messages = StringIO.StringIO()
        error_messages = StringIO.StringIO()
        return_code = _execute_and_capture(args, source_dir, output_messages, error_messages)

        sys.stdout.write("Doxygen output:\n")
        sys.stdout.write(output_messages.getvalue())
        sys.stdout.flush()

        errors = error_messages.getvalue()
        if errors:
            sys.stdout.write("Doxygen warnings/errors:\n")
            sys.stdout.write(errors)
            sys.stdout.flush()

        if 0 != return_code:
            raise DoxygenFailedError(return_code)

        if errors:
            raise IncompleteDocumentationError(len(errors.split('\n')))
    except OSError:
        raise NoDoxygenError('Unable to run doxygen executable "%s"' % doxygenpath)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='Generate documentation through doxygen')
    parser.add_argument("-d", "--doxygen", dest="doxygenpath", default=None, help="Path to the doxygen executable.")
    parser.add_argument("-u", "--update", dest="doxyupdate", default=False, action="store_true", required=False, help="Update the doxygen config file")
    args = parser.parse_args()

    try:
        build_documentation(os.getcwd(), args.doxygenpath, args.doxyupdate)
    except Exception, e:
        print >>sys.stdout, "*** Failure reason: %s" % str(e)
        sys.stdout.flush()
        sys.exit(1)
    else:
        print >>sys.stdout, "Done"
        sys.stdout.flush()
        sys.exit(0)
