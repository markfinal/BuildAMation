// <copyright file="CApplication.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QtCreatorBuilder
{
    public sealed partial class QtCreatorBuilder
    {
        public object Build(C.Application application, Opus.Core.DependencyNode node, out bool success)
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
            C.IToolchainOptions toolchainOptions = (application.Options as C.ILinkerOptions).ToolchainOptionCollection as C.IToolchainOptions;
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
#if true
                proFileWriter.WriteLine("--- Written by Opus");

                {
                    string relativePriPathName = Opus.Core.RelativePathUtilities.GetPath(this.DisableQtPriPathName, proFilePath);
                    proFileWriter.WriteLine("include({0})", relativePriPathName);
                }
#else
                proFileWriter.WriteLine("# -------------------------------------------------------------------------------------");
                proFileWriter.WriteLine("# Disable a load of Qt stuff");
                proFileWriter.WriteLine("QT -= core gui");
                proFileWriter.WriteLine("QMAKE_LIBS_QT_ENTRY=");
                proFileWriter.WriteLine("QMAKE_CFLAGS=");
                proFileWriter.WriteLine("QMAKE_CFLAGS_WARN_ON=");
                proFileWriter.WriteLine("QMAKE_CFLAGS_DEBUG=");
                proFileWriter.WriteLine("QMAKE_CFLAGS_MT_DBG=");
                proFileWriter.WriteLine("QMAKE_CFLAGS_MT_DLLDBG=");
                proFileWriter.WriteLine("QMAKE_CFLAGS_THREAD=");
                proFileWriter.WriteLine("QMAKE_CXXFLAGS=");
                proFileWriter.WriteLine("QMAKE_CXXFLAGS_DEBUG=");
                proFileWriter.WriteLine("QMAKE_CXXFLAGS_MT_DLLDBG=");
                proFileWriter.WriteLine("QMAKE_CXXFLAGS_MT_DLLDBG=");
                proFileWriter.WriteLine("QMAKE_INCDIR=");
                proFileWriter.WriteLine("QMAKE_INCDIR_QT=");
                //proFileWriter.WriteLine("QMAKESPEC=");
                proFileWriter.WriteLine("DEFINES=");
                proFileWriter.WriteLine("INCLUDEPATH=");
                proFileWriter.WriteLine("mmx:DEFINES=");
                proFileWriter.WriteLine("3dnow:DEFINES=");
                proFileWriter.WriteLine("sse:DEFINES=");
                proFileWriter.WriteLine("sse2:DEFINES=");
                proFileWriter.WriteLine("sse3:DEFINES=");
                proFileWriter.WriteLine("ssse3:DEFINES=");
                proFileWriter.WriteLine("sse4_1:DEFINES=");
                proFileWriter.WriteLine("sse4_2:DEFINES=");
                proFileWriter.WriteLine("avx:DEFINES=");
                proFileWriter.WriteLine("iwmmxt:DEFINES=");
                proFileWriter.WriteLine("QMAKE_PRL_DEFINES=");
                proFileWriter.WriteLine("QMAKE_LFLAGS=");
                proFileWriter.WriteLine("QMAKE_LFLAGS_DEBUG=");
                proFileWriter.WriteLine("QMAKE_INCDIR=");
                proFileWriter.WriteLine("QMAKE_INCDIR_QT=");
                proFileWriter.WriteLine("QMAKE_LIBDIR_QT=");
                proFileWriter.WriteLine("#QMAKE_RUN_CC=$(CC) -c -Fo$obj $src");
                proFileWriter.WriteLine("#QMAKE_RUN_CC_IMP=$(CC) -c -Fo$@ $<");
                proFileWriter.WriteLine("#QMAKE_RUN_CC_IMP_BATCH=$(CC) -c -Fo$@ @<<");
                proFileWriter.WriteLine("#QMAKE_RUN_CXX=$(CXX) -c -Fo$obj $src");
                proFileWriter.WriteLine("#QMAKE_RUN_CXX_IMP=$(CXX) -c -Fo$@ $<");
                proFileWriter.WriteLine("#QMAKE_RUN_CXX_IMP_BATCH=$(CXX) -c -Fo$@ @<<");
                proFileWriter.WriteLine("QMAKE_LFLAGS_CONSOLE=");
                proFileWriter.WriteLine("QMAKE_LFLAGS_WINDOWS=");
                proFileWriter.WriteLine("# -------------------------------------------------------------------------------------");
#endif
                proFileWriter.WriteLine("TARGET = {0}", application.OwningNode.ModuleName);
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

                // binary file directory
                proFileWriter.WriteLine("{0}:DESTDIR = {1}", nodeData.Configuration, linkerOptionCollection.OutputDirectoryPath.Replace('\\', '/'));
            }

            success = true;
            return nodeData;
        }
    }
}