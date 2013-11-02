// <copyright file="Win32Resource.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object Build(C.Win32Resource moduleToBuild, out bool success)
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

            var commandLineBuilder = new Opus.Core.StringArray();
            Opus.Core.DirectoryCollection directoriesToCreate = null;
            if (compilerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = compilerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);

                directoriesToCreate = commandLineOption.DirectoriesToCreate();
            }
            else
            {
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
            }

            // add output path
            commandLineBuilder.Add(System.String.Format("/fo {0}", compilerOptions.CompiledResourceFilePath));

            var toolset = target.Toolset;
            var compilerTool = toolset.Tool(typeof(C.IWinResourceCompilerTool));
            string executablePath = compilerTool.Executable((Opus.Core.BaseTarget)target);

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
            recipe = recipe.Replace(resourceFileOptions.OutputPaths[C.OutputFileFlags.Win32CompiledResource], "$@");

            var recipes = new Opus.Core.StringArray();
            recipes.Add(recipe);

            var makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            var makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            var rule = new MakeFileRule(resourceFileOptions.OutputPaths, C.OutputFileFlags.Win32CompiledResource, node.UniqueModuleName, directoriesToCreate, null, inputFiles, recipes);
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