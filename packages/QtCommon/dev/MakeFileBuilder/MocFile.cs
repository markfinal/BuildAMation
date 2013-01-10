// <copyright file="MocFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public partial class MakeFileBuilder
    {
        public object Build(QtCommon.MocFile mocFile, out System.Boolean success)
        {
            Opus.Core.BaseModule mocFileModule = mocFile as Opus.Core.BaseModule;
            Opus.Core.DependencyNode node = mocFileModule.OwningNode;
            Opus.Core.Target target = node.Target;
            Opus.Core.BaseOptionCollection mocFileOptions = mocFileModule.Options;
            QtCommon.MocOptionCollection toolOptions = mocFileOptions as QtCommon.MocOptionCollection;

            string sourceFilePath = mocFile.SourceFile.AbsolutePath;
            if (!System.IO.File.Exists(sourceFilePath))
            {
                throw new Opus.Core.Exception("Moc source file '{0}' does not exist", sourceFilePath);
            }

            Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
            inputFiles.Add(sourceFilePath);

            Opus.Core.StringArray outputFiles = new Opus.Core.StringArray();
            node.FilterOutputPaths(QtCommon.OutputFileFlags.MocGeneratedSourceFile, outputFiles);

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

            if (sourceFilePath.Contains(" "))
            {
                commandLineBuilder.Add(System.String.Format("\"{0}\"", sourceFilePath));
            }
            else
            {
                commandLineBuilder.Add(sourceFilePath);
            }

            Opus.Core.ITool tool = target.Toolset.Tool(typeof(QtCommon.IMocTool));
            string toolExePath = tool.Executable((Opus.Core.BaseTarget)target);

            Opus.Core.StringArray recipes = new Opus.Core.StringArray();
            if (toolExePath.Contains(" "))
            {
                recipes.Add("\"" + toolExePath + "\" " + commandLineBuilder.ToString(' '));
            }
            else
            {
                recipes.Add(toolExePath + " " + commandLineBuilder.ToString(' '));
            }

            string makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));
            Opus.Core.Log.DebugMessage("Makefile : '{0}'", makeFilePath);

            MakeFile makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            MakeFileRule rule = new MakeFileRule(mocFileOptions.OutputPaths, QtCommon.OutputFileFlags.MocGeneratedSourceFile, node.UniqueModuleName, directoriesToCreate, null, inputFiles, recipes);
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
