// <copyright file="MocFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public partial class NativeBuilder
    {
        public object Build(QtCommon.MocFile mocFile, out System.Boolean success)
        {
            Opus.Core.IModule mocFileModule = mocFile as Opus.Core.IModule;
            Opus.Core.DependencyNode node = mocFileModule.OwningNode;
            Opus.Core.Target target = node.Target;
            QtCommon.MocTool tool = new QtCommon.MocTool();
            Opus.Core.BaseOptionCollection mocFileOptions = mocFileModule.Options;
            QtCommon.MocOptionCollection toolOptions = mocFileOptions as QtCommon.MocOptionCollection;
            string toolExePath = tool.Executable(target);

            string sourceFilePath = mocFile.SourceFile.AbsolutePath;
            if (!System.IO.File.Exists(sourceFilePath))
            {
                throw new Opus.Core.Exception(System.String.Format("Moc source file '{0}' does not exist", sourceFilePath));
            }

            // dependency checking
            {
                Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
                inputFiles.Add(sourceFilePath);

                Opus.Core.StringArray outputFiles = mocFileOptions.OutputPaths.Paths;
                if (!RequiresBuilding(outputFiles, inputFiles))
                {
                    Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            if (toolOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = toolOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);

                Opus.Core.DirectoryCollection directoriesToCreate = commandLineOption.DirectoriesToCreate();
                foreach (string directoryPath in directoriesToCreate)
                {
                    NativeBuilder.MakeDirectory(directoryPath);
                }
            }
            else
            {
                throw new Opus.Core.Exception("Moc options does not support command line translation");
            }

            if (sourceFilePath.Contains(" "))
            {
                commandLineBuilder.Add(System.String.Format("\"{0}\"", sourceFilePath));
            }
            else
            {
                commandLineBuilder.Add(sourceFilePath);
            }

            int exitCode = CommandLineProcessor.Processor.Execute(node, tool, toolExePath, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}