#!/usr/bin/python
import platform
import sys


class TestSetup:
    _win = {}
    _linux = {}
    _osx = {}

    def __init__(self, win=None, linux=None, osx=None):
        self._win = {} if win is None else win
        self._linux = {} if linux is None else linux
        self._osx = {} if osx is None else osx

    def get_build_modes(self):
        platform = sys.platform
        if platform.startswith("win"):
            return self._win.keys()
        elif platform.startswith("linux"):
            return self._linux.keys()
        elif platform.startswith("darwin"):
            return self._osx.keys()
        else:
            raise RuntimeError("Unknown platform " + platform)

    def _get_list_of_test_configurations(self, builder):
        platform = sys.platform
        configurations = []
        if platform.startswith("win"):
            if self._win[builder]:
                configurations.extend(self._win[builder])
        elif platform.startswith("linux"):
            if self._linux[builder]:
                configurations.extend(self._linux[builder])
        elif platform.startswith("darwin"):
            if self._osx[builder]:
                configurations.extend(self._osx[builder])
        else:
            raise RuntimeError("Unknown platform " + platform)
        return configurations

    def get_variations(self, builder, excluded_variations, bitdepth):
        variations = set()
        for i in self._get_list_of_test_configurations(builder):
            if not excluded_variations or i.get_name() not in excluded_variations:
                if bitdepth == "*" or bitdepth == i.arch():
                    variations.add(i)
        return variations


def test_option_setup(opt_parser):
    def store_option(option, opt_str, value, parser):
        if not parser.values.Flavours:
            parser.values.Flavours = []
        parser.values.Flavours.append("%s=%s" % (opt_str, value))
    for opt, help_text in ConfigOptions.get_options():
        option_name = "--%s" % opt
        opt_parser.add_option(option_name,
                              dest="Flavours",
                              type="string",
                              action="callback",
                              callback=store_option,
                              default=None,
                              help=help_text)


class ConfigOptions(object):
    _allOptions = {}

    def __init__(self, name, arch):
        self._name = name
        self._arch = arch
        self._platforms = []
        self._argList = []
        if ("32" == arch or "64" == arch):
            self._argList.append("--C.bitdepth=%s"%arch)

    def get_name(self):
        return self._name

    def get_arguments(self):
        return self._argList

    def platforms(self):
        return self._platforms

    def arch(self):
        return self._arch

    def __repr__(self):
        return self._name

    @staticmethod
    def register_option(platform, option_tuple):
        if platform not in ConfigOptions._allOptions:
            ConfigOptions._allOptions[platform] = set()
        ConfigOptions._allOptions[platform].add(option_tuple)

    @staticmethod
    def get_options():
        return ConfigOptions._allOptions.get(platform.system(), [])


class VisualCCommon(ConfigOptions):
    def __init__(self, arch):
        super(VisualCCommon, self).__init__("VisualC", arch)
        self._argList.append("--C.toolchain=VisualC")
        self._platforms = []
        if arch == "32":
            self._platforms.append("Win32")
        elif arch == "64":
            self._platforms.append("x64")
        elif arch == "*":
            self._platforms.extend(["Win32","x64"])
        else:
            raise RuntimeError("Unrecognized arch: %s" % arch)
        ConfigOptions.register_option("Windows", ("VisualC.version", "Set the VisualC version"))
        ConfigOptions.register_option("Windows", ("WindowsSDK.version", "Set the WindowsSDK version"))


class VisualC64(VisualCCommon):
    def __init__(self):
        super(VisualC64, self).__init__("64")


class VisualC32(VisualCCommon):
    def __init__(self):
        super(VisualC32, self).__init__("32")


class Mingw32(ConfigOptions):
    def __init__(self):
        super(Mingw32, self).__init__("Mingw", "32")
        self._argList.append("--C.toolchain=Mingw")
        ConfigOptions.register_option("Windows", ("Mingw.version", "Set the Mingw version"))


class GccCommon(ConfigOptions):
    def __init__(self, arch):
        super(GccCommon, self).__init__("Gcc", arch)
        ConfigOptions.register_option("Linux", ("Gcc.version", "Set the Gcc version"))


class Gcc64(GccCommon):
    def __init__(self):
        super(Gcc64, self).__init__("64")


class Gcc32(GccCommon):
    def __init__(self):
        super(Gcc32, self).__init__("32")


class ClangCommon(ConfigOptions):
    def __init__(self, arch):
        super(ClangCommon, self).__init__("Clang", arch)
        ConfigOptions.register_option("Darwin", ("Clang.version", "Set the Clang version"))


class Clang32(ClangCommon):
    def __init__(self):
        super(Clang32, self).__init__("32")


class Clang64(ClangCommon):
    def __init__(self):
        super(Clang64, self).__init__("64")


visualc = VisualCCommon("*")
visualc64 = VisualC64()
visualc32 = VisualC32()
mingw32 = Mingw32()
gcc = GccCommon("*")
gcc32 = Gcc32()
gcc64 = Gcc64()
clang = ClangCommon("*")
clang32 = Clang32()
clang64 = Clang64()


""" Moved to bam-csharp
configs["CodeGenTest2"] = TestSetup(win={"Native":[visualc64,mingw32],"MakeFile":[visualc64,mingw32]},
                                    linux={"Native":[gcc64],"MakeFile":[gcc64]},
                                    osx={"Native":[clang64],"MakeFile":[clang64]})
"""
""" Moved to bam-csharp
configs["CSharpTest1"] = TestSetup(win={"Native":None,"MakeFile":None,"VSSolution":None},
                                   linux={"Native":None,"MakeFile":None},
                                   osx={"Native":None,"MakeFile":None})
"""
"""
configs["MixedModeCpp"] = TestSetup(win={"Native":[visualc64],"VSSolution":[visualc64],"MakeFile":[visualc64]})
"""
""" Moved to bam-csharp
configs["MixedTest"] = TestSetup(win={"Native":[visualc64],"MakeFile":[visualc64]})
"""
""" Moved to bam-hpc
configs["OpenCLTest1"] = TestSetup(win={"Native":[visualc64],"VSSolution":[visualc64],"MakeFile":[visualc64]})
"""
""" Moved to bam-graphicssdk
"""
""" Moved to bam-csharp
configs["WPFTest"] = TestSetup(win={"VSSolution":None})
"""
