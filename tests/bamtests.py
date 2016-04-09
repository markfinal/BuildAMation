from testconfigurations import TestSetup, visualc, visualc64, visualc32, mingw32, gcc, gcc64, gcc32, clang, clang32, clang64


def configure_repository():
    configs = {}
    configs["Test"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["Test2"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                 linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                 osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["Test3"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                 linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                 osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["Test4"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                 linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                 osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["Test5"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                 linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                 osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["Test6"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                 linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                 osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["Test7"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                 linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                 osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["Test8"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]})
    configs["Test9"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                 linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                 osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["Test10"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                  linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                  osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["Test11"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                  linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                  osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["Test12"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                  linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                  osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["Test13"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                  linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                  osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["Test14"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                  linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                  osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["Test15"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                  linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                  osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["Test16"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                  linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                  osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["CodeGenTest"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                       linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                       osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["CocoaTest1"] = TestSetup(osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["ObjectiveCTest1"] = TestSetup(osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["ProxyTest"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                     linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                     osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32]})
    configs["HeaderLibraryTest"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                             linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                             osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["Cxx11Test1"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                      linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                      osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["PluginTest"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                      linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                      osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["DeltaSettingsTest1"] = TestSetup(win={"Native": [visualc64, visualc32], "VSSolution": [visualc64, visualc32]},
                                              linux={"Native": [gcc64, gcc32]},
                                              osx={"Native": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["InstallerTest1"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32]},
                                          linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                          osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["MultiBitDepthModuleTest"] = TestSetup(win={"Native": [visualc], "VSSolution": [visualc]},
                                                   linux={"Native": [gcc], "MakeFile": [gcc]},
                                                   osx={"Native": [clang], "MakeFile": [clang], "Xcode": [clang]})
    configs["DuplicateSourceFilenameTest"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32]},
                                                       linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                                       osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["PreprocessorStringTest1"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32]},
                                                   linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                                   osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["PublishingTest1"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32]},
                                           linux={"Native": [gcc64, gcc32]},
                                           osx={"Native": [clang64, clang32], "Xcode": [clang64, clang32]})
    configs["PublishingTest2"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32]},
                                           linux={"Native": [gcc64, gcc32]},
                                           osx={"Native": [clang64, clang32]})
    configs["ProceduralHeaderTest1"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32]},
                                                 linux={"Native": [gcc64, gcc32]},
                                                 osx={"Native": [clang64, clang32]})
    configs["EmbedStaticIntoDynamicLibrary"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32]},
                                                         linux={"Native": [gcc64, gcc32]},
                                                         osx={"Native": [clang64, clang32]})
    configs["LinkPrebuiltLibrary"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32]},
                                               linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                               osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]})
    return configs
