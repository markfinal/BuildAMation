// <copyright file="SymlinkDirectory.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(FileUtilities.SymlinkDirectory moduleToBuild, out bool success)
        {
            string sourceFilePath = moduleToBuild.SourceFileLocation.GetSinglePath();
            if (!System.IO.Directory.Exists(sourceFilePath))
            {
                throw new Opus.Core.Exception("Source directory '{0}' does not exist", sourceFilePath);
            }

            Opus.Core.BaseOptionCollection baseOptions = moduleToBuild.Options;
            Opus.Core.DependencyNode node = moduleToBuild.OwningNode;

            // dependency checking
#if true
            if (DirectoryUpToDate(moduleToBuild.Locations[FileUtilities.SymlinkFile.OutputFile], sourceFilePath))
#else
            if (DirectoryUpToDate(baseOptions.OutputPaths[FileUtilities.OutputFileFlags.Symlink], sourceFilePath))
#endif
            {
                Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                success = true;
                return null;
            }

            // at this point, we know the node outputs need building

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var target = node.Target;

            var commandLineBuilder = new Opus.Core.StringArray();
            if (baseOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = baseOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
            }

            if (target.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                commandLineBuilder.Add("/D");
#if true
                var outputPath = moduleToBuild.Locations[FileUtilities.SymlinkFile.OutputFile].GetSinglePath();
                commandLineBuilder.Add(outputPath);
#else
                commandLineBuilder.Add(baseOptions.OutputPaths[FileUtilities.OutputFileFlags.Symlink]);
#endif
                commandLineBuilder.Add(sourceFilePath);
            }
            else
            {
                commandLineBuilder.Add(sourceFilePath);
#if true
                var outputPath = moduleToBuild.Locations[FileUtilities.SymlinkFile.OutputFile].GetSinglePath();
                commandLineBuilder.Add(outputPath);
#else
                commandLineBuilder.Add(baseOptions.OutputPaths[FileUtilities.OutputFileFlags.Symlink]);
#endif
            }

            Opus.Core.ITool tool = target.Toolset.Tool(typeof(FileUtilities.ISymlinkTool));
            int returnValue = CommandLineProcessor.Processor.Execute(node, tool, commandLineBuilder);
            success = (0 == returnValue);

            return null;
        }
    }
}
