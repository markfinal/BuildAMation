// <copyright file="CopyFiles.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(FileUtilities.CopyFiles copyFiles, Opus.Core.DependencyNode node, out bool success)
        {
            System.Enum sourceOutputPaths = copyFiles.SourceOutputFlags;
            Opus.Core.StringArray sourceFiles = new Opus.Core.StringArray();
            foreach (Opus.Core.IModule sourceModule in copyFiles.SourceModules)
            {
                sourceModule.Options.FilterOutputPaths(sourceOutputPaths, sourceFiles);
            }
            if (null != copyFiles.SourceFiles)
            {
                foreach (string path in copyFiles.SourceFiles)
                {
                    sourceFiles.Add(path);
                }
            }
            if (0 == sourceFiles.Count)
            {
                Opus.Core.Log.DebugMessage("No files to copy");
                success = true;
                return null;
            }

            if (copyFiles.Options is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = copyFiles.Options as CommandLineProcessor.ICommandLineSupport;

                Opus.Core.DirectoryCollection directoriesToCreate = commandLineOption.DirectoriesToCreate();
                foreach (string directoryPath in directoriesToCreate)
                {
                    NativeBuilder.MakeDirectory(directoryPath);
                }
            }
            else
            {
                throw new Opus.Core.Exception("Linker options does not support command line translation");
            }

            Opus.Core.IModule destinationModule = copyFiles.DestinationModule;
            string destinationDirectory = null;
            if (null != destinationModule)
            {
                Opus.Core.StringArray destinationPaths = new Opus.Core.StringArray();
                destinationModule.Options.FilterOutputPaths(copyFiles.DirectoryOutputFlags, destinationPaths);
                destinationDirectory = System.IO.Path.GetDirectoryName(destinationPaths[0]);
            }
            else
            {
                destinationDirectory = copyFiles.DestinationDirectory;
            }

            FileUtilities.CopyFilesTool tool = new FileUtilities.CopyFilesTool();
            string executablePath = tool.Executable(node.Target);

            int returnValue = -1;
            foreach (string sourcePath in sourceFiles)
            {
                string destinationFile = System.IO.Path.Combine(destinationDirectory, System.IO.Path.GetFileName(sourcePath));

                bool requiresBuilding = NativeBuilder.RequiresBuilding(destinationFile, sourcePath);
                if (!requiresBuilding)
                {
                    Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    returnValue = 0;
                    continue;
                }

                System.Text.StringBuilder commandLineBuilder = new System.Text.StringBuilder();
                if (Opus.Core.OSUtilities.IsWindowsHosting)
                {
                    commandLineBuilder.Append("/c COPY ");
                }
                commandLineBuilder.AppendFormat("\"{0}\" \"{1}\"", sourcePath, destinationDirectory);

                returnValue = CommandLineProcessor.Processor.Execute(node, tool, executablePath, commandLineBuilder);
                if (0 != returnValue)
                {
                    break;
                }
            }

            success = (0 == returnValue);
            return null;
        }
    }
}
