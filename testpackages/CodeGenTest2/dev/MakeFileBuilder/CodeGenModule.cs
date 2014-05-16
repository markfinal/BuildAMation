namespace MakeFileBuilder
{
    public partial class MakeFileBuilder
    {
        public object Build(CodeGenTest2.CodeGenModule moduleToBuild, out System.Boolean success)
        {
            Opus.Core.BaseModule codeGenModuleModule = moduleToBuild as Opus.Core.BaseModule;
            Opus.Core.DependencyNode node = codeGenModuleModule.OwningNode;
            Opus.Core.Target target = node.Target;
            Opus.Core.BaseOptionCollection codeGenModuleOptions = codeGenModuleModule.Options;
            CodeGenTest2.CodeGenOptionCollection toolOptions = codeGenModuleOptions as CodeGenTest2.CodeGenOptionCollection;
            Opus.Core.ITool tool = target.Toolset.Tool(typeof(CodeGenTest2.ICodeGenTool));
            string toolExePath = tool.Executable((Opus.Core.BaseTarget)target);

            Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
            inputFiles.Add(toolExePath);
#if true
#else
            Opus.Core.StringArray outputFiles = codeGenModuleOptions.OutputPaths.Paths;
#endif

            // at this point, we know the node outputs need building

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);

            var commandLineBuilder = new Opus.Core.StringArray();
            if (toolOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = toolOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
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

#if true
            var rule = new MakeFileRule(
                moduleToBuild,
                CodeGenTest2.CodeGenModule.OutputFile,
                node.UniqueModuleName,
                dirsToCreate,
                null,
                inputFiles,
                recipes);
#else
            MakeFileRule rule = new MakeFileRule(codeGenModuleOptions.OutputPaths, CodeGenTest2.OutputFileFlags.GeneratedSourceFile, node.UniqueModuleName, directoriesToCreate, null, inputFiles, recipes);
#endif
            makeFile.RuleArray.Add(rule);

            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> environment = null;
            if (tool is Opus.Core.IToolEnvironmentVariables)
            {
                environment = (tool as Opus.Core.IToolEnvironmentVariables).Variables((Opus.Core.BaseTarget)target);
            }
            MakeFileData returnData = new MakeFileData(makeFilePath, makeFile.ExportedTargets, makeFile.ExportedVariables, environment);
            success = true;
            return returnData;
        }
    }
}