// Automatically generated file from OpusOptionCodeGenerator.
// Command line:
// -i=ISymlinkOptions.cs -n=FileUtilities -c=SymlinkOptionCollection -p -d -dd=../../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs -pv=PrivateData

namespace FileUtilities
{
    public partial class SymlinkOptionCollection
    {
        #region ISymlinkOptions Option delegates
        private static void TargetNameCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        #endregion
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            this["TargetName"].PrivateData = new PrivateData(TargetNameCommandLineProcessor);
        }
    }
}
