// <copyright file="CObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object Build(C.ObjectFile objectFile, Opus.Core.DependencyNode node, out bool success)
        {
            Opus.Core.Target target = node.Target;
            C.Toolchain toolchain = C.ToolchainFactory.GetTargetInstance(target);
            C.Compiler compilerInstance = C.CompilerFactory.GetTargetInstance(target, C.ClassNames.CCompilerTool);
            Opus.Core.ITool compilerTool = compilerInstance as Opus.Core.ITool;

            C.ICCompilerOptions compilerOptions = objectFile.Options as C.ICCompilerOptions;

            string executable;
            C.IToolchainOptions toolchainOptions = (objectFile.Options as C.ICCompilerOptions).ToolchainOptionCollection as C.IToolchainOptions;
            if (toolchainOptions.IsCPlusPlus)
            {
                executable = compilerInstance.ExecutableCPlusPlus(target);
            }
            else
            {
                executable = compilerTool.Executable(target);
            }

            string sourceFilePath = objectFile.SourceFile.AbsolutePath;

            Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
            inputFiles.Add(sourceFilePath);

            System.Text.StringBuilder commandLineBuilder = new System.Text.StringBuilder();
            Opus.Core.DirectoryCollection directoriesToCreate = null;
            if (compilerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = compilerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);

                directoriesToCreate = commandLineOption.DirectoriesToCreate();
            }
            else
            {
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
            }

            string recipe = System.String.Format("\"{0}\" {1}$<", executable, commandLineBuilder.ToString());
            // replace target with $@
            recipe = recipe.Replace(objectFile.Options.OutputPaths[C.OutputFileFlags.ObjectFile], "$@");

            Opus.Core.StringArray recipes = new Opus.Core.StringArray();
            recipes.Add(recipe);

            string makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            MakeFile makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            MakeFileRule rule = new MakeFileRule(objectFile.Options.OutputPaths, C.OutputFileFlags.ObjectFile, node.UniqueModuleName, directoriesToCreate, null, inputFiles, recipes);
            makeFile.RuleArray.Add(rule);

            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            MakeFileTargetDictionary targetDictionary = makeFile.ExportedTargets;
            MakeFileVariableDictionary variableDictionary = makeFile.ExportedVariables;
            MakeFileData returnData = new MakeFileData(makeFilePath, node.Parent != null, targetDictionary, variableDictionary, compilerTool.EnvironmentPaths(target));

            success = true;
            return returnData;
        }
    }
}