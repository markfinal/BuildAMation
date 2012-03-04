// <copyright file="Win32Resource.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>WindowsSDKCommon package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object Build(C.Win32Resource resourceFile, out bool success)
        {
            string resourceFilePath = resourceFile.ResourceFile.AbsolutePath;
            if (!System.IO.File.Exists(resourceFilePath))
            {
                throw new Opus.Core.Exception(System.String.Format("Resource file '{0}' does not exist", resourceFilePath));
            }

            Opus.Core.Log.MessageAll("TODO: Stub function for QMake support for {0}", resourceFilePath);

            success = true;
            return null;
#if false
            C.Win32ResourceCompilerOptionCollection compilerOptions = resourceFile.Options as C.Win32ResourceCompilerOptionCollection;

            // dependency checking, source against output files
            {
                Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
                inputFiles.Add(resourceFilePath);
                Opus.Core.StringArray outputFiles = compilerOptions.OutputPaths.Paths;
                if (!RequiresBuilding(outputFiles, inputFiles))
                {
                    Opus.Core.Log.DebugMessage("'{0}' is up-to-date", (resourceFile as Opus.Core.IModule).OwningNode.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            Opus.Core.Target target = (resourceFile as Opus.Core.IModule).OwningNode.Target;

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

            // add output path
            commandLineBuilder.Add(System.String.Format("/fo {0}", compilerOptions.CompiledResourceFilePath));

            C.Win32ResourceCompiler compilerInstance = C.Win32ResourceCompilerFactory.GetTargetInstance(target);
            Opus.Core.ITool compilerTool = compilerInstance as Opus.Core.ITool;

            string executablePath = compilerTool.Executable(target);
            if (resourceFilePath.Contains(" "))
            {
                commandLineBuilder.Add(System.String.Format("\"{0}\"", resourceFilePath));
            }
            else
            {
                commandLineBuilder.Add(resourceFilePath);
            }

            int exitCode = CommandLineProcessor.Processor.Execute((resourceFile as Opus.Core.IModule).OwningNode, compilerTool, executablePath, commandLineBuilder);
            success = (0 == exitCode);

            return null;
#endif
        }
    }
}