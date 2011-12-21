// <copyright file="CObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(C.ObjectFile objectFile, out bool success)
        {
            string sourceFilePath = objectFile.SourceFile.AbsolutePath;
            if (!System.IO.File.Exists(sourceFilePath))
            {
                throw new Opus.Core.Exception(System.String.Format("Source file '{0}' does not exist", sourceFilePath));
            }

            C.CompilerOptionCollection compilerOptions = objectFile.Options as C.CompilerOptionCollection;

            Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
            inputFiles.Add(sourceFilePath);
            Opus.Core.StringArray outputFiles = compilerOptions.OutputPaths.Paths;
            if (!RequiresBuilding(outputFiles, inputFiles))
            {
                Opus.Core.Log.DebugMessage("'{0}' is up-to-date", objectFile.OwningNode.UniqueModuleName);
                success = true;
                return null;
            }

            Opus.Core.Target target = objectFile.OwningNode.Target;
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

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
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

            if (sourceFilePath.Contains(" "))
            {
                commandLineBuilder.Add(System.String.Format("\"{0}\"", sourceFilePath));
            }
            else
            {
                commandLineBuilder.Add(sourceFilePath);
            }

            int exitCode = CommandLineProcessor.Processor.Execute(objectFile.OwningNode, compilerTool, executablePath, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}