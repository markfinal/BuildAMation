// <copyright file="Win32Resource.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(C.Win32Resource resourceFile, out bool success)
        {
            string resourceFilePath = resourceFile.ResourceFile.AbsolutePath;
            if (!System.IO.File.Exists(resourceFilePath))
            {
                throw new Opus.Core.Exception(System.String.Format("Resource file '{0}' does not exist", resourceFilePath));
            }

            Opus.Core.IModule resourceFileModule = resourceFile as Opus.Core.IModule;
            Opus.Core.BaseOptionCollection resourceFileOptions = resourceFileModule.Options;

            C.Win32ResourceCompilerOptionCollection compilerOptions = resourceFileOptions as C.Win32ResourceCompilerOptionCollection;

            // dependency checking, source against output files
            {
                Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
                inputFiles.Add(resourceFilePath);
                Opus.Core.StringArray outputFiles = compilerOptions.OutputPaths.Paths;
                if (!RequiresBuilding(outputFiles, inputFiles))
                {
                    Opus.Core.Log.DebugMessage("'{0}' is up-to-date", resourceFileModule.OwningNode.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            Opus.Core.Target target = resourceFileModule.OwningNode.Target;

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

            C.IWinResourceCompilerTool compilerTool = target.Toolset.Tool(typeof(C.IWinResourceCompilerTool)) as C.IWinResourceCompilerTool;

            // add output path
            commandLineBuilder.Add(System.String.Format("{0}{1}", compilerTool.OutputFileSwitch, compilerOptions.CompiledResourceFilePath));

            if (resourceFilePath.Contains(" "))
            {
                commandLineBuilder.Add(System.String.Format("{0}\"{1}\"", compilerTool.InputFileSwitch, resourceFilePath));
            }
            else
            {
                commandLineBuilder.Add(System.String.Format("{0}{1}", compilerTool.InputFileSwitch, resourceFilePath));
            }

            int exitCode = CommandLineProcessor.Processor.Execute(resourceFileModule.OwningNode, compilerTool, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}