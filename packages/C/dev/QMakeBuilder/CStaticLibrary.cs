// <copyright file="CStaticLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object Build(C.StaticLibrary staticLibrary, out bool success)
        {
            Opus.Core.DependencyNode node = staticLibrary.OwningNode;
            Opus.Core.Target target = node.Target;

            NodeData nodeData = new NodeData();
            nodeData.Configuration = GetQtConfiguration(target);
            foreach (Opus.Core.DependencyNode childNode in node.Children)
            {
                NodeData childData = childNode.Data as NodeData;
                nodeData.Merge(childData);
            }

            C.ArchiverOptionCollection archiverOptionCollection = staticLibrary.Options as C.ArchiverOptionCollection;
            C.IArchiverOptions archiverOptions = staticLibrary.Options as C.IArchiverOptions;

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            if (archiverOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = archiverOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);
            }
            else
            {
                throw new Opus.Core.Exception("Archiver options does not support command line translation");
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

                string targetName = staticLibrary.OwningNode.ModuleName;
                nodeData.AddUniqueVariable("TARGET", new Opus.Core.StringArray(targetName));
                proFileWriter.WriteLine("TARGET = {0}", targetName);
                proFileWriter.WriteLine("TEMPLATE = lib");
                proFileWriter.WriteLine("CONFIG += {0}", nodeData.Configuration);
                proFileWriter.WriteLine("CONFIG += staticlib");

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
                    System.Reflection.BindingFlags fieldBindingFlags = System.Reflection.BindingFlags.Instance |
                                                                       System.Reflection.BindingFlags.Public |
                                                                       System.Reflection.BindingFlags.NonPublic;
                    System.Reflection.FieldInfo[] fields = staticLibrary.GetType().GetFields(fieldBindingFlags);
                    foreach (System.Reflection.FieldInfo field in fields)
                    {
                        var headerFileAttributes = field.GetCustomAttributes(typeof(C.HeaderFilesAttribute), false);
                        if (headerFileAttributes.Length > 0)
                        {
                            Opus.Core.FileCollection headerFileCollection = field.GetValue(staticLibrary) as Opus.Core.FileCollection;
                            if (headerFileCollection.Count > 0)
                            {
                                System.Text.StringBuilder headersStatement = new System.Text.StringBuilder();
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