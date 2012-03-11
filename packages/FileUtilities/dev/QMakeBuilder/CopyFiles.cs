// <copyright file="CopyFiles.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object Build(FileUtilities.CopyFiles copyFiles, out bool success)
        {
            System.Enum sourceOutputPaths = copyFiles.SourceOutputFlags;
            System.Collections.Generic.List<NodeData> sourceFileDataArray = new System.Collections.Generic.List<NodeData>();
            Opus.Core.StringArray sourceFiles = null;
            foreach (Opus.Core.IModule sourceModule in copyFiles.SourceModules)
            {
                if (sourceModule.Options.OutputPaths.Has(sourceOutputPaths))
                {
                    if (sourceModule.OwningNode.Data != null)
                    {
                        NodeData data = sourceModule.OwningNode.Data as NodeData;
                        sourceFileDataArray.Add(data);
                    }
                }
            }
            if (null != copyFiles.SourceFiles)
            {
                sourceFiles = new Opus.Core.StringArray();
                foreach (string path in copyFiles.SourceFiles)
                {
                    sourceFiles.Add(path);
                }
            }
            if (0 == sourceFileDataArray.Count && null == sourceFiles)
            {
                Opus.Core.Log.DebugMessage("No files to copy");
                success = true;
                return null;
            }

            Opus.Core.IModule destinationModule = copyFiles.DestinationModule;
            string destinationDirectory = null;
            System.Enum destinationOutputFlags;
            if (null != destinationModule)
            {
                Opus.Core.StringArray destinationPaths = new Opus.Core.StringArray();
                destinationModule.Options.FilterOutputPaths(copyFiles.DirectoryOutputFlags, destinationPaths);
                destinationOutputFlags = copyFiles.DirectoryOutputFlags;
                destinationDirectory = System.IO.Path.GetDirectoryName(destinationPaths[0]);
            }
            else
            {
                destinationOutputFlags = sourceOutputPaths;
                destinationDirectory = copyFiles.DestinationDirectory;
            }

            Opus.Core.IModule copyFilesModule = copyFiles as Opus.Core.IModule;
            Opus.Core.Target target = copyFilesModule.OwningNode.Target;

            FileUtilities.CopyFilesTool tool = new FileUtilities.CopyFilesTool();
            string toolExecutablePath = tool.Executable(target);

            Opus.Core.BaseOptionCollection copyFilesOptions = copyFilesModule.Options;

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            if (copyFilesOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = copyFilesOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);
            }
            else
            {
                throw new Opus.Core.Exception("Linker options does not support command line translation");
            }

            foreach (NodeData sourceNode in sourceFileDataArray)
            {
                string proFilePathName = sourceNode.ProFilePathName;
                using (System.IO.TextWriter proFileWriter = new System.IO.StreamWriter(proFilePathName, true))
                {
                    proFileWriter.WriteLine("# Write '{0}' to '{1}'", "$(DESTDIR_TARGET)", destinationDirectory);
                    if (sourceNode.HasPostLinks)
                    {
                        proFileWriter.WriteLine("QMAKE_POST_LINK += &&");
                    }
                    proFileWriter.WriteLine("win32:QMAKE_POST_LINK += {0} {1} {2} {3}", toolExecutablePath, commandLineBuilder.ToString(' '), "$(DESTDIR_TARGET)", destinationDirectory);
                    proFileWriter.WriteLine("!win32:QMAKE_POST_LINK += {0} {1} {2} {3}", toolExecutablePath, commandLineBuilder.ToString(' '), "$$DESTDIR/$(TARGET0)", destinationDirectory);
                    sourceNode.HasPostLinks = true;
                }
            }

            success = true;
            return null;
        }
    }
}
