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
            var dependentObjectFiles = new Opus.Core.StringArray();
            if (null != node.Children)
            {
                node.Children.FilterOutputPaths(C.OutputFileFlags.ObjectFile, dependentObjectFiles);
            }
            if (null != node.ExternalDependents)
            {
                node.ExternalDependents.FilterOutputPaths(C.OutputFileFlags.ObjectFile, dependentObjectFiles);
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
                var inputFiles = new Opus.Core.StringArray();
                inputFiles.AddRange(dependentObjectFiles);
                var outputFiles = staticLibraryOptions.OutputPaths.Paths;
                if (!RequiresBuilding(outputFiles, inputFiles))
                {
                    Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            var commandLineBuilder = new Opus.Core.StringArray();
            if (staticLibraryOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = staticLibraryOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);

                var directoriesToCreate = commandLineOption.DirectoriesToCreate();
                foreach (string directoryPath in directoriesToCreate)
                {
                    NativeBuilder.MakeDirectory(directoryPath);
                }
            }
            else
            {
                throw new Opus.Core.Exception("Archiver options does not support command line translation");
            }

            foreach (var dependentObjectFile in dependentObjectFiles)
            {
                commandLineBuilder.Add(dependentObjectFile);
            }

            var archiverTool = target.Toolset.Tool(typeof(C.IArchiverTool));
            int exitCode = CommandLineProcessor.Processor.Execute(node, archiverTool, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}