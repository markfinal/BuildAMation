// <copyright file="CStaticLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(C.StaticLibrary staticLibrary, Opus.Core.DependencyNode node, out bool success)
        {
            Opus.Core.Target target = node.Target;
            C.Archiver archiverInstance = C.ArchiverFactory.GetTargetInstance(target);
            Opus.Core.ITool archiverTool = archiverInstance as Opus.Core.ITool;

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
                throw new Opus.Core.Exception("There are no object files to archive");
            }

            Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
            inputFiles.AddRange(dependentObjectFiles);
            Opus.Core.StringArray outputFiles = staticLibrary.Options.OutputPaths.Paths;
            if (!RequiresBuilding(outputFiles, inputFiles))
            {
                Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                success = true;
                return null;
            }

            System.Text.StringBuilder commandLineBuilder = new System.Text.StringBuilder();
            if (staticLibrary.Options is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = staticLibrary.Options as CommandLineProcessor.ICommandLineSupport;
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

            string executablePath = archiverTool.Executable(target);

            commandLineBuilder.Append(dependentObjectFiles.ToString(' '));

            int exitCode = CommandLineProcessor.Processor.Execute(node, archiverTool, executablePath, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}