// <copyright file="CObjectFile.cs" company="Mark Final">
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
            C.ObjectFile moduleToBuild,
            out bool success)
        {
            var objectFileModule = moduleToBuild as Opus.Core.BaseModule;
            var node = objectFileModule.OwningNode;
            var target = node.Target;
            var moduleToolAttributes = moduleToBuild.GetType().GetCustomAttributes(typeof(Opus.Core.ModuleToolAssignmentAttribute), true);
            var toolType = (moduleToolAttributes[0] as Opus.Core.ModuleToolAssignmentAttribute).ToolType;
            var toolInterface = target.Toolset.Tool(toolType);
            var objectFileOptions = objectFileModule.Options;
            var compilerOptions = objectFileOptions as C.ICCompilerOptions;

            var sourceFilePath = moduleToBuild.SourceFileLocation.GetSinglePath();

            var inputFiles = new Opus.Core.StringArray();
            inputFiles.Add(sourceFilePath);

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

            var executable = toolInterface.Executable((Opus.Core.BaseTarget)target);

            string recipe = null;
            if (executable.Contains(" "))
            {
                recipe += System.String.Format("\"{0}\"", executable);
            }
            else
            {
                recipe += executable;
            }
            recipe += System.String.Format(" {0} $<", commandLineBuilder.ToString(' '));
            // replace target with $@
            var outputPath = moduleToBuild.Locations[C.ObjectFile.OutputFile].GetSinglePath();
            recipe = recipe.Replace(outputPath, "$@");

            var recipes = new Opus.Core.StringArray();
            recipes.Add(recipe);

            var makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            var makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            var rule = new MakeFileRule(
                moduleToBuild,
                C.ObjectFile.OutputFile,
                node.UniqueModuleName,
                dirsToCreate,
                null,
                inputFiles,
                recipes);

            var toolOutputLocKeys = toolInterface.OutputLocationKeys(moduleToBuild);
            var outputFileLocations = moduleToBuild.Locations.Keys(Opus.Core.ScaffoldLocation.ETypeHint.File, Opus.Core.Location.EExists.WillExist);
            var outputFileLocationsOfInterest = outputFileLocations.Intersect(toolOutputLocKeys);
            rule.OutputLocationKeys = outputFileLocationsOfInterest;

            makeFile.RuleArray.Add(rule);

            using (var makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            var targetDictionary = makeFile.ExportedTargets;
            var variableDictionary = makeFile.ExportedVariables;
            System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> environment = null;
            if (toolInterface is Opus.Core.IToolEnvironmentVariables)
            {
                environment = (toolInterface as Opus.Core.IToolEnvironmentVariables).Variables((Opus.Core.BaseTarget)target);
            }
            var returnData = new MakeFileData(makeFilePath, targetDictionary, variableDictionary, environment);
            success = true;
            return returnData;
        }
    }
}
