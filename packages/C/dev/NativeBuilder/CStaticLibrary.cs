// <copyright file="CStaticLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(C.StaticLibrary staticLibrary, out bool success)
        {
            Opus.Core.BaseModule staticLibraryModule = staticLibrary as Opus.Core.BaseModule;
            Opus.Core.DependencyNode node = staticLibraryModule.OwningNode;
            Opus.Core.Target target = node.Target;

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
                Opus.Core.Log.Detail("There were no object files to archive for module '{0}'", node.UniqueModuleName);
                success = true;
                return null;
            }

            Opus.Core.BaseOptionCollection staticLibraryOptions = staticLibraryModule.Options;

            // dependency checking
            {
                Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
                inputFiles.AddRange(dependentObjectFiles);
                Opus.Core.StringArray outputFiles = staticLibraryOptions.OutputPaths.Paths;
                if (!RequiresBuilding(outputFiles, inputFiles))
                {
                    Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            if (staticLibraryOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = staticLibraryOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);

                Opus.Core.DirectoryCollection directoriesToCreate = commandLineOption.DirectoriesToCreate();
                foreach (string directoryPath in directoriesToCreate)
                {
                    NativeBuilder.MakeDirectory(directoryPath);
                }
            }
            else
            {
                throw new Opus.Core.Exception("Archiver options does not support command line translation");
            }

            foreach (string dependentObjectFile in dependentObjectFiles)
            {
                commandLineBuilder.Add(dependentObjectFile);
            }

            Opus.Core.ITool archiverTool = target.Toolset.Tool(typeof(C.IArchiverTool));
            int exitCode = CommandLineProcessor.Processor.Execute(node, archiverTool, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}