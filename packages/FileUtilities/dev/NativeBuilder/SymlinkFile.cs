// <copyright file="SymlinkFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(FileUtilities.SymlinkFile symlinkFile, out bool success)
        {
            string sourceFilePath = symlinkFile.SourceFile.AbsolutePath;
            if (!System.IO.File.Exists(sourceFilePath))
            {
                throw new Opus.Core.Exception("Source file '{0}' does not exist", sourceFilePath);
            }

            Opus.Core.BaseOptionCollection baseOptions = symlinkFile.Options;
            Opus.Core.DependencyNode node = symlinkFile.OwningNode;

            // dependency checking
            {
                Opus.Core.StringArray inputFiles = new Opus.Core.StringArray(
                    sourceFilePath
                );
                Opus.Core.StringArray outputFiles = baseOptions.OutputPaths.Paths;
                if (!RequiresBuilding(outputFiles, inputFiles))
                {
                    Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            Opus.Core.Target target = node.Target;

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            if (baseOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = baseOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);

                Opus.Core.DirectoryCollection directoriesToCreate = commandLineOption.DirectoriesToCreate();
                foreach (string directoryPath in directoriesToCreate)
                {
                    NativeBuilder.MakeDirectory(directoryPath);
                }
            }
            else
            {
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
            }

            if (target.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                commandLineBuilder.Add(baseOptions.OutputPaths[FileUtilities.OutputFileFlags.Symlink][0]);
                commandLineBuilder.Add(sourceFilePath);
            }
            else
            {
                commandLineBuilder.Add(sourceFilePath);
                commandLineBuilder.Add(baseOptions.OutputPaths[FileUtilities.OutputFileFlags.Symlink][0]);
            }

            Opus.Core.ITool tool = target.Toolset.Tool(typeof(FileUtilities.ISymlinkTool));
            int returnValue = CommandLineProcessor.Processor.Execute(node, tool, commandLineBuilder);
            success = (0 == returnValue);

            return null;
        }
    }
}
