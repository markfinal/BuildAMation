// <copyright file="CObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object Build(C.ObjectFile objectFile, out bool success)
        {
            Opus.Core.IModule objectFileModule = objectFile as Opus.Core.IModule;
            Opus.Core.DependencyNode node = objectFileModule.OwningNode;
            Opus.Core.Target target = node.Target;
            C.Toolchain toolchain = C.ToolchainFactory.GetTargetInstance(target);
            C.Compiler compilerInstance = C.CompilerFactory.GetTargetInstance(target, C.ClassNames.CCompilerTool);
            Opus.Core.ITool compilerTool = compilerInstance as Opus.Core.ITool;
            Opus.Core.BaseOptionCollection objectFileOptions = objectFileModule.Options;
            C.ICCompilerOptions compilerOptions = objectFileOptions as C.ICCompilerOptions;

            // NEW STYLE
#if true
            string executablePath = compilerTool.Executable(target);
#else
            string executable;
            C.IToolchainOptions toolchainOptions = (objectFileOptions as C.ICCompilerOptions).ToolchainOptionCollection as C.IToolchainOptions;
            if (toolchainOptions.IsCPlusPlus)
            {
                executable = compilerInstance.ExecutableCPlusPlus(target);
            }
            else
            {
                executable = compilerTool.Executable(target);
            }
#endif

            string sourceFilePath = objectFile.SourceFile.AbsolutePath;

            Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
            inputFiles.Add(sourceFilePath);

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
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
            recipe = recipe.Replace(objectFileOptions.OutputPaths[C.OutputFileFlags.ObjectFile], "$@");

            Opus.Core.StringArray recipes = new Opus.Core.StringArray();
            recipes.Add(recipe);

            string makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            MakeFile makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            MakeFileRule rule = new MakeFileRule(objectFileOptions.OutputPaths, C.OutputFileFlags.ObjectFile, node.UniqueModuleName, directoriesToCreate, null, inputFiles, recipes);
            makeFile.RuleArray.Add(rule);

            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            MakeFileTargetDictionary targetDictionary = makeFile.ExportedTargets;
            MakeFileVariableDictionary variableDictionary = makeFile.ExportedVariables;
            Opus.Core.StringArray environmentPaths = null;
            if (compilerTool is Opus.Core.IToolEnvironmentPaths)
            {
                environmentPaths = (compilerTool as Opus.Core.IToolEnvironmentPaths).Paths(target);
            }
            MakeFileData returnData = new MakeFileData(makeFilePath, targetDictionary, variableDictionary, environmentPaths);
            success = true;
            return returnData;
        }
    }
}