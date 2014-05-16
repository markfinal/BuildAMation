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
            var sourceLocation = moduleToBuild.SourceFileLocation;
            var sourceFilePath = sourceLocation.GetSinglePath();
            if (!System.IO.File.Exists(sourceFilePath))
            {
                throw new Opus.Core.Exception("Source file '{0}' does not exist", sourceFilePath);
            }

            var baseOptions = moduleToBuild.Options;
            var node = moduleToBuild.OwningNode;

            // dependency checking

            {
#if true
                var inputLocations = new Opus.Core.LocationArray(
                    sourceLocation
                    );
                var outputLocations = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.File, Opus.Core.Location.EExists.WillExist);
                if (!RequiresBuilding(outputLocations, inputLocations))
#else
                Opus.Core.StringArray inputFiles = new Opus.Core.StringArray(
                    sourceFilePath
                );
                Opus.Core.StringArray outputFiles = baseOptions.OutputPaths.Paths;
                if (!RequiresBuilding(outputFiles, inputFiles))
#endif
                {
                    Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }
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

            const string delimiter = "\"";
            if (target.HasPlatform(Opus.Core.EPlatform.Windows))
            {
#if true
                var outputPath = moduleToBuild.Locations[FileUtilities.SymlinkFile.OutputFile].GetSinglePath();
                commandLineBuilder.Add(System.String.Format("{0}{1}{2}", delimiter, outputPath, delimiter));
#else
                commandLineBuilder.Add(System.String.Format("{0}{1}{2}", delimiter, baseOptions.OutputPaths[FileUtilities.OutputFileFlags.Symlink], delimiter));
#endif
                commandLineBuilder.Add(System.String.Format("{0}{1}{2}", delimiter, sourceFilePath, delimiter));
            }
            else
            {
                commandLineBuilder.Add(System.String.Format("{0}{1}{2}", delimiter, sourceFilePath, delimiter));
#if true
                var outputPath = moduleToBuild.Locations[FileUtilities.SymlinkFile.OutputFile].GetSinglePath();
                commandLineBuilder.Add(System.String.Format("{0}{1}{2}", delimiter, outputPath, delimiter));
#else
                commandLineBuilder.Add(System.String.Format("{0}{1}{2}", delimiter, baseOptions.OutputPaths[FileUtilities.OutputFileFlags.Symlink], delimiter));
#endif
            }

            Opus.Core.ITool tool = target.Toolset.Tool(typeof(FileUtilities.ISymlinkTool));
            int returnValue = CommandLineProcessor.Processor.Execute(node, tool, commandLineBuilder);
            success = (0 == returnValue);

            return null;
        }
    }
}
