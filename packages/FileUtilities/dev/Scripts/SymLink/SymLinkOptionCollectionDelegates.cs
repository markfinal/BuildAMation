// Automatically generated file from OpusOptionInterfacePropertyGenerator.
// Command line:
// -i=ISymLinkOptions.cs -n=FileUtilities -c=SymLinkOptionCollection -p -d -dd=..\..\..\..\CommandLineProcessor\dev\Scripts\CommandLineDelegate.cs -pv=SymLinkPrivateData

namespace FileUtilities
{
    public partial class SymLinkOptionCollection
    {
        private static void LinkDirectoryCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            // empty
        }
        private static void LinkNameCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            // empty
        }
        private static void TypeCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            if (!target.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                return;
            }

            Opus.Core.ValueTypeOption<EType> enumOption = option as Opus.Core.ValueTypeOption<EType>;
            if (enumOption.Value == EType.Directory)
            {
                commandLineBuilder.Add("/D");
            }
        }
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            this["LinkDirectory"].PrivateData = new SymLinkPrivateData(LinkDirectoryCommandLineProcessor);
            this["LinkName"].PrivateData = new SymLinkPrivateData(LinkNameCommandLineProcessor);
            this["Type"].PrivateData = new SymLinkPrivateData(TypeCommandLineProcessor);
        }
    }
}
