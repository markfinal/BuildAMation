#!/usr/bin/python
import os
import sys

class TestSetup:
    _win = {}
    _linux = {}
    _osx = {}

    def __init__(self, win={}, linux={}, osx={}):
        self._win = win
        self._linux = linux
        self._osx = osx

    def GetBuildModes(self):
        platform = sys.platform
        if platform.startswith("win"):
            return self._win.keys()
        elif platform.startswith("linux"):
            return self._linux.keys()
        elif platform.startswith("darwin"):
            return self._osx.keys()
        else:
            raise RuntimeError("Unknown platform " + platform)

    def _GetSetOfAllResponseNames(self):
        platform = sys.platform
        uniqueResponseFiles = set()
        # TODO: can we do this with a lambda expression?
        if platform.startswith("win"):
            for i in self._win.values():
                if not i:
                    continue
                for j in i:
                    uniqueResponseFiles.add(j)
        elif platform.startswith("linux"):
            for i in self._linux.values():
                if not i:
                    continue
                for j in i:
                    uniqueResponseFiles.add(j)
        elif platform.startswith("darwin"):
            for i in self._osx.values():
                if not i:
                    continue
                for j in i:
                    uniqueResponseFiles.add(j)
        else:
            raise RuntimeError("Unknown platform " + platform)
        return uniqueResponseFiles

    def _GetListOfResponseNames(self, builder):
        platform = sys.platform
        responseNames = []
        # TODO: can we do this with a lambda expression?
        if platform.startswith("win"):
            if self._win[builder]:
                for i in self._win[builder]:
                    responseNames.append(i)
            else:
                responseNames.append(None);
        elif platform.startswith("linux"):
            if self._linux[builder]:
                for i in self._linux[builder]:
                    responseNames.append(i)
            else:
                responseNames.append(None);
        elif platform.startswith("darwin"):
            if self._osx[builder]:
                for i in self._osx[builder]:
                    responseNames.append(i)
            else:
                responseNames.append(None);
        else:
            raise RuntimeError("Unknown platform " + platform)
        return responseNames

    def GetVariations(self, builder, excludedVariations):
        variations = set()
        for i in self._GetListOfResponseNames(builder):
            """
            if not i:
                responseFiles.append(i)
            else:
                if not excludedResponseFiles or i not in excludedResponseFiles:
                    responseFiles.append(i)
            """
            variations.add(i)
        return variations

def GetResponsePath(responseName):
  return "%s.rsp" % responseName

def GetTestConfig(packageName, options):
    try:
        config = configs[packageName]
    except KeyError, e:
        if options.verbose:
            print "No configuration for package: '%s'" % str(e)
        return None
    return config

def TestOptionSetup(optParser):
    allResponseNames = set()
    for config in configs.values():
        results = config._GetSetOfAllResponseNames()
        allResponseNames.update(results)
    allOptions = set()
    for response in allResponseNames:
        for opt in response.GetOptions():
            allOptions.add(opt)
    for opt,help in allOptions:
        optName = "--%s" % opt
        variable = opt.replace('.', '_')
        optParser.add_option(optName, dest=variable, action="append", default=None, help=help)


class ConfigOptions(object):
    def __init__(self):
        self._argList = []
        self._options = []

    def GetArguments(self):
        return self._argList

    def GetOptions(self):
        return self._options


class VisualCCommon(ConfigOptions):
    def __init__(self):
        super(VisualCCommon, self).__init__()
        self._argList.append("--C.toolchain=VisualC")
        self._options.append(("VisualC.version", "Set the VisualC version"))
        self._options.append(("WindowsSDK.version", "Set the WindowsSDK version"))


class VisualC64(VisualCCommon):
    def __init__(self):
        super(VisualC64, self).__init__()
        self._argList.append("--C.bitdepth=64")


class VisualC32(VisualCCommon):
    def __init__(self):
        super(VisualC32, self).__init__()
        self._argList.append("--C.bitdepth=32")


class Mingw32(ConfigOptions):
    def __init__(self):
        super(Mingw32, self).__init__()
        self._argList.append("--C.bitdepth=32")
        self._argList.append("--C.toolchain=Mingw")
        self._options.append(("Mingw.version", "Set the Mingw version"))


class GccCommon(ConfigOptions):
    def __init__(self):
        super(GccCommon, self).__init__()
        self._options.append(("GCC.version", "Set the GCC version"))


class Gcc64(GccCommon):
    def __init__(self):
        super(Gcc64, self).__init__()
        self._argList.append("--C.bitdepth=64")


class Gcc32(GccCommon):
    def __init__(self):
        super(Gcc32, self).__init__()
        self._argList.append("--C.bitdepth=32")


class Clang64(ConfigOptions):
    def __init__(self):
        super(Clang64, self).__init__()
        self._argList.append("--Xcode.generateSchemes"); # TODO: this is only for the Xcode build mode
        self._options.append(("Clang.version", "Set the Clang version"))


visualc64 = VisualC64()
visualc32 = VisualC32()
mingw32 = Mingw32()
gcc32 = Gcc32()
gcc64 = Gcc64()
clang64 = Clang64()


# TODO: change the list of response files to a dictionary, with the key as the response file (which also serves as part of a Bam command option) and the value is a list of supported versions, e.g. {"visual":["8.0","9.0","10.0"]}
configs = {}
configs["Test"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                            linux={"Native":[gcc64],"MakeFile":[gcc64]},
                            osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
configs["Test2"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                             linux={"Native":[gcc64],"MakeFile":[gcc64]},
                             osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
configs["Test3"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                             linux={"Native":[gcc64],"MakeFile":[gcc64]},
                             osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
configs["Test4"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                             linux={"Native":[gcc64],"MakeFile":[gcc64]},
                             osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
configs["Test5"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                             linux={"Native":[gcc64],"MakeFile":[gcc64]},
                             osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
configs["Test6"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                             linux={"Native":[gcc64],"MakeFile":[gcc64]},
                             osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
configs["Test7"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                             linux={"Native":[gcc64],"MakeFile":[gcc64]},
                             osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
configs["Test8"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]})
configs["Test9"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                             linux={"Native":[gcc64],"MakeFile":[gcc64]},
                             osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
configs["Test10"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                              linux={"Native":[gcc64],"MakeFile":[gcc64]},
                              osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
configs["Test11"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                              linux={"Native":[gcc64],"MakeFile":[gcc64]},
                              osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
configs["Test12"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                              linux={"Native":[gcc64],"MakeFile":[gcc64]},
                              osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
""" Moved to bam-qt
configs["Test13"] = TestSetup(win={"Native":[visualc64],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]})
"""
configs["Test14"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                              linux={"Native":[gcc64],"MakeFile":[gcc64]},
                              osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
configs["Test15"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                              linux={"Native":[gcc64],"MakeFile":[gcc64]},
                              osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
configs["Test16"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                              linux={"Native":[gcc64],"MakeFile":[gcc64]},
                              osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
configs["Test17"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                              linux={"Native":[gcc64],"MakeFile":[gcc64]},
                              osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
configs["CodeGenTest"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                                   linux={"Native":[gcc64],"MakeFile":[gcc64]},
                                   osx={"Native":[clang64],"MakeFile":[clang64]})
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
""" Moved to bam-graphics
configs["Direct3DTriangle"] = TestSetup(win={"Native":[visualc64],"VSSolution":[visualc64],"MakeFile":[visualc64]})
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
configs["OpenGLUniformBufferTest"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]})
configs["RenderTextureAndProcessor"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]})
"""
""" Moved to bam-csharp
configs["WPFTest"] = TestSetup(win={"VSSolution":None})
"""
configs["CocoaTest1"] = TestSetup(osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
configs["ObjectiveCTest1"] = TestSetup(osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
configs["ProxyTest"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                                 linux={"Native":[gcc64],"MakeFile":[gcc64]},
                                 osx={"Native":[clang64],"MakeFile":[clang64]})
configs["HeaderLibraryTest"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                                         linux={"Native":[gcc64],"MakeFile":[gcc64]},
                                         osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
configs["Cxx11Test1"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                                  linux={"Native":[gcc64],"MakeFile":[gcc64]},
                                  osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
configs["PluginTest"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                                  linux={"Native":[gcc64],"MakeFile":[gcc64]},
                                  osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
""" Moved to bam-qt
configs["Qt5Test1"] = TestSetup(win={"Native":[visualc64],"VSSolution":[visualc64],"MakeFile":[visualc64]},
                                linux={"Native":[gcc64],"MakeFile":[gcc64]},
                                osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
"""
""" Moved to bam-zeromq
configs["zeromqtest"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                                  linux={"Native":[gcc64],"MakeFile":[gcc64]},
                                  osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
"""
