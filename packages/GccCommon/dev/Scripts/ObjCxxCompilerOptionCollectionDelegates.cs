// Automatically generated file from OpusOptionCodeGenerator.
// Command line:
// -i=../../../C/dev/Scripts/ICxxCompilerOptions.cs -n=GccCommon -c=ObjCxxCompilerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs:../../../XcodeProjectProcessor/dev/Scripts/Delegate.cs -pv=PrivateData -e -b

namespace GccCommon
{
    public partial class ObjCxxCompilerOptionCollection
    {
        #region C.ICxxCompilerOptions Option delegates
        public static void ExceptionHandlerCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.Cxx.EExceptionHandler> exceptionHandlerOption = option as Opus.Core.ValueTypeOption<C.Cxx.EExceptionHandler>;
            switch (exceptionHandlerOption.Value)
            {
            case C.Cxx.EExceptionHandler.Disabled:
                commandLineBuilder.Add("-fno-exceptions");
                break;
            case C.Cxx.EExceptionHandler.Asynchronous:
            case C.Cxx.EExceptionHandler.Synchronous:
                commandLineBuilder.Add("-fexceptions");
                break;
            default:
                throw new Opus.Core.Exception("Unrecognized exception handler option");
            }
        }
        public static void ExceptionHandlerXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var exceptionHandler = option as Opus.Core.ValueTypeOption<C.Cxx.EExceptionHandler>;
            var exceptionsOption = configuration.Options["GCC_ENABLE_CPP_EXCEPTIONS"];
            switch (exceptionHandler.Value)
            {
                case C.Cxx.EExceptionHandler.Disabled:
                    exceptionsOption.AddUnique("NO");
                    break;
                case C.Cxx.EExceptionHandler.Asynchronous:
                case C.Cxx.EExceptionHandler.Synchronous:
                    exceptionsOption.AddUnique("YES");
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized exception handler option");
            }

            if (exceptionsOption.Count != 1)
            {
                throw new Opus.Core.Exception("More than one exceptions option has been set");
            }
        }
        #endregion
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            base.SetDelegates(node);
            this["ExceptionHandler"].PrivateData = new PrivateData(ExceptionHandlerCommandLineProcessor,ExceptionHandlerXcodeProjectProcessor);
        }
    }
}
