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
            foreach (Opus.Core.DependencyNode childNode in node.Children)
            {
                NodeData childData = childNode.Data as NodeData;
                nodeData.Merge(childData);
            }

            C.LinkerOptionCollection linkerOptionCollection = node.Module.Options as C.LinkerOptionCollection;
            C.ILinkerOptions linkerOptions = node.Module.Options as C.ILinkerOptions;
            C.IToolchainOptions toolchainOptions = (application.Options as C.ILinkerOptions).ToolchainOptionCollection as C.IToolchainOptions;

            string proFilePath = QtCreatorBuilder.GetProFilePath(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(proFilePath));

            using (System.IO.TextWriter proFileWriter = new System.IO.StreamWriter(proFilePath))
            {
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
                proFileWriter.WriteLine("QMAKESPEC=");
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
                proFileWriter.WriteLine("TARGET = {0}", application.OwningNode.ModuleName);
                proFileWriter.WriteLine("TEMPLATE = app");
                proFileWriter.WriteLine("CONFIG += debug");
                proFileWriter.WriteLine("CONFIG += console");

                // sources
                {
                    Opus.Core.StringArray sourcesArray = nodeData["SOURCES"];
                    System.Text.StringBuilder sourcesStatement = new System.Text.StringBuilder();
                    sourcesStatement.Append("SOURCES += ");
                    foreach (string source in sourcesArray)
                    {
                        sourcesStatement.AppendFormat("\\\n\t{0}", source.Replace('\\', '/'));
                    }
                    proFileWriter.WriteLine(sourcesStatement.ToString());
                }

                // cflags
                {
                    Opus.Core.StringArray cflags = nodeData["CFLAGS"];
                    // TODO: need to replace "<X>" with $$quote(<X>)
                    proFileWriter.WriteLine("QMAKE_CFLAGS += {0}", cflags.ToString());
                }

                // object file directory
                proFileWriter.WriteLine("OBJECTS_DIR = {0}", nodeData["OBJECTS_DIR"].ToString().Replace('\\', '/'));

                // binary file directory
                proFileWriter.WriteLine("DESTDIR = {0}", linkerOptionCollection.OutputDirectoryPath.Replace('\\', '/'));
            }

            success = true;
            return null;
        }
    }
}