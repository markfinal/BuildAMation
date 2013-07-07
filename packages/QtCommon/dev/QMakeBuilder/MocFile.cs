// <copyright file="MocFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public partial class QMakeBuilder2
    {
        public object Build(QtCommon.MocFile moduleToBuild, out System.Boolean success)
        {
            var sourceFilePath = moduleToBuild.SourceFile.AbsolutePath;
            var node = moduleToBuild.OwningNode;
            var target = node.Target;

            var data = new QMakeData(node);
            data.PriPaths.Add(this.EmptyConfigPriPath);
            data.Headers.Add(sourceFilePath);
            data.Output = QMakeData.OutputType.MocFile;

            var options = moduleToBuild.Options as QtCommon.MocOptionCollection;
            data.MocDir = options.OutputDirectoryPath;

            success = true;
            return data;
        }
    }

    public partial class QMakeBuilder
    {
        public object Build(QtCommon.MocFile mocFile, out System.Boolean success)
        {
            var mocFileModule = mocFile as Opus.Core.BaseModule;
            var target = mocFileModule.OwningNode.Target;
            var mocFileOptions = mocFileModule.Options;

            var mocOptions = mocFileOptions as QtCommon.IMocOptions;
            var commandLineBuilder = new Opus.Core.StringArray();
            if (mocFileOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = mocFileOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Opus.Core.Exception("Moc options does not support command line translation");
            }

            NodeData nodeData = new NodeData();
            nodeData.Configuration = GetQtConfiguration(target);
            nodeData.AddVariable("HEADERS", mocFile.SourceFile.AbsolutePath);
            nodeData.AddUniqueVariable("MOC_DIR", new Opus.Core.StringArray(System.IO.Path.GetDirectoryName(mocOptions.MocOutputPath)));

            Opus.Core.ITool tool = target.Toolset.Tool(typeof(QtCommon.IMocTool));
            string toolExePath = tool.Executable((Opus.Core.BaseTarget)target);
            nodeData.AddUniqueVariable("QMAKE_MOC", new Opus.Core.StringArray(toolExePath.Replace("\\", "/")));

            success = true;
            return nodeData;
        }
    }
}