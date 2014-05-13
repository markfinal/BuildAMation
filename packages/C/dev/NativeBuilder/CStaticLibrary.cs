// <copyright file="CStaticLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(C.StaticLibrary moduleToBuild, out bool success)
        {
            var staticLibraryModule = moduleToBuild as Opus.Core.BaseModule;
            var node = staticLibraryModule.OwningNode;
            var target = node.Target;

            // find dependent object files
            var keysToFilter = new Opus.Core.Array<Opus.Core.LocationKey>(
                C.ObjectFile.ObjectFileLocationKey
                );
            var dependentObjectFiles = new Opus.Core.LocationArray();
            if (null != node.Children)
            {
                node.Children.FilterOutputLocations(keysToFilter, dependentObjectFiles);
            }
            if (null != node.ExternalDependents)
            {
                node.ExternalDependents.FilterOutputLocations(keysToFilter, dependentObjectFiles);
            }
            if (0 == dependentObjectFiles.Count)
            {
                Opus.Core.Log.Detail("There were no object files to archive for module '{0}'", node.UniqueModuleName);
                success = true;
                return null;
            }

            var staticLibraryOptions = staticLibraryModule.Options;

            // dependency checking
            {
                var inputFiles = new Opus.Core.LocationArray();
                inputFiles.AddRange(dependentObjectFiles);
                var outputFileLKeys = new Opus.Core.Array<Opus.Core.LocationKey>(
                    C.StaticLibrary.OutputFileLocKey
                    );
                var outputFiles = moduleToBuild.Locations.FilterByKey(outputFileLKeys);
                if (!RequiresBuilding(outputFiles, inputFiles))
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

            var commandLineBuilder = new Opus.Core.StringArray();
            if (staticLibraryOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = staticLibraryOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Opus.Core.Exception("Archiver options does not support command line translation");
            }

            foreach (var dependentObjectFile in dependentObjectFiles)
            {
                commandLineBuilder.Add(dependentObjectFile.GetSinglePath());
            }

            var archiverTool = target.Toolset.Tool(typeof(C.IArchiverTool));
            var exitCode = CommandLineProcessor.Processor.Execute(node, archiverTool, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}