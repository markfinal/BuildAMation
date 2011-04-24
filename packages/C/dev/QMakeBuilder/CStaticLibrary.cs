// <copyright file="CStaticLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object Build(C.StaticLibrary staticLibrary, Opus.Core.DependencyNode node, out bool success)
        {
            NodeData nodeData = new NodeData();
            nodeData.Configuration = GetQtConfiguration(node.Target);
            foreach (Opus.Core.DependencyNode childNode in node.Children)
            {
                NodeData childData = childNode.Data as NodeData;
                nodeData.Merge(childData);
            }

            C.ArchiverOptionCollection archiverOptionCollection = node.Module.Options as C.ArchiverOptionCollection;
            C.IArchiverOptions archiverOptions = node.Module.Options as C.IArchiverOptions;
            Opus.Core.Target target = node.Target;

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

                // cflags, include paths and defines
                {
                    Opus.Core.StringArray cflags = nodeData["CFLAGS"];
                    // TODO: need to replace "<X>" with $$quote(<X>)
                    System.Text.StringBuilder cflagsStatement = new System.Text.StringBuilder();
                    System.Text.StringBuilder includePathsStatement = new System.Text.StringBuilder();
                    System.Text.StringBuilder definesStatement = new System.Text.StringBuilder();
                    cflagsStatement.AppendFormat("{0}:QMAKE_CFLAGS += ", nodeData.Configuration);
                    includePathsStatement.AppendFormat("{0}:INCLUDEPATH += ", nodeData.Configuration);
                    definesStatement.AppendFormat("{0}:DEFINES += ", nodeData.Configuration);
                    foreach (string cflag in cflags)
                    {
                        if (cflag.StartsWith("-o") || cflag.StartsWith("/Fo"))
                        {
                            // don't include any output path
                            continue;
                        }

                        string cflagModified = cflag;
                        if (cflag.Contains("\""))
                        {
                            int indexOfFirstQuote = cflag.IndexOf('"');
                            cflagModified = cflag.Substring(0, indexOfFirstQuote);
                            cflagModified += "$$quote(";
                            int indexOfLastQuote = cflag.IndexOf('"', indexOfFirstQuote + 1);
                            cflagModified += cflag.Substring(indexOfFirstQuote + 1, indexOfLastQuote - indexOfFirstQuote - 1);
                            cflagModified += ")";
                            cflagModified += cflag.Substring(indexOfLastQuote + 1);
                        }

                        if (cflagModified.StartsWith("-I") || cflagModified.StartsWith("-isystem") || cflagModified.StartsWith("/I"))
                        {
                            // strip the include path command
                            if (cflagModified.StartsWith("-isystem"))
                            {
                                cflagModified = cflagModified.Remove(0, 8);
                            }
                            else
                            {
                                cflagModified = cflagModified.Remove(0, 2);
                            }
                            includePathsStatement.AppendFormat("\\\n\t{0}", cflagModified.Replace('\\', '/'));
                        }
                        else if (cflagModified.StartsWith("-D") || cflagModified.StartsWith("/D"))
                        {
                            // strip the define command
                            cflagModified = cflagModified.Remove(0, 2);
                            definesStatement.AppendFormat("\\\n\t{0}", cflagModified.Replace('\\', '/'));
                        }
                        else
                        {
                            cflagsStatement.AppendFormat("\\\n\t{0}", cflagModified.Replace('\\', '/'));
                        }
                    }
                    proFileWriter.WriteLine(cflagsStatement.ToString());
                    proFileWriter.WriteLine("{0}:QMAKE_CXXFLAGS = $$QMAKE_CFLAGS", nodeData.Configuration);
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