// Automatically generated file from OpusOptionInterfacePropertyGenerator.
// Command line:
// -i=IMocOptions.cs -n=QtCommon -c=MocOptionCollection -p -d -dd=..\..\..\..\CommandLineProcessor\dev\Scripts\CommandLineDelegate.cs -pv=MocPrivateData

namespace QtCommon
{
    public partial class MocOptionCollection
    {
        private static void MocOutputPathCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            if (stringOption.Value.Contains(" "))
            {
                commandLineBuilder.Add(System.String.Format("-o\"{0}\"", stringOption.Value));
            }
            else
            {
                commandLineBuilder.Add(System.String.Format("-o{0}", stringOption.Value));
            }
        }
        private static void IncludePathsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> directoryCollectionOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
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
        private static void DefinesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<C.DefineCollection> definesCollectionOption = option as Opus.Core.ReferenceTypeOption<C.DefineCollection>;
            foreach (string directory in definesCollectionOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-D{0}", directory));
            }
        }
        private static void DoNotGenerateIncludeStatementCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-i");
            }
        }
        private static void DoNotDisplayWarningsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-nw");
            }
        }
        private static void PathPrefixCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            if (stringOption.Value != null)
            {
                commandLineBuilder.Add(System.String.Format("-p {0}", stringOption.Value));
            }
        }
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            this["MocOutputPath"].PrivateData = new MocPrivateData(MocOutputPathCommandLineProcessor);
            this["IncludePaths"].PrivateData = new MocPrivateData(IncludePathsCommandLineProcessor);
            this["Defines"].PrivateData = new MocPrivateData(DefinesCommandLineProcessor);
            this["DoNotGenerateIncludeStatement"].PrivateData = new MocPrivateData(DoNotGenerateIncludeStatementCommandLineProcessor);
            this["DoNotDisplayWarnings"].PrivateData = new MocPrivateData(DoNotDisplayWarningsCommandLineProcessor);
            this["PathPrefix"].PrivateData = new MocPrivateData(PathPrefixCommandLineProcessor);
        }
    }
}
