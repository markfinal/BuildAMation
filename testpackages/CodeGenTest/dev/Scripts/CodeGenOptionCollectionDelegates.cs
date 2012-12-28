// Automatically generated file from OpusOptionInterfacePropertyGenerator.
// Command line:
// -i=ICodeGenOptions.cs -n=CodeGenTest -c=CodeGenOptionCollection -p -d -dd=../../../../packages/CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs -pv=PrivateData

namespace CodeGenTest
{
    public partial class CodeGenOptionCollection
    {
        #region ICodeGenOptions Option delegates
        private static void OutputSourceDirectoryCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            commandLineBuilder.Add(stringOption.Value);
        }
        private static void OutputNameCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            commandLineBuilder.Add(stringOption.Value);
        }
        #endregion
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            this["OutputSourceDirectory"].PrivateData = new PrivateData(OutputSourceDirectoryCommandLineProcessor);
            this["OutputName"].PrivateData = new PrivateData(OutputNameCommandLineProcessor);
        }
    }
}
