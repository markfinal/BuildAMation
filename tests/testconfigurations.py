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

    def _get_set_of_all_response_names(self):
        platform = sys.platform
        unique_response_files = set()
        # TODO: can we do this with a lambda expression?
        if platform.startswith("win"):
            for i in self._win.values():
                if not i:
                    continue
                for j in i:
                    unique_response_files.add(j)
        elif platform.startswith("linux"):
            for i in self._linux.values():
                if not i:
                    continue
                for j in i:
                    unique_response_files.add(j)
        elif platform.startswith("darwin"):
            for i in self._osx.values():
                if not i:
                    continue
                for j in i:
                    unique_response_files.add(j)
        else:
            raise RuntimeError("Unknown platform " + platform)
        return unique_response_files

    def _get_list_of_response_names(self, builder):
        platform = sys.platform
        response_names = []
        # TODO: can we do this with a lambda expression?
        if platform.startswith("win"):
            if self._win[builder]:
                for i in self._win[builder]:
                    response_names.append(i)
            else:
                response_names.append(None)
        elif platform.startswith("linux"):
            if self._linux[builder]:
                for i in self._linux[builder]:
                    response_names.append(i)
            else:
                response_names.append(None)
        elif platform.startswith("darwin"):
            if self._osx[builder]:
                for i in self._osx[builder]:
                    response_names.append(i)
            else:
                response_names.append(None)
        else:
            raise RuntimeError("Unknown platform " + platform)
        return response_names

    def get_variations(self, builder, excluded_variations):
        variations = set()
        for i in self._get_list_of_response_names(builder):
            """
            if not i:
                responseFiles.append(i)
            else:
                if not excludedResponseFiles or i not in excludedResponseFiles:
                    responseFiles.append(i)
            """
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

    def __init__(self):
        self._platforms = []
        self._argList = []

    def get_arguments(self):
        return self._argList

    def platforms(self):
        return self._platforms

    @staticmethod
    def register_option(platform, option_tuple):
        if platform not in ConfigOptions._allOptions:
            ConfigOptions._allOptions[platform] = set()
        ConfigOptions._allOptions[platform].add(option_tuple)

    @staticmethod
    def get_options():
        return ConfigOptions._allOptions.get(platform.system(), [])


class VisualCCommon(ConfigOptions):
    def __init__(self):
        super(VisualCCommon, self).__init__()
        self._argList.append("--C.toolchain=VisualC")
        self._platforms = ["Win32", "x64"]
        ConfigOptions.register_option("Windows", ("VisualC.version", "Set the VisualC version"))
        ConfigOptions.register_option("Windows", ("WindowsSDK.version", "Set the WindowsSDK version"))


class VisualC64(VisualCCommon):
    def __init__(self):
        super(VisualC64, self).__init__()
        self._argList.append("--C.bitdepth=64")
        self._platforms.remove("Win32")


class VisualC32(VisualCCommon):
    def __init__(self):
        super(VisualC32, self).__init__()
        self._argList.append("--C.bitdepth=32")
        self._platforms.remove("x64")


class Mingw32(ConfigOptions):
    def __init__(self):
        super(Mingw32, self).__init__()
        self._argList.append("--C.bitdepth=32")
        self._argList.append("--C.toolchain=Mingw")
        ConfigOptions.register_option("Windows", ("Mingw.version", "Set the Mingw version"))


class GccCommon(ConfigOptions):
    def __init__(self):
        super(GccCommon, self).__init__()
        ConfigOptions.register_option("Linux", ("GCC.version", "Set the GCC version"))


class Gcc64(GccCommon):
    def __init__(self):
        super(Gcc64, self).__init__()
        self._argList.append("--C.bitdepth=64")


class Gcc32(GccCommon):
    def __init__(self):
        super(Gcc32, self).__init__()
        self._argList.append("--C.bitdepth=32")


class ClangCommon(ConfigOptions):
    def __init__(self):
        super(ClangCommon, self).__init__()
        self._argList.append("--Xcode.generateSchemes")  # TODO: this is only for the Xcode build mode
        ConfigOptions.register_option("Darwin", ("Clang.version", "Set the Clang version"))


class Clang64(ClangCommon):
    def __init__(self):
        super(Clang64, self).__init__()
        self._argList.append("--C.bitdepth=64")


visualc = VisualCCommon()
visualc64 = VisualC64()
visualc32 = VisualC32()
mingw32 = Mingw32()
gcc = GccCommon()
gcc32 = Gcc32()
gcc64 = Gcc64()
clang = ClangCommon()
clang64 = Clang64()


# TODO: change the list of response files to a dictionary, with the key as the response file
# (which also serves as part of a Bam command option) and the value is a list of supported versions,
# e.g. {"visual":["8.0","9.0","10.0"]}
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
