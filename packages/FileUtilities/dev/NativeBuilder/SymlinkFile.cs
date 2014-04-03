// <copyright file="SymlinkFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(FileUtilities.SymlinkFile moduleToBuild, out bool success)
        {
            string sourceFilePath = moduleToBuild.SourceFileLocation.GetSinglePath();
            if (!System.IO.File.Exists(sourceFilePath))
            {
                throw new Opus.Core.Exception("Source file '{0}' does not exist", sourceFilePath);
            }

            Opus.Core.BaseOptionCollection baseOptions = moduleToBuild.Options;
            Opus.Core.DependencyNode node = moduleToBuild.OwningNode;

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

            var target = node.Target;

            var commandLineBuilder = new Opus.Core.StringArray();
            if (baseOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = baseOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);

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

            const string delimiter = "\"";
            if (target.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                commandLineBuilder.Add(System.String.Format("{0}{1}{2}", delimiter, baseOptions.OutputPaths[FileUtilities.OutputFileFlags.Symlink], delimiter));
                commandLineBuilder.Add(System.String.Format("{0}{1}{2}", delimiter, sourceFilePath, delimiter));
            }
            else
            {
                commandLineBuilder.Add(System.String.Format("{0}{1}{2}", delimiter, sourceFilePath, delimiter));
                commandLineBuilder.Add(System.String.Format("{0}{1}{2}", delimiter, baseOptions.OutputPaths[FileUtilities.OutputFileFlags.Symlink], delimiter));
            }

            Opus.Core.ITool tool = target.Toolset.Tool(typeof(FileUtilities.ISymlinkTool));
            int returnValue = CommandLineProcessor.Processor.Execute(node, tool, commandLineBuilder);
            success = (0 == returnValue);

            return null;
        }
    }
}
