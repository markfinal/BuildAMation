// Automatically generated file from OpusOptionInterfacePropertyGenerator.
// Command line:
// -i=..\..\..\C\dev\Scripts\ICPlusPlusCompilerOptions.cs -n=Clang -c=CxxCompilerOptionCollection -p -d -dd=..\..\..\CommandLineProcessor\dev\Scripts\CommandLineDelegate.cs -pv=PrivateData 

namespace Clang
{
    public partial class CxxCompilerOptionCollection
    {
        private void ExceptionHandlerCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            this["ExceptionHandler"].PrivateData = new PrivateData(ExceptionHandlerCommandLineProcessor);
        }
    }
}
