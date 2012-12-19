// Automatically generated file from OpusOptionInterfacePropertyGenerator.
// Command line:
// -i=..\..\..\C\dev\Scripts\ICxxCompilerOptions.cs -n=Mingw -c=CxxCompilerOptionCollection -p -d -dd=..\..\..\CommandLineProcessor\dev\Scripts\CommandLineDelegate.cs -pv=MingwCommon.PrivateData -e

namespace Mingw
{
    public partial class CxxCompilerOptionCollection
    {
        #region C.ICxxCompilerOptions Option delegates
        private static void ExceptionHandlerCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            MingwCommon.CxxCompilerOptionCollection.ExceptionHandlerCommandLineProcessor(sender, commandLineBuilder, option, target);
        }
        #endregion
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            base.SetDelegates(node);
            this["ExceptionHandler"].PrivateData = new MingwCommon.PrivateData(ExceptionHandlerCommandLineProcessor);
        }
    }
}
