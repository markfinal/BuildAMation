// <copyright file="CApplication.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object Build(C.Application application, out bool success)
        {
            Opus.Core.IModule applicationModule = application as Opus.Core.IModule;
            Opus.Core.DependencyNode node = applicationModule.OwningNode;
            Opus.Core.Target target = node.Target;

            NodeData nodeData = new NodeData();
            nodeData.Configuration = GetQtConfiguration(target);
            foreach (Opus.Core.DependencyNode childNode in node.Children)
            {
                NodeData childData = childNode.Data as NodeData;
                nodeData.Merge(childData);
            }

            Opus.Core.BaseOptionCollection applicationOptions = applicationModule.Options;
            C.LinkerOptionCollection linkerOptionCollection = applicationOptions as C.LinkerOptionCollection;
            C.ILinkerOptions linkerOptions = applicationOptions as C.ILinkerOptions;

            {
                Opus.Core.ITool linkerTool = target.Toolset.Tool(typeof(C.ILinkerTool));
                nodeData.AddUniqueVariable("QMAKE_LINK", new Opus.Core.StringArray(linkerTool.Executable(target).Replace("\\", "/")));
            }

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            if (linkerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = linkerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);
            }
            else
            {
                throw new Opus.Core.Exception("Linker options does not support command line translation");
            }

            // find dependent library files
            Opus.Core.StringArray dependentLibraryFiles = null;
            if (null != node.ExternalDependents)
            {
                dependentLibraryFiles = new Opus.Core.StringArray();
                node.ExternalDependents.FilterOutputPaths(C.OutputFileFlags.StaticLibrary | C.OutputFileFlags.StaticImportLibrary, dependentLibraryFiles);
            }

            string proFilePath = QMakeBuilder.GetProFilePath(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(proFilePath));
            nodeData.ProFilePathName = proFilePath;

            using (System.IO.TextWriter proFileWriter = new System.IO.StreamWriter(proFilePath))
            {
                proFileWriter.WriteLine("# --- Written by Opus");

                {
                    string relativePriPathName = Opus.Core.RelativePathUtilities.GetPath(this.DisableQtPriPathName, proFilePath);
                    proFileWriter.WriteLine("include({0})", relativePriPathName.Replace('\\', '/'));
                }

                string targetName = applicationModule.OwningNode.ModuleName;
                nodeData.AddUniqueVariable("TARGET", new Opus.Core.StringArray(targetName));
                proFileWriter.WriteLine("TARGET = {0}", targetName);
                proFileWriter.WriteLine("TEMPLATE = app");
                proFileWriter.WriteLine("CONFIG += {0}", nodeData.Configuration);
                proFileWriter.WriteLine("CONFIG += console");

                // sources
                {
                    Opus.Core.StringArray sourcesArray = nodeData["SOURCES"];
                    System.Text.StringBuilder sourcesStatement = new System.Text.StringBuilder();
                    sourcesStatement.AppendFormat("{0}:SOURCES += ", nodeData.Configuration);
                    foreach (string source in sourcesArray)
                    {
                        sourcesStatement.AppendFormat("\\\n\t{0}", source.Replace('\\', '/'));
                    }
                    proFileWriter.WriteLine(sourcesStatement.ToString());
                }

                // headers
                {
                    Opus.Core.StringArray headerFiles = new Opus.Core.StringArray();

                    // moc headers
                    if (nodeData.Contains("HEADERS"))
                    {
                        headerFiles.AddRange(nodeData["HEADERS"]);
                    }

                    // other headers
                    System.Reflection.BindingFlags fieldBindingFlags = System.Reflection.BindingFlags.Instance |
                                                                       System.Reflection.BindingFlags.Public |
                                                                       System.Reflection.BindingFlags.NonPublic;
                    System.Reflection.FieldInfo[] fields = application.GetType().GetFields(fieldBindingFlags);
                    foreach (System.Reflection.FieldInfo field in fields)
                    {
                        var headerFileAttributes = field.GetCustomAttributes(typeof(C.HeaderFilesAttribute), false);
                        if (headerFileAttributes.Length > 0)
                        {
                            Opus.Core.FileCollection headerFileCollection = field.GetValue(application) as Opus.Core.FileCollection;
                            foreach (string fileName in headerFileCollection)
                            {
                                headerFiles.Add(fileName);
                            }
                        }
                    }

                    headerFiles.RemoveDuplicates();

                    if (headerFiles.Count > 0)
                    {
                        System.Text.StringBuilder headersStatement = new System.Text.StringBuilder();
                        headersStatement.AppendFormat("{0}:HEADERS += ", nodeData.Configuration);
                        foreach (string header in headerFiles)
                        {
                            headersStatement.AppendFormat("\\\n\t{0}", header.Replace('\\', '/'));
                        }
                        proFileWriter.WriteLine(headersStatement.ToString());
                    }
                }

                // cflags, cxxflags, include paths, defines, and tools
                {
                    System.Text.StringBuilder includePathsStatement = new System.Text.StringBuilder();
                    System.Text.StringBuilder definesStatement = new System.Text.StringBuilder();
                    includePathsStatement.AppendFormat("{0}:INCLUDEPATH += ", nodeData.Configuration);
                    definesStatement.AppendFormat("{0}:DEFINES += ", nodeData.Configuration);

                    if (nodeData.Contains("CFLAGS"))
                    {
                        Opus.Core.StringArray cflags = nodeData["CFLAGS"];
                        System.Text.StringBuilder cflagsStatement = new System.Text.StringBuilder();
                        cflagsStatement.AppendFormat("{0}:QMAKE_CFLAGS += ", nodeData.Configuration);
                        this.ProcessCompilerFlags(cflags, cflagsStatement, includePathsStatement, definesStatement);
                        proFileWriter.WriteLine(cflagsStatement.ToString());
                    }

                    if (nodeData.Contains("CXXFLAGS"))
                    {
                        Opus.Core.StringArray cxxflags = nodeData["CXXFLAGS"];
                        System.Text.StringBuilder cxxflagsStatement = new System.Text.StringBuilder();
                        cxxflagsStatement.AppendFormat("{0}:QMAKE_CXXFLAGS += ", nodeData.Configuration);
                        this.ProcessCompilerFlags(cxxflags, cxxflagsStatement, includePathsStatement, definesStatement);
                        proFileWriter.WriteLine(cxxflagsStatement.ToString());
                    }

                    if (nodeData.Contains("QMAKE_CC"))
                    {
                        proFileWriter.WriteLine("{0}:QMAKE_CC = $$quote({1})", nodeData.Configuration, nodeData["QMAKE_CC"]);
                    }
                    if (nodeData.Contains("QMAKE_CXX"))
                    {
                        proFileWriter.WriteLine("{0}:QMAKE_CXX = $$quote({1})", nodeData.Configuration, nodeData["QMAKE_CXX"]);
                    }
                    if (nodeData.Contains("QMAKE_LINK"))
                    {
                        proFileWriter.WriteLine("{0}:QMAKE_LINK = $$quote({1})", nodeData.Configuration, nodeData["QMAKE_LINK"]);
                    }
                    if (nodeData.Contains("QMAKE_MOC"))
                    {
                        proFileWriter.WriteLine("{0}:QMAKE_MOC = $$quote({1})", nodeData.Configuration, nodeData["QMAKE_MOC"]);
                    }

                    proFileWriter.WriteLine(includePathsStatement.ToString());
                    proFileWriter.WriteLine(definesStatement.ToString());
                }

                // link flags
                {
                    System.Text.StringBuilder linkFlagsStatement = new System.Text.StringBuilder();
                    System.Text.StringBuilder libDirStatement = new System.Text.StringBuilder();
                    linkFlagsStatement.AppendFormat("{0}:QMAKE_LFLAGS += ", nodeData.Configuration);
                    libDirStatement.AppendFormat("{0}:QMAKE_LIBDIR += ", nodeData.Configuration);
                    foreach (string linkFlag in commandLineBuilder)
                    {
                        if (linkFlag.StartsWith("-o") || linkFlag.StartsWith("/OUT:"))
                        {
                            // don't include any output path
                            continue;
                        }

                        string linkFlagModified = linkFlag;
                        if (linkFlag.Contains("\""))
                        {
                            int indexOfFirstQuote = linkFlag.IndexOf('"');
                            linkFlagModified = linkFlag.Substring(0, indexOfFirstQuote);
                            linkFlagModified += "$$quote(";
                            int indexOfLastQuote = linkFlag.IndexOf('"', indexOfFirstQuote + 1);
                            linkFlagModified += linkFlag.Substring(indexOfFirstQuote + 1, indexOfLastQuote - indexOfFirstQuote - 1);
                            linkFlagModified += ")";
                            linkFlagModified += linkFlag.Substring(indexOfLastQuote + 1);
                        }

                        if (linkFlagModified.StartsWith("-L") || linkFlagModified.StartsWith("/LIBPATH:"))
                        {
                            // strip the lib path command
                            if (linkFlagModified.StartsWith("/LIBPATH:"))
                            {
                                linkFlagModified = linkFlagModified.Remove(0, 9);
                            }
                            else
                            {
                                linkFlagModified = linkFlagModified.Remove(0, 2);
                            }
                            libDirStatement.AppendFormat("\\\n\t{0}", linkFlagModified.Replace('\\', '/'));
                        }
                        else
                        {
                            linkFlagsStatement.AppendFormat("\\\n\t{0}", linkFlagModified.Replace('\\', '/'));
                        }
                    }
                    proFileWriter.WriteLine(linkFlagsStatement.ToString());
                    proFileWriter.WriteLine(libDirStatement.ToString());
                }

                // libraries
                {
                    // NEW STYLE
                    Opus.Core.StringArray libraryFiles = new Opus.Core.StringArray();
#if true
                    C.ILinkerTool linkerTool = target.Toolset.Tool(typeof(C.ILinkerTool)) as C.ILinkerTool;
                    C.LinkerUtilities.AppendLibrariesToCommandLine(libraryFiles, linkerTool, linkerOptions, dependentLibraryFiles);
#else
                    C.Linker linkerInstance = C.LinkerFactory.GetTargetInstance(target);
                    linkerInstance.AppendLibrariesToCommandLine(libraryFiles, linkerOptions, dependentLibraryFiles);
#endif

                    System.Text.StringBuilder libStatement = new System.Text.StringBuilder();
                    libStatement.AppendFormat("{0}:QMAKE_LIBS += ", nodeData.Configuration);
                    foreach (string lib in libraryFiles)
                    {
                        libStatement.AppendFormat("\\\n\t{0}", lib.Replace('\\', '/'));
                    }
                    proFileWriter.WriteLine(libStatement.ToString());

                    if (null != dependentLibraryFiles)
                    {
                        foreach (string dependentLibrary in dependentLibraryFiles)
                        {
                            proFileWriter.WriteLine("{0}:PRE_TARGETDEPS += {1}", nodeData.Configuration, dependentLibrary.Replace('\\', '/'));
                        }
                    }
                }

                // object file directory
                proFileWriter.WriteLine("{0}:OBJECTS_DIR = {1}", nodeData.Configuration, nodeData["OBJECTS_DIR"].ToString().Replace('\\', '/'));

                // binary file directory
                proFileWriter.WriteLine("{0}:DESTDIR = {1}", nodeData.Configuration, linkerOptionCollection.OutputDirectoryPath.Replace('\\', '/'));
            }

            success = true;
            return nodeData;
        }
    }
}