// <copyright file="CStaticLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QtCreatorBuilder
{
    public sealed partial class QtCreatorBuilder
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

            string proFilePath = QtCreatorBuilder.GetProFilePath(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(proFilePath));
            nodeData.ProjectFileDirectory = System.IO.Path.GetDirectoryName(proFilePath);

            using (System.IO.TextWriter proFileWriter = new System.IO.StreamWriter(proFilePath))
            {
                proFileWriter.WriteLine("# --- Written by Opus");

                {
                    string relativePriPathName = Opus.Core.RelativePathUtilities.GetPath(this.DisableQtPriPathName, proFilePath);
                    proFileWriter.WriteLine("include({0})", relativePriPathName.Replace('\\', '/'));
                }

                proFileWriter.WriteLine("TARGET = {0}", staticLibrary.OwningNode.ModuleName);
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

                // cflags and include paths
                {
                    Opus.Core.StringArray cflags = nodeData["CFLAGS"];
                    // TODO: need to replace "<X>" with $$quote(<X>)
                    System.Text.StringBuilder cflagsStatement = new System.Text.StringBuilder();
                    System.Text.StringBuilder includePathsStatement = new System.Text.StringBuilder();
                    cflagsStatement.AppendFormat("{0}:QMAKE_CFLAGS += ", nodeData.Configuration);
                    includePathsStatement.AppendFormat("{0}:INCLUDEPATH += ", nodeData.Configuration);
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
                        else
                        {
                            cflagsStatement.AppendFormat("\\\n\t{0}", cflagModified.Replace('\\', '/'));
                        }
                    }
                    proFileWriter.WriteLine(cflagsStatement.ToString());
                    proFileWriter.WriteLine(includePathsStatement.ToString());
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