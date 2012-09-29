// <copyright file="MocFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public partial class QMakeBuilder
    {
        public object Build(QtCommon.MocFile mocFile, out System.Boolean success)
        {
            Opus.Core.IModule mocFileModule = mocFile as Opus.Core.IModule;
            Opus.Core.Target target = mocFileModule.OwningNode.Target;
            Opus.Core.BaseOptionCollection mocFileOptions = mocFileModule.Options;

            QtCommon.IMocOptions mocOptions = mocFileOptions as QtCommon.IMocOptions;
            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            if (mocFileOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = mocFileOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);
            }
            else
            {
                throw new Opus.Core.Exception("Moc options does not support command line translation");
            }

            NodeData nodeData = new NodeData();
            nodeData.Configuration = GetQtConfiguration(target);
            nodeData.AddVariable("HEADERS", mocFile.SourceFile.AbsolutePath);
            nodeData.AddUniqueVariable("MOC_DIR", new Opus.Core.StringArray(System.IO.Path.GetDirectoryName(mocOptions.MocOutputPath)));

            QtCommon.MocTool tool = new QtCommon.MocTool();
            string toolExePath = tool.Executable(target);
            nodeData.AddUniqueVariable("QMAKE_MOC", new Opus.Core.StringArray(toolExePath.Replace("\\", "/")));

            success = true;
            return nodeData;
        }
    }
}