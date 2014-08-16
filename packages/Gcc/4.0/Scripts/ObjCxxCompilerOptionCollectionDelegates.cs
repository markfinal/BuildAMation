// Automatically generated file from OpusOptionCodeGenerator.
// Command line arguments:
//     -i=../../../C/dev/Scripts/ICxxCompilerOptions.cs
//     -n=Gcc
//     -c=ObjCxxCompilerOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs
//     -pv=GccCommon.PrivateData
//     -e

namespace Gcc
{
    public partial class ObjCxxCompilerOptionCollection
    {
        #region C.ICxxCompilerOptions Option delegates
        private static void
        ExceptionHandlerCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            base.SetDelegates(node);
            this["ExceptionHandler"].PrivateData = new GccCommon.PrivateData(ExceptionHandlerCommandLineProcessor);
        }
    }
}
