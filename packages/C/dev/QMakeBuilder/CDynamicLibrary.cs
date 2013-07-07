// <copyright file="CDynamicLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder2
    {
        public object Build(C.DynamicLibrary moduleToBuild, out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var options = moduleToBuild.Options as C.LinkerOptionCollection;

            var data = new QMakeData(node);
            foreach (var child in node.Children)
            {
                var childData = child.Data as QMakeData;
                if (null != childData)
                {
                    data.Merge(childData);
                }
            }
            if (null != node.ExternalDependents)
            {
                foreach (var dependent in node.ExternalDependents)
                {
                    var depData = dependent.Data as QMakeData;
                    if (null != depData)
                    {
                        data.Merge(depData, QMakeData.OutputType.StaticLibrary | QMakeData.OutputType.DynamicLibrary);
                    }
                }
            }

            data.Output = QMakeData.OutputType.DynamicLibrary;
            data.DestDir = options.OutputDirectoryPath;

            // find dependent library files
            if (null != node.ExternalDependents)
            {
                var dependentLibraryFiles = new Opus.Core.StringArray();
                node.ExternalDependents.FilterOutputPaths(C.OutputFileFlags.StaticLibrary | C.OutputFileFlags.StaticImportLibrary, dependentLibraryFiles);
                data.Libraries.AddRangeUnique(dependentLibraryFiles);
            }

            var optionsInterface = moduleToBuild.Options as C.ILinkerOptions;

            // find static library files
            data.Libraries.AddRangeUnique(optionsInterface.Libraries.ToStringArray());

            success = true;
            return data;
        }
    }

    public sealed partial class QMakeBuilder
    {
        public object Build(C.DynamicLibrary dynamicLibrary, out bool success)
        {
            var dynamicLibraryModule = dynamicLibrary as Opus.Core.BaseModule;
            var node = dynamicLibraryModule.OwningNode;
            var target = node.Target;

            var nodeData = new NodeData();
            nodeData.Configuration = GetQtConfiguration(target);
            foreach (var childNode in node.Children)
            {
                var childData = childNode.Data as NodeData;
                nodeData.Merge(childData);
            }

            var dynamicLibraryOptions = dynamicLibraryModule.Options;
            var linkerOptionCollection = dynamicLibraryOptions as C.LinkerOptionCollection;
            var linkerOptions = dynamicLibraryOptions as C.ILinkerOptions;

            {
                var linkerTool = target.Toolset.Tool(typeof(C.ILinkerTool));
                nodeData.AddUniqueVariable("QMAKE_LINK", new Opus.Core.StringArray(linkerTool.Executable((Opus.Core.BaseTarget)target).Replace("\\", "/")));
            }

            var commandLineBuilder = new Opus.Core.StringArray();
            if (linkerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = linkerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
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

            var proFilePath = QMakeBuilder.GetProFilePath(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(proFilePath));
            nodeData.ProFilePathName = proFilePath;

            using (var proFileWriter = new System.IO.StreamWriter(proFilePath))
            {
                proFileWriter.WriteLine("# --- Written by Opus");

                {
                    var relativePriPathName = Opus.Core.RelativePathUtilities.GetPath(this.DisableQtPriPathName, proFilePath);
                    proFileWriter.WriteLine("include({0})", relativePriPathName.Replace('\\', '/'));
                }

                var targetName = dynamicLibraryModule.OwningNode.ModuleName;
                nodeData.AddUniqueVariable("TARGET", new Opus.Core.StringArray(targetName));
                proFileWriter.WriteLine("TARGET = {0}", targetName);
                proFileWriter.WriteLine("TEMPLATE = lib");
                proFileWriter.WriteLine("CONFIG += {0}", nodeData.Configuration);

                // sources
                {
                    var sourcesArray = nodeData["SOURCES"];
                    var sourcesStatement = new System.Text.StringBuilder();
                    sourcesStatement.AppendFormat("{0}:SOURCES += ", nodeData.Configuration);
                    foreach (var source in sourcesArray)
                    {
                        sourcesStatement.AppendFormat("\\\n\t{0}", source.Replace('\\', '/'));
                    }
                    proFileWriter.WriteLine(sourcesStatement.ToString());
                }

                // headers
                {
                    var headerFiles = new Opus.Core.StringArray();

                    // moc headers
                    if (nodeData.Contains("HEADERS"))
                    {
                        headerFiles.AddRange(nodeData["HEADERS"]);
                    }

                    // other headers
                    var fieldBindingFlags = System.Reflection.BindingFlags.Instance |
                                            System.Reflection.BindingFlags.Public |
                                            System.Reflection.BindingFlags.NonPublic;
                    var fields = dynamicLibrary.GetType().GetFields(fieldBindingFlags);
                    foreach (var field in fields)
                    {
                        var headerFileAttributes = field.GetCustomAttributes(typeof(C.HeaderFilesAttribute), false);
                        if (headerFileAttributes.Length > 0)
                        {
                            var headerFileCollection = field.GetValue(dynamicLibrary) as Opus.Core.FileCollection;
                            foreach (string fileName in headerFileCollection)
                            {
                                headerFiles.Add(fileName);
                            }
                        }
                    }

                    headerFiles.RemoveDuplicates();

                    if (headerFiles.Count > 0)
                    {
                        var headersStatement = new System.Text.StringBuilder();
                        headersStatement.AppendFormat("{0}:HEADERS += ", nodeData.Configuration);
                        foreach (var header in headerFiles)
                        {
                            headersStatement.AppendFormat("\\\n\t{0}", header.Replace('\\', '/'));
                        }
                        proFileWriter.WriteLine(headersStatement.ToString());
                    }
                }

                // cflags, cxxflags, include paths, defines, and tools
                {
                    var includePathsStatement = new System.Text.StringBuilder();
                    var definesStatement = new System.Text.StringBuilder();
                    includePathsStatement.AppendFormat("{0}:INCLUDEPATH += ", nodeData.Configuration);
                    definesStatement.AppendFormat("{0}:DEFINES += ", nodeData.Configuration);

                    if (nodeData.Contains("CFLAGS"))
                    {
                        var cflags = nodeData["CFLAGS"];
                        var cflagsStatement = new System.Text.StringBuilder();
                        cflagsStatement.AppendFormat("{0}:QMAKE_CFLAGS += ", nodeData.Configuration);
                        this.ProcessCompilerFlags(cflags, cflagsStatement, includePathsStatement, definesStatement);
                        proFileWriter.WriteLine(cflagsStatement.ToString());
                    }

                    if (nodeData.Contains("CXXFLAGS"))
                    {
                        var cxxflags = nodeData["CXXFLAGS"];
                        var cxxflagsStatement = new System.Text.StringBuilder();
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
                    var linkFlagsStatement = new System.Text.StringBuilder();
                    var libDirStatement = new System.Text.StringBuilder();
                    linkFlagsStatement.AppendFormat("{0}:QMAKE_LFLAGS += ", nodeData.Configuration);
                    libDirStatement.AppendFormat("{0}:QMAKE_LIBDIR += ", nodeData.Configuration);
                    foreach (var linkFlag in commandLineBuilder)
                    {
                        if (linkFlag.StartsWith("-o") || linkFlag.StartsWith("/OUT:"))
                        {
                            // don't include any output path
                            continue;
                        }

                        var linkFlagModified = linkFlag;
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
                    var libraryFiles = new Opus.Core.StringArray();
                    var linkerTool = target.Toolset.Tool(typeof(C.ILinkerTool)) as C.ILinkerTool;
                    C.LinkerUtilities.AppendLibrariesToCommandLine(libraryFiles, linkerTool, linkerOptions, dependentLibraryFiles);

                    var libStatement = new System.Text.StringBuilder();
                    libStatement.AppendFormat("{0}:QMAKE_LIBS += ", nodeData.Configuration);
                    foreach (var lib in libraryFiles)
                    {
                        libStatement.AppendFormat("\\\n\t{0}", lib.Replace('\\', '/'));
                    }
                    proFileWriter.WriteLine(libStatement.ToString());

                    if (null != dependentLibraryFiles)
                    {
                        foreach (var dependentLibrary in dependentLibraryFiles)
                        {
                            proFileWriter.WriteLine("{0}:PRE_TARGETDEPS += {1}", nodeData.Configuration, dependentLibrary.Replace('\\', '/'));
                        }
                    }
                }

                // object file directory
                proFileWriter.WriteLine("{0}:OBJECTS_DIR = {1}", nodeData.Configuration, nodeData["OBJECTS_DIR"].ToString().Replace('\\', '/'));

                // faking the lib directory (via MOC, probably do this better through QMAKE_EXTRA_TARGET)
                proFileWriter.WriteLine("{0}:MOC_DIR = {1}", nodeData.Configuration, linkerOptionCollection.LibraryDirectoryPath.Replace('\\', '/'));

                // binary file directory
                proFileWriter.WriteLine("{0}:DESTDIR = {1}", nodeData.Configuration, linkerOptionCollection.OutputDirectoryPath.Replace('\\', '/'));
            }

            success = true;
            return nodeData;
        }
    }
}