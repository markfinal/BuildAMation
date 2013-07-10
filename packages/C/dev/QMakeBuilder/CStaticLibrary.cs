// <copyright file="CStaticLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder2
    {
        public object Build(C.StaticLibrary moduleToBuild, out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var options = moduleToBuild.Options as C.ArchiverOptionCollection;

            var data = new QMakeData(node);
            foreach (var child in node.Children)
            {
                var childData = child.Data as QMakeData;
                if (null != childData)
                {
                    data.Merge(childData);
                }
            }

            data.Target = options.OutputName;
            data.Output = QMakeData.OutputType.StaticLibrary;
            data.DestDir = options.OutputDirectoryPath;

            success = true;
            return data;
        }
    }

    public sealed partial class QMakeBuilder
    {
        public object Build(C.StaticLibrary staticLibrary, out bool success)
        {
            var staticLibraryModule = staticLibrary as Opus.Core.BaseModule;
            var node = staticLibraryModule.OwningNode;
            var target = node.Target;

            var nodeData = new NodeData();
            nodeData.Configuration = GetQtConfiguration(target);
            foreach (var childNode in node.Children)
            {
                var childData = childNode.Data as NodeData;
                nodeData.Merge(childData);
            }

            var staticLibraryOptions = staticLibraryModule.Options;

            var archiverOptionCollection = staticLibraryOptions as C.ArchiverOptionCollection;
            var archiverOptions = staticLibraryOptions as C.IArchiverOptions;

            var commandLineBuilder = new Opus.Core.StringArray();
            if (archiverOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = archiverOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Opus.Core.Exception("Archiver options does not support command line translation");
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

                var targetName = node.ModuleName;
                nodeData.AddUniqueVariable("TARGET", new Opus.Core.StringArray(targetName));
                proFileWriter.WriteLine("TARGET = {0}", targetName);
                proFileWriter.WriteLine("TEMPLATE = lib");
                proFileWriter.WriteLine("CONFIG += {0}", nodeData.Configuration);
                proFileWriter.WriteLine("CONFIG += staticlib");

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
                    var fieldBindingFlags = System.Reflection.BindingFlags.Instance |
                                            System.Reflection.BindingFlags.Public |
                                            System.Reflection.BindingFlags.NonPublic;
                    var fields = staticLibrary.GetType().GetFields(fieldBindingFlags);
                    foreach (var field in fields)
                    {
                        var headerFileAttributes = field.GetCustomAttributes(typeof(C.HeaderFilesAttribute), false);
                        if (headerFileAttributes.Length > 0)
                        {
                            var headerFileCollection = field.GetValue(staticLibrary) as Opus.Core.FileCollection;
                            if (headerFileCollection.Count > 0)
                            {
                                var headersStatement = new System.Text.StringBuilder();
                                headersStatement.AppendFormat("{0}:HEADERS += ", nodeData.Configuration);
                                foreach (string headerPath in headerFileCollection)
                                {
                                    headersStatement.AppendFormat("\\\n\t{0}", headerPath.Replace('\\', '/'));
                                }
                                proFileWriter.WriteLine(headersStatement.ToString());
                            }
                        }
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

                // TODO: how do we specify archiver flags?

                // object file directory
                proFileWriter.WriteLine("{0}:OBJECTS_DIR = {1}", nodeData.Configuration, nodeData["OBJECTS_DIR"].ToString().Replace('\\', '/'));

                // output file directory
                proFileWriter.WriteLine("{0}:DESTDIR = {1}", nodeData.Configuration, archiverOptionCollection.OutputDirectoryPath.Replace('\\', '/'));
            }

            success = true;
            return nodeData;
        }
    }
}