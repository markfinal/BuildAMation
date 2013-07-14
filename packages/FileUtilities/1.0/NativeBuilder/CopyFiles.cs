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

            Opus.Core.BaseModule copyFilesModule = copyFiles as Opus.Core.BaseModule;
            Opus.Core.DependencyNode node = copyFilesModule.OwningNode;
            Opus.Core.Target target = node.Target;

            Opus.Core.BaseOptionCollection copyFilesOptions = copyFilesModule.Options;

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            if (copyFilesOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = copyFilesOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);

                Opus.Core.DirectoryCollection directoriesToCreate = commandLineOption.DirectoriesToCreate();
                foreach (string directoryPath in directoriesToCreate)
                {
                    NativeBuilder.MakeDirectory(directoryPath);
                }
            }
            else
            {
                throw new Opus.Core.Exception("CopyFiles options does not support command line translation");
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

            Opus.Core.ITool tool = target.Toolset.Tool(typeof(FileUtilities.ICopyFilesTool));

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

                Opus.Core.StringArray thisCommandLineBuilder = new Opus.Core.StringArray(commandLineBuilder);

                if (sourcePath.Contains(" "))
                {
                    thisCommandLineBuilder.Add(System.String.Format("\"{0}\"", sourcePath));
                }
                else
                {
                    thisCommandLineBuilder.Add(sourcePath);
                }
                if (destinationDirectory.Contains(" "))
                {
                    thisCommandLineBuilder.Add(System.String.Format("\"{0}\"", destinationDirectory));
                }
                else
                {
                    thisCommandLineBuilder.Add(destinationDirectory);
                }
                returnValue = CommandLineProcessor.Processor.Execute(node, tool, thisCommandLineBuilder);
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
