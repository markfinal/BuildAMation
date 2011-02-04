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
            if (compilerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = compilerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);
            }
            else
            {
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
            }

            Opus.Core.StringArray commandLines = new Opus.Core.StringArray();
            commandLines.Add("\"" + executable + "\" " + commandLineBuilder.ToString() + " " + sourceFilePath);

            string makeFile = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFile));
            Opus.Core.Log.DebugMessage("Makefile : '{0}'", makeFile);

            MakeFileBuilderRecipe recipe = new MakeFileBuilderRecipe(node, inputFiles, null, commandLines, this.topLevelMakeFilePath);

            string makeFileTargetName = null;
            string makeFileVariableName = null;
            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFile))
            {
                recipe.Write(makeFileWriter, C.OutputFileFlags.ObjectFile);
                makeFileTargetName = recipe.TargetName;
                makeFileVariableName = recipe.VariableName;
            }

            success = true;

            MakeFileData returnData = new MakeFileData(makeFile, makeFileTargetName, makeFileVariableName, compilerTool.EnvironmentPaths(target));
            return returnData;
        }
    }
}