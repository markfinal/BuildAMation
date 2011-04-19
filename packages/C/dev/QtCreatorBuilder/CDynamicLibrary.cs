// <copyright file="CDynamicLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QtCreatorBuilder
{
    public sealed partial class QtCreatorBuilder
    {
        public object Build(C.DynamicLibrary dynamicLibrary, Opus.Core.DependencyNode node, out bool success)
        {
            NodeData nodeData = new NodeData();
            nodeData.Configuration = GetQtConfiguration(node.Target);
            foreach (Opus.Core.DependencyNode childNode in node.Children)
            {
                NodeData childData = childNode.Data as NodeData;
                nodeData.Merge(childData);
            }

            C.LinkerOptionCollection linkerOptionCollection = node.Module.Options as C.LinkerOptionCollection;
            C.ILinkerOptions linkerOptions = node.Module.Options as C.ILinkerOptions;
            C.IToolchainOptions toolchainOptions = (dynamicLibrary.Options as C.ILinkerOptions).ToolchainOptionCollection as C.IToolchainOptions;
            Opus.Core.Target target = node.Target;

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

                proFileWriter.WriteLine("TARGET = {0}", dynamicLibrary.OwningNode.ModuleName);
                proFileWriter.WriteLine("TEMPLATE = lib");
                proFileWriter.WriteLine("CONFIG += {0}", nodeData.Configuration);

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
                    C.Linker linkerInstance = C.LinkerFactory.GetTargetInstance(target);
                    Opus.Core.StringArray libraryFiles = new Opus.Core.StringArray();
                    linkerInstance.AppendLibrariesToCommandLine(libraryFiles, linkerOptions, dependentLibraryFiles);

                    System.Text.StringBuilder libStatement = new System.Text.StringBuilder();
                    libStatement.AppendFormat("{0}:QMAKE_LIBS += ", nodeData.Configuration);
                    foreach (string lib in libraryFiles)
                    {
                        libStatement.AppendFormat("\\\n\t{0}", lib.Replace('\\', '/'));
                    }
                    proFileWriter.WriteLine(libStatement.ToString());

                    foreach (string dependentLibrary in dependentLibraryFiles)
                    {
                        proFileWriter.WriteLine("{0}:PRE_TARGETDEPS += {1}", nodeData.Configuration, dependentLibrary.Replace('\\', '/'));
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