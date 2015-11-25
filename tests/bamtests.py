from testconfigurations import TestSetup, visualc, visualc64, mingw32, gcc, gcc64, clang, clang64

def ConfigureRepository():
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
                                       osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
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
    configs["DeltaSettingsTest1"] = TestSetup(win={"Native":[visualc64],"VSSolution":[visualc64]},
                                              linux={"Native":[gcc64]},
                                              osx={"Native":[clang64],"Xcode":[clang64]})
    configs["InstallerTest1"] = TestSetup(win={"Native":[visualc64],"VSSolution":[visualc64]},
                                          linux={"Native":[gcc64],"MakeFile":[gcc64]},
                                          osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
    configs["MultiBitDepthModuleTest"] = TestSetup(win={"Native":[visualc],"VSSolution":[visualc]},
                                                   linux={"Native":[gcc],"MakeFile":[gcc]},
                                                   osx={"Native":[clang],"MakeFile":[clang],"Xcode":[clang]})
    configs["DuplicateSourceFilenameTest"] = TestSetup(win={"Native":[visualc],"VSSolution":[visualc]},
                                                       linux={"Native":[gcc],"MakeFile":[gcc]},
                                                       osx={"Native":[clang],"MakeFile":[clang]})
    return configs
