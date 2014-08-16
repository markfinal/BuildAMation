// Automatically generated file from OpusOptionCodeGenerator.
// Command line arguments:
//     -i=../../../C/dev/Scripts/ICxxCompilerOptions.cs
//     -n=ClangCommon
//     -c=CxxCompilerOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs&../../../XcodeProjectProcessor/dev/Scripts/Delegate.cs
//     -pv=PrivateData
//     -e

namespace ClangCommon
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
            // TODO: should this be empty?
        }
        private static void
        ExceptionHandlerXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            // TODO: should this be empty?
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            base.SetDelegates(node);
            this["ExceptionHandler"].PrivateData = new PrivateData(ExceptionHandlerCommandLineProcessor,ExceptionHandlerXcodeProjectProcessor);
        }
    }
}
