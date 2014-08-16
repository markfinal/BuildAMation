// Automatically generated file from OpusOptionCodeGenerator.
// Command line arguments:
//     -i=../../../C/dev/Scripts/ICxxCompilerOptions.cs
//     -n=ComposerXE
//     -c=CxxCompilerOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs
//     -pv=ComposerXECommon.PrivateData
//     -e

namespace ComposerXE
{
    public partial class CxxCompilerOptionCollection
    {
        #region C.ICxxCompilerOptions Option delegates
        private static void
        ExceptionHandlerCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            ComposerXECommon.CxxCompilerOptionCollection.ExceptionHandlerCommandLineProcessor(sender, commandLineBuilder, option, target);
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            base.SetDelegates(node);
            this["ExceptionHandler"].PrivateData = new ComposerXECommon.PrivateData(ExceptionHandlerCommandLineProcessor);
        }
    }
}
