// Automatically generated file from OpusOptionCodeGenerator.
// Command line:
// -i=../../../C/dev/Scripts/ICxxCompilerOptions.cs -n=LLVMGcc -c=ObjCxxCompilerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs -pv=GccCommon.PrivateData -e

namespace LLVMGcc
{
    public partial class ObjCxxCompilerOptionCollection
    {
        #region C.ICxxCompilerOptions Option delegates
        private static void ExceptionHandlerCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        #endregion
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            base.SetDelegates(node);
            this["ExceptionHandler"].PrivateData = new GccCommon.PrivateData(ExceptionHandlerCommandLineProcessor);
        }
    }
}
