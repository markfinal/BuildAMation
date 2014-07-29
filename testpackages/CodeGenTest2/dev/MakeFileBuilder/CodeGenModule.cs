namespace MakeFileBuilder
{
    public partial class MakeFileBuilder
    {
        public object
        Build(
            CodeGenTest2.CodeGenModule moduleToBuild,
            out System.Boolean success)
        {
            var codeGenModuleModule = moduleToBuild as Opus.Core.BaseModule;
            var node = codeGenModuleModule.OwningNode;
            var target = node.Target;
            var codeGenModuleOptions = codeGenModuleModule.Options;
            var toolOptions = codeGenModuleOptions as CodeGenTest2.CodeGenOptionCollection;
            var tool = target.Toolset.Tool(typeof(CodeGenTest2.ICodeGenTool));
            var toolExePath = tool.Executable((Opus.Core.BaseTarget)target);

            var inputFiles = new Opus.Core.StringArray();
            inputFiles.Add(toolExePath);

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

            var recipes = new Opus.Core.StringArray();
            if (toolExePath.Contains(" "))
            {
                recipes.Add("\"" + toolExePath + "\" " + commandLineBuilder.ToString());
            }
            else
            {
                recipes.Add(toolExePath + " " + commandLineBuilder.ToString());
            }

            var makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));
            Opus.Core.Log.DebugMessage("Makefile : '{0}'", makeFilePath);

            var makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            var rule = new MakeFileRule(
                moduleToBuild,
                CodeGenTest2.CodeGenModule.OutputFile,
                node.UniqueModuleName,
                dirsToCreate,
                null,
                inputFiles,
                recipes);
            rule.OutputLocationKeys = new Opus.Core.Array<Opus.Core.LocationKey>(CodeGenTest2.CodeGenModule.OutputFile);
            makeFile.RuleArray.Add(rule);

            using (var makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> environment = null;
            if (tool is Opus.Core.IToolEnvironmentVariables)
            {
                environment = (tool as Opus.Core.IToolEnvironmentVariables).Variables((Opus.Core.BaseTarget)target);
            }
            var returnData = new MakeFileData(makeFilePath, makeFile.ExportedTargets, makeFile.ExportedVariables, environment);
            success = true;
            return returnData;
        }
    }
}
