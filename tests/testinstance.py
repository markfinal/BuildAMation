#!/usr/bin/python

class TestInstance:
    """
    A TestInstance is a runnable instance of the test, encompasing all the details it needs
    in order to generate a command line.
    This makes instances unique, and can be gathered up in advance, and easily filtered.
    """

    def __init__(self, package, flavour, variation, package_options):
        self._package = package
        self._flavour = flavour
        self._variation = variation
        self._package_options = package_options

    def package_name(self):
        return self._package.get_id()

    def package_description(self):
        return self._package.get_description()

    def package_path(self):
        return self._package.get_path()

    def flavour(self):
        return self._flavour

    def variation_arguments(self):
        args = self._package_options if self._package_options else []
        args.extend(self._variation.get_arguments())
        return args

    def runnable(self):
        return self._variation is not None

    def platforms(self):
        return self._variation.platforms()

    def __repr__(self):
        return "%s-%s-%s" % (self._package.get_name(), self._flavour, str(self._variation))
