// <copyright file="CopyFiles.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(FileUtilities.CopyFiles copyFiles, out bool success)
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

            Opus.Core.Target target = copyFiles.OwningNode.Target;

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            if (copyFiles.Options is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = copyFiles.Options as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);

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

            string destinationDirectory = null;
            Opus.Core.IModule destinationModule = copyFiles.DestinationModule;
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
            string executablePath = tool.Executable(target);

            int returnValue = -1;
            foreach (string sourcePath in sourceFiles)
            {
                string destinationFile = System.IO.Path.Combine(destinationDirectory, System.IO.Path.GetFileName(sourcePath));

                bool requiresBuilding = NativeBuilder.RequiresBuilding(destinationFile, sourcePath);
                if (!requiresBuilding)
                {
                    Opus.Core.Log.DebugMessage("'{0}' is up-to-date", copyFiles.OwningNode.UniqueModuleName);
                    returnValue = 0;
                    continue;
                }

                Opus.Core.StringArray thisCommandLineBuilder = new Opus.Core.StringArray(commandLineBuilder);

                thisCommandLineBuilder.Add(System.String.Format("\"{0}\"", sourcePath));
                thisCommandLineBuilder.Add(System.String.Format("\"{0}\"", destinationDirectory));
                returnValue = CommandLineProcessor.Processor.Execute(copyFiles.OwningNode, tool, executablePath, thisCommandLineBuilder);
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
