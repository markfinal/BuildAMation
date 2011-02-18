// <copyright file="CDynamicLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(C.DynamicLibrary dynamicLibrary, Opus.Core.DependencyNode node, out bool success)
        {
            Opus.Core.Target target = node.Target;
            C.Linker linkerInstance = C.LinkerFactory.GetTargetInstance(target);
            Opus.Core.ITool linkerTool = linkerInstance as Opus.Core.ITool;
            C.ILinkerOptions linkerOptions = dynamicLibrary.Options as C.ILinkerOptions;

            // find dependent object files
            Opus.Core.StringArray dependentObjectFiles = new Opus.Core.StringArray();
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
                throw new Opus.Core.Exception("There are no object files to link");
            }

            // find dependent library files
            Opus.Core.StringArray dependentLibraryFiles = null;
            if (null != node.ExternalDependents)
            {
                dependentLibraryFiles = new Opus.Core.StringArray();
                node.ExternalDependents.FilterOutputPaths(C.OutputFileFlags.StaticLibrary | C.OutputFileFlags.StaticImportLibrary, dependentLibraryFiles);
                linkerOptions.Libraries.AddRange(dependentLibraryFiles);
            }

            Opus.Core.StringArray inputFiles = dependentObjectFiles;
            if (null != dependentLibraryFiles)
            {
                inputFiles.AddRange(dependentLibraryFiles);
            }
            Opus.Core.StringArray outputFiles = dynamicLibrary.Options.OutputPaths.Paths;
            if (!RequiresBuilding(outputFiles, inputFiles))
            {
                Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                success = true;
                return null;
            }

            System.Text.StringBuilder commandLineBuilder = new System.Text.StringBuilder();
            if (linkerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = linkerOptions as CommandLineProcessor.ICommandLineSupport;
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

            string executablePath;
            C.IToolchainOptions toolchainOptions = (dynamicLibrary.Options as C.ILinkerOptions).ToolchainOptionCollection as C.IToolchainOptions;
            if (toolchainOptions.IsCPlusPlus)
            {
                executablePath = linkerInstance.ExecutableCPlusPlus(target);
            }
            else
            {
                executablePath = linkerTool.Executable(target);
            }

            // object files must come before everything else, for some compilers
            commandLineBuilder.Insert(0, dependentObjectFiles.ToString(' '));

            int exitCode = CommandLineProcessor.Processor.Execute(node, linkerTool, executablePath, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}