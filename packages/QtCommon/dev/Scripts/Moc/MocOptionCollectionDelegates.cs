// Automatically generated file from OpusOptionCodeGenerator.
// Command line arguments:
//     -i=IMocOptions.cs
//     -n=QtCommon
//     -c=MocOptionCollection
//     -p
//     -d
//     -dd=../../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs
//     -pv=MocPrivateData

namespace QtCommon
{
    public partial class MocOptionCollection
    {
        #region IMocOptions Option delegates
        private static void
        IncludePathsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var directoryCollectionOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.DirectoryCollection>;
            // TODO: convert to var
            foreach (string directory in directoryCollectionOption.Value)
            {
                if (directory.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("-I\"{0}\"", directory));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("-I{0}", directory));
                }
            }
        }
        private static void
        DefinesCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var definesCollectionOption = option as Bam.Core.ReferenceTypeOption<C.DefineCollection>;
            foreach (var directory in definesCollectionOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-D{0}", directory));
            }
        }
        private static void
        DoNotGenerateIncludeStatementCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-i");
            }
        }
        private static void
        DoNotDisplayWarningsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-nw");
            }
        }
        private static void
        PathPrefixCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var stringOption = option as Bam.Core.ReferenceTypeOption<string>;
            if (stringOption.Value != null)
            {
                commandLineBuilder.Add(System.String.Format("-p {0}", stringOption.Value));
            }
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            this["IncludePaths"].PrivateData = new MocPrivateData(IncludePathsCommandLineProcessor);
            this["Defines"].PrivateData = new MocPrivateData(DefinesCommandLineProcessor);
            this["DoNotGenerateIncludeStatement"].PrivateData = new MocPrivateData(DoNotGenerateIncludeStatementCommandLineProcessor);
            this["DoNotDisplayWarnings"].PrivateData = new MocPrivateData(DoNotDisplayWarningsCommandLineProcessor);
            this["PathPrefix"].PrivateData = new MocPrivateData(PathPrefixCommandLineProcessor);
        }
    }
}
