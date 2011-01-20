namespace NativeBuilder
{
    public partial class NativeBuilder
    {
        public object Build(Qt.MocFile mocFile, Opus.Core.DependencyNode node, out System.Boolean success)
        {
            Opus.Core.Target target = node.Target;
            Qt.MocTool tool = new Qt.MocTool();
            Qt.MocOptionCollection toolOptions = mocFile.Options as Qt.MocOptionCollection;
            string toolExePath = tool.Executable(target);

            Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
            inputFiles.Add(toolExePath);

            // TODO: this is very assuming of the base path - it would be ok if we were more explicit
            string sourceFilePath = System.IO.Path.Combine(node.Package.Directory, mocFile.SourceFile.RelativePath);
            if (!System.IO.File.Exists(sourceFilePath))
            {
                throw new Opus.Core.Exception(System.String.Format("Moc source file '{0}' does not exist", sourceFilePath));
            }
            inputFiles.Add(sourceFilePath);

            Opus.Core.IOutputPaths outputPaths = toolOptions as Opus.Core.IOutputPaths;
            Opus.Core.StringArray outputFiles = new Opus.Core.StringArray(outputPaths.GetOutputPaths().Values);
            if (!RequiresBuilding(outputFiles, inputFiles))
            {
                Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                success = true;
                return null;
            }

            System.Text.StringBuilder commandLineBuilder = new System.Text.StringBuilder();
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

            commandLineBuilder.AppendFormat("\"{0}\"", sourceFilePath);

            int exitCode = CommandLineProcessor.Processor.Execute(node, tool, toolExePath, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}