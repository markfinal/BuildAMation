// <copyright file="CDynamicLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(C.DynamicLibrary moduleToBuild, out bool success)
        {
            var dynamicLibraryModule = moduleToBuild as Opus.Core.BaseModule;
            var node = dynamicLibraryModule.OwningNode;
            var target = node.Target;
            var dynamicLibraryOptions = dynamicLibraryModule.Options;
            var linkerOptions = dynamicLibraryOptions as C.ILinkerOptions;

            var objectFileFlags = C.OutputFileFlags.ObjectFile;
            if (target.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                objectFileFlags |= C.OutputFileFlags.Win32CompiledResource;
            }

            // find dependent object files
            var dependentObjectFiles = new Opus.Core.StringArray();
            if (null != node.Children)
            {
                node.Children.FilterOutputPaths(objectFileFlags, dependentObjectFiles);
            }
            if (null != node.ExternalDependents)
            {
                node.ExternalDependents.FilterOutputPaths(objectFileFlags, dependentObjectFiles);
            }
            if (0 == dependentObjectFiles.Count)
            {
                Opus.Core.Log.Detail("There were no object files to link for module '{0}'", node.UniqueModuleName);
                success = true;
                return null;
            }

            // find dependent library files
            Opus.Core.StringArray dependentLibraryFiles = null;
            if (null != node.ExternalDependents)
            {
                dependentLibraryFiles = new Opus.Core.StringArray();
                node.ExternalDependents.FilterOutputPaths(C.OutputFileFlags.StaticLibrary | C.OutputFileFlags.StaticImportLibrary, dependentLibraryFiles);
            }

            // dependency checking
            {
                var inputFiles = new Opus.Core.StringArray();
                inputFiles.AddRange(dependentObjectFiles);
                if (null != dependentLibraryFiles)
                {
                    inputFiles.AddRange(dependentLibraryFiles);
                }

                // don't dependency check against the static import library, since it is generally not rewritten
                // when code changes
                // note that a copy is taken here as we do not want to remove the static import library from the original outputs
                var filteredOutputPaths = new Opus.Core.OutputPaths(dynamicLibraryOptions.OutputPaths);
                filteredOutputPaths.Remove(C.OutputFileFlags.StaticImportLibrary);

                var outputFiles = filteredOutputPaths.Paths;
                if (!RequiresBuilding(outputFiles, inputFiles))
                {
                    Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            var commandLineBuilder = new Opus.Core.StringArray();
            if (linkerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = linkerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);

                var directoriesToCreate = commandLineOption.DirectoriesToCreate();
                foreach (string directoryPath in directoriesToCreate)
                {
                    NativeBuilder.MakeDirectory(directoryPath);
                }
            }
            else
            {
                throw new Opus.Core.Exception("Linker options does not support command line translation");
            }

            // object files must come before everything else, for some compilers
            commandLineBuilder.Insert(0, dependentObjectFiles.ToString(' '));

            // then libraries
            var linkerTool = target.Toolset.Tool(typeof(C.ILinkerTool)) as C.ILinkerTool;
            C.LinkerUtilities.AppendLibrariesToCommandLine(commandLineBuilder, linkerTool, linkerOptions, dependentLibraryFiles);

            int exitCode = CommandLineProcessor.Processor.Execute(node, linkerTool, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}