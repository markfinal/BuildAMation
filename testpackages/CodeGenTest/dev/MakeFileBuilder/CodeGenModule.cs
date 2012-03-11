namespace MakeFileBuilder
{
    public partial class MakeFileBuilder
    {
        public object Build(CodeGenTest.CodeGenModule codeGenModule, out System.Boolean success)
        {
            Opus.Core.IModule codeGenModuleModule = codeGenModule as Opus.Core.IModule;
            Opus.Core.DependencyNode node = codeGenModuleModule.OwningNode;
            Opus.Core.Target target = node.Target;
            CodeGenTest.CodeGenTool tool = new CodeGenTest.CodeGenTool();
            Opus.Core.BaseOptionCollection codeGenModuleOptions = codeGenModuleModule.Options;
            CodeGenTest.CodeGenOptions toolOptions = codeGenModuleOptions as CodeGenTest.CodeGenOptions;
            string toolExePath = tool.Executable(target);

            Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
            inputFiles.Add(tool.Executable(target));
            Opus.Core.StringArray outputFiles = codeGenModuleOptions.OutputPaths.Paths;

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            Opus.Core.DirectoryCollection directoriesToCreate = null;
            if (toolOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = toolOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);

                directoriesToCreate = commandLineOption.DirectoriesToCreate();
            }
            else
            {
                throw new Opus.Core.Exception("Moc options does not support command line translation");
            }

            Opus.Core.StringArray recipes = new Opus.Core.StringArray();
            if (toolExePath.Contains(" "))
            {
                recipes.Add("\"" + toolExePath + "\" " + commandLineBuilder.ToString());
            }
            else
            {
                recipes.Add(toolExePath + " " + commandLineBuilder.ToString());
            }

            string makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));
            Opus.Core.Log.DebugMessage("Makefile : '{0}'", makeFilePath);

            MakeFile makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            MakeFileRule rule = new MakeFileRule(codeGenModuleOptions.OutputPaths, CodeGenTest.OutputFileFlags.GeneratedSourceFile, node.UniqueModuleName, directoriesToCreate, null, inputFiles, recipes);
            makeFile.RuleArray.Add(rule);

            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            Opus.Core.StringArray environmentPaths = null;
            if (tool is Opus.Core.IToolEnvironmentPaths)
            {
                environmentPaths = (tool as Opus.Core.IToolEnvironmentPaths).Paths(target);
            }
            MakeFileData returnData = new MakeFileData(makeFilePath, makeFile.ExportedTargets, makeFile.ExportedVariables, environmentPaths);
            success = true;
            return returnData;
        }
    }
}