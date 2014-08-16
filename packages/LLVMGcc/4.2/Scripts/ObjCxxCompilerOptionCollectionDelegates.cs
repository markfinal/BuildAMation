// Automatically generated file from OpusOptionCodeGenerator.
// Command line arguments:
//     -i=../../../C/dev/Scripts/ICxxCompilerOptions.cs
//     -n=LLVMGcc
//     -c=ObjCxxCompilerOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs&../../../XcodeProjectProcessor/dev/Scripts/Delegate.cs
//     -pv=GccCommon.PrivateData
//     -e

namespace LLVMGcc
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
            GccCommon.ObjCxxCompilerOptionCollection.ExceptionHandlerCommandLineProcessor(sender, commandLineBuilder, option, target);
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
            GccCommon.ObjCxxCompilerOptionCollection.ExceptionHandlerXcodeProjectProcessor(sender, project, currentObject, configuration, option, target);
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            base.SetDelegates(node);
            this["ExceptionHandler"].PrivateData = new GccCommon.PrivateData(ExceptionHandlerCommandLineProcessor,ExceptionHandlerXcodeProjectProcessor);
        }
    }
}
