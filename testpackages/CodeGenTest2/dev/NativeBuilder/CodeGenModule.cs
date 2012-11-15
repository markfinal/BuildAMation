namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(CodeGenTest2.CodeGenModule codeGenModule, out bool success)
        {
            Opus.Core.IModule codeGenModuleModule = codeGenModule as Opus.Core.IModule;
            Opus.Core.DependencyNode node = codeGenModuleModule.OwningNode;
            Opus.Core.Target target = node.Target;
            // NEW STYLE
#if true
            Opus.Core.IToolset toolset = target.Toolset;
            Opus.Core.ITool tool = toolset.Tool(typeof(CodeGenTest2.ICodeGenTool));
#else
            CodeGenTest2.CodeGenTool tool = new CodeGenTest2.CodeGenTool();
#endif
            Opus.Core.BaseOptionCollection codeGenModuleOptions = codeGenModuleModule.Options;
            CodeGenTest2.CodeGenOptions toolOptions = codeGenModuleOptions as CodeGenTest2.CodeGenOptions;

            // dependency checking
            {
                Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
                inputFiles.Add(tool.Executable(target));
                Opus.Core.StringArray outputFiles = codeGenModuleOptions.OutputPaths.Paths;
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
                throw new Opus.Core.Exception("CodeGen options does not support command line translation");
            }

            int exitCode = CommandLineProcessor.Processor.Execute(node, tool, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}