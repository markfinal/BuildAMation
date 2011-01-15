// <copyright file="CObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(C.ObjectFile objectFile, Opus.Core.DependencyNode node, out bool success)
        {
            string sourceFilePath = System.IO.Path.Combine(node.Package.Directory, objectFile.SourceFile.RelativePath);
            if (!System.IO.File.Exists(sourceFilePath))
            {
                throw new Opus.Core.Exception(System.String.Format("Source file '{0}' does not exist", sourceFilePath));
            }

            C.CompilerOptionCollection compilerOptions = objectFile.Options as C.CompilerOptionCollection;
            Opus.Core.IOutputPaths outputPaths = objectFile.Options as Opus.Core.IOutputPaths;

            Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
            inputFiles.Add(sourceFilePath);
            Opus.Core.StringArray outputFiles = new Opus.Core.StringArray(outputPaths.GetOutputPaths().Values);
            if (!RequiresBuilding(outputFiles, inputFiles))
            {
                Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                success = true;
                return null;
            }

            Opus.Core.Target target = node.Target;
            C.Compiler compilerInstance = C.CompilerFactory.GetTargetInstance(target, C.ClassNames.CCompilerTool);
            Opus.Core.ITool compilerTool = compilerInstance as Opus.Core.ITool;

            string executablePath;
            C.IToolchainOptions toolchainOptions = (objectFile.Options as C.ICCompilerOptions).ToolchainOptionCollection as C.IToolchainOptions;
            if (toolchainOptions.IsCPlusPlus)
            {
                executablePath = compilerInstance.ExecutableCPlusPlus(target);
            }
            else
            {
                executablePath = compilerTool.Executable(target);
            }

            System.Text.StringBuilder commandLineBuilder = new System.Text.StringBuilder();
            if (compilerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = compilerOptions as CommandLineProcessor.ICommandLineSupport;
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

            commandLineBuilder.Append(System.String.Format("\"{0}\"", sourceFilePath));

            int exitCode = CommandLineProcessor.Processor.Execute(node, compilerTool, executablePath, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}