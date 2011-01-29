namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(CodeGenTest2.CodeGenModule codeGenModule, Opus.Core.DependencyNode node, out bool success)
        {
            Opus.Core.Target target = node.Target;
            CodeGenTest2.CodeGenTool tool = new CodeGenTest2.CodeGenTool();
            CodeGenTest2.CodeGenOptions toolOptions = codeGenModule.Options as CodeGenTest2.CodeGenOptions;

            Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
            inputFiles.Add(tool.Executable(target));
            Opus.Core.StringArray outputFiles = codeGenModule.Options.OutputPaths.Paths;
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
                throw new Opus.Core.Exception("CodeGen options does not support command line translation");
            }

            int exitCode = CommandLineProcessor.Processor.Execute(node, tool, tool.Executable(target), commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}