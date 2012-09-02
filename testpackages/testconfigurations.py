#!/usr/bin/python
import sys

class TestConfiguration:
    _builders = []
    _win = []
    _linux = []
    _osx = []
  
    def __init__(self, builders=[], win=[], linux=[], osx=[]):
        if not builders:
            raise RuntimeError("No builder configured")
        self._builders = builders
        self._win = win
        self._linux = linux
        self._osx = osx
        
    def GetBuilders(self):
        return self._builders
     
    def GetResponseFiles(self):
        platform = sys.platform
        if platform.startswith("win"):
            responseFiles = []
            for i in self._win:
                responseFiles.append(i+".rsp")
            return responseFiles
        elif platform.startswith("linux"):
            responseFiles = []
            for i in self._linux:
                responseFiles.append(i+".rsp")
            return responseFiles
        elif platform.startswith("darwin"):
            responseFiles = []
            for i in self._osx:
                responseFiles.append(i+".rsp")
            return responseFiles
        else:
            raise RuntimeError("Unknown platform " + platform)
  
def GetTestConfig(packageName, options):
    try:
        config = configs[packageName]
    except KeyError, e:
        if options.verbose:
            print "No configuration for package: '%s'" % str(e)
        return None
    return config

nativeVSSolutionBuilders = ["Native","VSSolution"]
nativeMakeFileBuilders = ["Native","MakeFile"]
nativeMakeFileVSSolutionBuilders = ["Native","MakeFile","VSSolution"]
nativeMakeFileVSSolutionQMakeBuilders = ["Native","MakeFile","VSSolution","QMake"]

configs = {}
configs["Test-dev"] = TestConfiguration(builders=nativeMakeFileVSSolutionBuilders, win=["visualc"],linux=["gcc"],osx=["gcc"])
configs["Test2-dev"] = TestConfiguration(builders=nativeMakeFileVSSolutionBuilders, win=["visualc"],linux=["gcc"],osx=["gcc"])
configs["Test3-dev"] = TestConfiguration(builders=nativeMakeFileVSSolutionBuilders, win=["visualc"],linux=["gcc"],osx=["gcc"])
configs["Test4-dev"] = TestConfiguration(builders=nativeMakeFileVSSolutionBuilders, win=["visualc"],linux=["gcc"],osx=["gcc"])
configs["Test5-dev"] = TestConfiguration(builders=nativeMakeFileVSSolutionBuilders, win=["visualc"],linux=["gcc"],osx=["gcc"])
configs["Test6-dev"] = TestConfiguration(builders=nativeMakeFileVSSolutionBuilders, win=["visualc"],linux=["gcc"],osx=["gcc"])
configs["Test7-dev"] = TestConfiguration(builders=nativeMakeFileVSSolutionBuilders, win=["visualc"],linux=["gcc"],osx=["gcc"])
configs["Test8-dev"] = TestConfiguration(builders=nativeMakeFileVSSolutionBuilders, win=["visualc"],linux=["gcc"],osx=["gcc"])
configs["Test9-dev"] = TestConfiguration(builders=nativeMakeFileVSSolutionBuilders, win=["visualc"],linux=["gcc"],osx=["gcc"])
configs["Test10-dev"] = TestConfiguration(builders=nativeMakeFileVSSolutionBuilders, win=["visualc"],linux=["gcc"],osx=["gcc"])
configs["Test11-dev"] = TestConfiguration(builders=nativeMakeFileVSSolutionBuilders, win=["visualc"],linux=["gcc"],osx=["gcc"])
configs["Test12-dev"] = TestConfiguration(builders=nativeMakeFileVSSolutionBuilders, win=["visualc"],linux=["gcc"],osx=["gcc"])
configs["Test13-dev"] = TestConfiguration(builders=nativeMakeFileVSSolutionQMakeBuilders, win=["visualc"],linux=["gcc"],osx=["gcc"])
configs["Test14-dev"] = TestConfiguration(builders=nativeMakeFileVSSolutionBuilders, win=["visualc"],linux=["gcc"],osx=["gcc"])
configs["CodeGenTest-dev"] = TestConfiguration(builders=nativeMakeFileBuilders, win=["visualc"],linux=["gcc"],osx=["gcc"])
configs["CodeGenTest2-dev"] = TestConfiguration(builders=nativeMakeFileBuilders, win=["visualcandcsharp"],linux=["gccandcsharp"],osx=["gccandcsharp"])
configs["CSharpTest1-dev"] = TestConfiguration(builders=nativeMakeFileVSSolutionBuilders, win=["csharp"],linux=["monounix"],osx=["monoosx"])
configs["Direct3DTriangle-dev"] = TestConfiguration(builders=nativeMakeFileVSSolutionBuilders, win=["visualc"])
configs["MixedModeCpp-dev"] = TestConfiguration(builders=nativeMakeFileVSSolutionBuilders, win=["visualc"])
configs["MixedTest-dev"] = TestConfiguration(builders=nativeMakeFileBuilders, win=["visualcandcsharp"])
configs["OpenCLTest1-dev"] = TestConfiguration(builders=nativeVSSolutionBuilders, win=["visualc"])
configs["OpenGLUniformBufferTest-dev"] = TestConfiguration(builders=nativeMakeFileVSSolutionBuilders, win=["visualc"])
configs["RenderTextureAndProcessor-dev"] = TestConfiguration(builders=nativeMakeFileVSSolutionBuilders, win=["visualc"])
configs["Symlinks-dev"] = TestConfiguration(builders="Native", win=["notoolchain"],linux=["notoolchain"],osx=["notoolchain"])
configs["WPFTest-dev"] = TestConfiguration(builders=["VSSolution"], win=["csharp"])
