// <copyright file="Win32Resource.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object
        Build(
            C.Win32Resource moduleToBuild,
            out bool success)
        {
            var resourceFilePath = moduleToBuild.ResourceFileLocation.GetSinglePath();
            if (!System.IO.File.Exists(resourceFilePath))
            {
                throw new Opus.Core.Exception("Resource file '{0}' does not exist", resourceFilePath);
            }

            var inputFiles = new Opus.Core.StringArray();
            inputFiles.Add(resourceFilePath);

            var resourceFileModule = moduleToBuild as Opus.Core.BaseModule;
            var resourceFileOptions = resourceFileModule.Options;

            var compilerOptions = resourceFileOptions as C.Win32ResourceCompilerOptionCollection;

            var node = resourceFileModule.OwningNode;
            var target = node.Target;

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);

            var commandLineBuilder = new Opus.Core.StringArray();
            if (compilerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = compilerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
            }

            var toolset = target.Toolset;
            var compilerTool = toolset.Tool(typeof(C.IWinResourceCompilerTool)) as C.IWinResourceCompilerTool;

            // add output path
            var outputPath = moduleToBuild.Locations[C.Win32Resource.OutputFile].GetSinglePath();
            commandLineBuilder.Add(System.String.Format("{0}{1}",
                                                        compilerTool.OutputFileSwitch,
                                                        outputPath));

            var executablePath = compilerTool.Executable((Opus.Core.BaseTarget)target);

            string recipe = null;
            if (executablePath.Contains(" "))
            {
                recipe += System.String.Format("\"{0}\"", executablePath);
            }
            else
            {
                recipe += executablePath;
            }
            recipe += System.String.Format(" {0} $<", commandLineBuilder.ToString(' '));
            // replace target with $@
            recipe = recipe.Replace(outputPath, "$@");

            var recipes = new Opus.Core.StringArray();
            recipes.Add(recipe);

            var makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            var makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            var rule = new MakeFileRule(
                moduleToBuild,
                C.Win32Resource.OutputFile,
                node.UniqueModuleName,
                dirsToCreate,
                null,
                inputFiles,recipes);
            rule.OutputLocationKeys = new Opus.Core.Array<Opus.Core.LocationKey>(C.Win32Resource.OutputFile);
            makeFile.RuleArray.Add(rule);

            using (var makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            var targetDictionary = makeFile.ExportedTargets;
            var variableDictionary = makeFile.ExportedVariables;
            System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> environment = null;
            if (compilerTool is Opus.Core.IToolEnvironmentVariables)
            {
                environment = (compilerTool as Opus.Core.IToolEnvironmentVariables).Variables((Opus.Core.BaseTarget)target);
            }
            var returnData = new MakeFileData(makeFilePath, targetDictionary, variableDictionary, environment);
            success = true;
            return returnData;
        }
    }
}
