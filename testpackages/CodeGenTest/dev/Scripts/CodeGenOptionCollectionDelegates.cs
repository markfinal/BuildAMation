// Automatically generated file from OpusOptionCodeGenerator.
// Command line arguments:
//     -i=ICodeGenOptions.cs
//     -n=CodeGenTest
//     -c=CodeGenOptionCollection
//     -p
//     -d
//     -dd=../../../../packages/CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs
//     -pv=PrivateData

namespace CodeGenTest
{
    public partial class CodeGenOptionCollection
    {
        #region ICodeGenOptions Option delegates
        private static void
        OutputSourceDirectoryCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            Bam.Core.ReferenceTypeOption<string> stringOption = option as Bam.Core.ReferenceTypeOption<string>;
            commandLineBuilder.Add(stringOption.Value);
        }
        private static void
        OutputNameCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            Bam.Core.ReferenceTypeOption<string> stringOption = option as Bam.Core.ReferenceTypeOption<string>;
            commandLineBuilder.Add(stringOption.Value);
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            this["OutputSourceDirectory"].PrivateData = new PrivateData(OutputSourceDirectoryCommandLineProcessor);
            this["OutputName"].PrivateData = new PrivateData(OutputNameCommandLineProcessor);
        }
    }
}
