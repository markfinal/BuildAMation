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

    def GetBuilders(self):
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

    def GetResponseNames(self, builder, excludedResponseFiles):
        responseFiles = []
        for i in self._GetListOfResponseNames(builder):
            if not i:
                responseFiles.append(i)
            else:
                if not excludedResponseFiles or i not in excludedResponseFiles:
                    responseFiles.append(i)
        return responseFiles

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
    for response in allResponseNames:
      longName = "--%s.version" % response
      dest = "%s_version" % response
      # TODO: what sort of data is this? is it a single value, or can multiple versions be specified?
      optParser.add_option(longName, dest=dest, action="append", default=None, help="Versions to test for '%s'" % response)

# TODO: change the list of response files to a dictionary, with the key as the response file (which also serves as part of a Bam command option) and the value is a list of supported versions, e.g. {"visual":["8.0","9.0","10.0"]}
configs = {}
configs["Test-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"],"QMake":["visualc"]},
                                linux={"Native":["gcc"],"MakeFile":["gcc"],"QMake":["gcc"]},
                                osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"],"QMake":["clang"],"Xcode":["llvm-gcc", "clang"]})
configs["Test2-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"],"QMake":["visualc"]},
                                 linux={"Native":["gcc"],"MakeFile":["gcc"],"QMake":["gcc"]},
                                 osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"],"QMake":["clang"],"Xcode":["llvm-gcc", "clang"]})
configs["Test3-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"],"QMake":["visualc"]},
                                 linux={"Native":["gcc"],"MakeFile":["gcc"],"QMake":["gcc"]},
                                 osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"],"QMake":["clang"],"Xcode":["llvm-gcc", "clang"]})
configs["Test4-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"],"QMake":["visualc"]},
                                 linux={"Native":["gcc"],"MakeFile":["gcc"],"QMake":["gcc"]},
                                 osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"],"QMake":["clang"],"Xcode":["llvm-gcc", "clang"]})
configs["Test5-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"],"QMake":["visualc"]},
                                 linux={"Native":["gcc"],"MakeFile":["gcc"],"QMake":["gcc"]},
                                 osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"],"QMake":["clang"],"Xcode":["llvm-gcc", "clang"]})
configs["Test6-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"],"QMake":["visualc"]},
                                 linux={"Native":["gcc"],"MakeFile":["gcc"],"QMake":["gcc"]},
                                 osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"],"QMake":["clang"],"Xcode":["llvm-gcc", "clang"]})
configs["Test7-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"],"QMake":["visualc"]},
                                 linux={"Native":["gcc"],"MakeFile":["gcc"],"QMake":["gcc"]},
                                 osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"],"QMake":["clang"],"Xcode":["llvm-gcc", "clang"]})
configs["Test8-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"],"QMake":["visualc"]},
                                 linux={"Native":["gcc"],"MakeFile":["gcc"],"QMake":["gcc"]},
                                 osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"],"QMake":["clang"],"Xcode":["llvm-gcc", "clang"]})
configs["Test9-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"],"QMake":["visualc"]},
                                 linux={"Native":["gcc"],"MakeFile":["gcc"],"QMake":["gcc"]},
                                 osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"],"QMake":["clang"],"Xcode":["llvm-gcc", "clang"]})
configs["Test10-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"],"QMake":["visualc"]},
                                  linux={"Native":["gcc"],"MakeFile":["gcc"],"QMake":["gcc"]},
                                  osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"],"QMake":["clang"],"Xcode":["llvm-gcc", "clang"]})
configs["Test11-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"],"QMake":["visualc"]},
                                  linux={"Native":["gcc"],"MakeFile":["gcc"],"QMake":["gcc"]},
                                  osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"],"QMake":["clang"],"Xcode":["llvm-gcc", "clang"]})
configs["Test12-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"],"QMake":["visualc"]},
                                  linux={"Native":["gcc"],"MakeFile":["gcc"],"QMake":["gcc"]},
                                  osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"],"QMake":["clang"],"Xcode":["llvm-gcc", "clang"]})
configs["Test13-dev"] = TestSetup(win={"Native":["visualc"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"],"QMake":["visualc"]},
                                  linux={"Native":["gcc"],"MakeFile":["gcc"],"QMake":["gcc"]},
                                  osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"],"QMake":["clang"],"Xcode":["clang"]})
configs["Test14-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"],"QMake":["visualc"]},
                                  linux={"Native":["gcc"],"MakeFile":["gcc"],"QMake":["gcc"]},
                                  osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"],"QMake":["clang"],"Xcode":["llvm-gcc", "clang"]})
configs["Test15-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"],"QMake":["visualc"]},
                                  linux={"Native":["gcc"],"MakeFile":["gcc"],"QMake":["gcc"]},
                                  osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"],"QMake":["clang"],"Xcode":["llvm-gcc", "clang"]})
configs["Test16-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"],"QMake":["visualc"]},
                                  linux={"Native":["gcc"],"MakeFile":["gcc"],"QMake":["gcc"]},
                                  osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"],"QMake":["clang"],"Xcode":["llvm-gcc", "clang"]})
configs["Test17-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"],"QMake":["visualc"]},
                                  linux={"Native":["gcc"],"MakeFile":["gcc"],"QMake":["gcc"]},
                                  osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"],"QMake":["clang"],"Xcode":["llvm-gcc", "clang"]})
configs["CodeGenTest-dev"] = TestSetup(win={"Native":["visualc","mingw"],"MakeFile":["visualc","mingw"]},
                                       linux={"Native":["gcc"],"MakeFile":["gcc"]},
                                       osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"]})
configs["CodeGenTest2-dev"] = TestSetup(win={"Native":["visualc","mingw"],"MakeFile":["visualc","mingw"]},
                                        linux={"Native":["gcc"],"MakeFile":["gcc"]},
                                        osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"]})
configs["CSharpTest1-dev"] = TestSetup(win={"Native":None,"MakeFile":None,"VSSolution":None},
                                       linux={"Native":None,"MakeFile":None},
                                       osx={"Native":None,"MakeFile":None})
configs["Direct3DTriangle-dev"] = TestSetup(win={"Native":["visualc"],"VSSolution":["visualc"],"MakeFile":["visualc"]})
configs["MixedModeCpp-dev"] = TestSetup(win={"Native":["visualc"],"VSSolution":["visualc"],"MakeFile":["visualc"]})
configs["MixedTest-dev"] = TestSetup(win={"Native":["visualc"],"MakeFile":["visualc"]})
configs["OpenCLTest1-dev"] = TestSetup(win={"Native":["visualc"],"VSSolution":["visualc"],"MakeFile":["visualc"]})
configs["OpenGLUniformBufferTest-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"]})
configs["RenderTextureAndProcessor-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"]})
configs["WPFTest-dev"] = TestSetup(win={"VSSolution":None})
configs["CocoaTest1-dev"] = TestSetup(osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"],"Xcode":["llvm-gcc", "clang"]})
configs["ObjectiveCTest1-dev"] = TestSetup(osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"],"Xcode":["llvm-gcc", "clang"]})
configs["ProxyTest-dev"] = TestSetup(win={"Native":["visualc","mingw"],"MakeFile":["visualc","mingw"]},
                                     linux={"Native":["gcc"],"MakeFile":["gcc"]},
                                     osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"],"QMake":["clang"]})
configs["HeaderLibraryTest-dev"] = TestSetup(win={"Native":["visualc","mingw"],"MakeFile":["visualc","mingw"]},
                                             linux={"Native":["gcc"],"MakeFile":["gcc"],"QMake":["clang"]},
                                             osx={"Native":["llvm-gcc", "clang"],"MakeFile":["llvm-gcc", "clang"],"QMake":["clang"],"Xcode":["llvm-gcc", "clang"]})
