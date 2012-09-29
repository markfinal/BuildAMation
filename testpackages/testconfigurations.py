#!/usr/bin/python
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
     
    def GetResponseFiles(self, builder):
        platform = sys.platform
        responseFiles = []
        # TODO: can we do this with a lambda expression?
        if platform.startswith("win"):
            for i in self._win[builder]:
                responseFiles.append(i+".rsp")
        elif platform.startswith("linux"):
            for i in self._linux[builder]:
                responseFiles.append(i+".rsp")
        elif platform.startswith("darwin"):
            for i in self._osx[builder]:
                responseFiles.append(i+".rsp")
        else:
            raise RuntimeError("Unknown platform " + platform)
        return responseFiles
            
def GetTestConfig(packageName, options):
    try:
        config = configs[packageName]
    except KeyError, e:
        if options.verbose:
            print "No configuration for package: '%s'" % str(e)
        return None
    return config

configs = {}
configs["Test-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"]},
                                linux={"Native":["gcc"],"MakeFile":["gcc"]},
                                osx={"Native":["gcc"]})
configs["Test2-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"]},
                                 linux={"Native":["gcc"],"MakeFile":["gcc"]},
                                 osx={"Native":["gcc"],"MakeFile":["gcc"]})
configs["Test3-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"]},
                                 linux={"Native":["gcc"],"MakeFile":["gcc"]},
                                 osx={"Native":["gcc"],"MakeFile":["gcc"]})
configs["Test4-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"]},
                                 linux={"Native":["gcc"],"MakeFile":["gcc"]},
                                 osx={"Native":["gcc"],"MakeFile":["gcc"]})
configs["Test5-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"]},
                                 linux={"Native":["gcc"],"MakeFile":["gcc"]},
                                 osx={"Native":["gcc"],"MakeFile":["gcc"]})
configs["Test6-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"]},
                                 linux={"Native":["gcc"],"MakeFile":["gcc"]},
                                 osx={"Native":["gcc"],"MakeFile":["gcc"]})
configs["Test7-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"]},
                                 linux={"Native":["gcc"],"MakeFile":["gcc"]},
                                 osx={"Native":["gcc"],"MakeFile":["gcc"]})
configs["Test8-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"]},
                                 linux={"Native":["gcc"],"MakeFile":["gcc"]},
                                 osx={"Native":["gcc"],"MakeFile":["gcc"]})
configs["Test9-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"]},
                                 linux={"Native":["gcc"],"MakeFile":["gcc"]},
                                 osx={"Native":["gcc"],"MakeFile":["gcc"]})
configs["Test10-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"]},
                                  linux={"Native":["gcc"],"MakeFile":["gcc"]},
                                  osx={"Native":["gcc"],"MakeFile":["gcc"]})
configs["Test11-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"]},
                                  linux={"Native":["gcc"],"MakeFile":["gcc"]},
                                  osx={"Native":["gcc"],"MakeFile":["gcc"]})
configs["Test12-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"]},
                                  linux={"Native":["gcc"],"MakeFile":["gcc"]},
                                  osx={"Native":["gcc"],"MakeFile":["gcc"]})
configs["Test13-dev"] = TestSetup(win={"Native":["visualc"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"],"QMake":["visualc"]},
                                  linux={"Native":["gcc"],"MakeFile":["gcc"],"QMake":["gcc"]},
                                  osx={"Native":["gcc"],"MakeFile":["gcc"]})
configs["Test14-dev"] = TestSetup(win={"Native":["visualc","mingw"],"VSSolution":["visualc"],"MakeFile":["visualc","mingw"]},
                                  linux={"Native":["gcc"],"MakeFile":["gcc"]},
                                  osx={"Native":["gcc"],"MakeFile":["gcc"]})
configs["CodeGenTest-dev"] = TestSetup(win={"Native":["visualc","mingw"],"MakeFile":["visualc","mingw"]},
                                       linux={"Native":["gcc"],"MakeFile":["gcc"]},
                                       osx={"Native":["gcc"],"MakeFile":["gcc"]})
configs["CodeGenTest2-dev"] = TestSetup(win={"Native":["visualcandcsharp"],"MakeFile":["visualcandcsharp"]},
                                        linux={"Native":["gccandcsharp"],"MakeFile":["gccandcsharp"]},
                                        osx={"Native":["gccandcsharp"],"MakeFile":["gccandcsharp"]})
configs["CSharpTest1-dev"] = TestSetup(win={"Native":["csharp"],"MakeFile":["csharp"],"VSSolution":["csharp"]},
                                       linux={"Native":["monounix"],"MakeFile":["monounix"]},
                                       osx={"Native":["monounix"],"MakeFile":["monounix"]})
configs["Direct3DTriangle-dev"] = TestSetup(win={"Native":["visualc"],"VSSolution":["visualc"],"MakeFile":["visualc"]})
configs["MixedModeCpp-dev"] = TestSetup(win={"Native":["visualc"],"VSSolution":["visualc"],"MakeFile":["visualc"]})
configs["MixedTest-dev"] = TestSetup(win={"Native":["visualcandcsharp"],"MakeFile":["visualcandcsharp"]})
configs["OpenCLTest1-dev"] = TestSetup(win={"Native":["visualc"],"VSSolution":["visualc"],"MakeFile":["visualc"]})
configs["OpenGLUniformBufferTest-dev"] = TestSetup(win={"Native":["visualc"],"VSSolution":["visualc"],"MakeFile":["visualc"]})
configs["RenderTextureAndProcessor-dev"] = TestSetup(win={"Native":["visualc"],"VSSolution":["visualc"],"MakeFile":["visualc"]})
configs["Symlinks-dev"] = TestSetup(win={"Native":["notoolchain"]},
                                    linux={"Native":["notoolchain"]},
                                    osx={"Native":["notoolchain"]})
configs["WPFTest-dev"] = TestSetup(win={"VSSolution":["csharp"]})
