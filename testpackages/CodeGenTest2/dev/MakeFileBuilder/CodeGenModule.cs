namespace MakeFileBuilder
{
    public partial class MakeFileBuilder
    {
        public object Build(CodeGenTest2.CodeGenModule codeGenModule, out System.Boolean success)
        {
            Opus.Core.DependencyNode node = codeGenModule.OwningNode;
            Opus.Core.Target target = node.Target;
            CodeGenTest2.CodeGenTool tool = new CodeGenTest2.CodeGenTool();
            CodeGenTest2.CodeGenOptions toolOptions = codeGenModule.Options as CodeGenTest2.CodeGenOptions;
            string toolExePath = tool.Executable(target);

            Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
            inputFiles.Add(tool.Executable(target));
            Opus.Core.StringArray outputFiles = codeGenModule.Options.OutputPaths.Paths;

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

            MakeFileRule rule = new MakeFileRule(codeGenModule.Options.OutputPaths, CodeGenTest2.OutputFileFlags.GeneratedSourceFile, node.UniqueModuleName, directoriesToCreate, null, inputFiles, recipes);
            makeFile.RuleArray.Add(rule);

            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            success = true;

            MakeFileData returnData = new MakeFileData(makeFilePath, makeFile.ExportedTargets, makeFile.ExportedVariables, tool.EnvironmentPaths(target));
            return returnData;
        }
    }
}