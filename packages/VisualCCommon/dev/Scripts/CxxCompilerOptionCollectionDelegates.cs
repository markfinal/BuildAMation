// Automatically generated file from OpusOptionCodeGenerator.
// Command line:
// -i=../../../C/dev/Scripts/ICxxCompilerOptions.cs -n=VisualCCommon -c=CxxCompilerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs;../../../VisualStudioProcessor/dev/Scripts/VisualStudioDelegate.cs -pv=PrivateData -e

namespace VisualCCommon
{
    public partial class CxxCompilerOptionCollection
    {
        #region C.ICxxCompilerOptions Option delegates
        private static void ExceptionHandlerCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.Cxx.EExceptionHandler> exceptionHandlerOption = option as Opus.Core.ValueTypeOption<C.Cxx.EExceptionHandler>;
            switch (exceptionHandlerOption.Value)
            {
                case C.Cxx.EExceptionHandler.Disabled:
                    // nothing
                    break;
                case C.Cxx.EExceptionHandler.Asynchronous:
                    commandLineBuilder.Add("/EHa");
                    break;
                case C.Cxx.EExceptionHandler.Synchronous:
                    commandLineBuilder.Add("/EHsc");
                    break;
                case C.Cxx.EExceptionHandler.SyncWithCExternFunctions:
                    commandLineBuilder.Add("/EHs");
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized exception handler option");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary ExceptionHandlerVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            VisualStudioProcessor.ToolAttributeDictionary returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            Opus.Core.ValueTypeOption<C.Cxx.EExceptionHandler> exceptionHandlerOption = option as Opus.Core.ValueTypeOption<C.Cxx.EExceptionHandler>;
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                switch (exceptionHandlerOption.Value)
                {
                    case C.Cxx.EExceptionHandler.Disabled:
                    case C.Cxx.EExceptionHandler.Asynchronous:
                    case C.Cxx.EExceptionHandler.Synchronous:
                    case C.Cxx.EExceptionHandler.SyncWithCExternFunctions:
                        returnVal.Add("ExceptionHandling", System.String.Format("{0}", (int)exceptionHandlerOption.Value));
                        break;
                    default:
                        throw new Opus.Core.Exception("Unrecognized exception handler option");
                }
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                switch (exceptionHandlerOption.Value)
                {
                    case C.Cxx.EExceptionHandler.Disabled:
                        returnVal.Add("ExceptionHandling", "false");
                        break;
                    case C.Cxx.EExceptionHandler.Asynchronous:
                        returnVal.Add("ExceptionHandling", "Async");
                        break;
                    case C.Cxx.EExceptionHandler.Synchronous:
                        returnVal.Add("ExceptionHandling", "Sync");
                        break;
                    case C.Cxx.EExceptionHandler.SyncWithCExternFunctions:
                        returnVal.Add("ExceptionHandling", "SyncCThrow");
                        break;
                    default:
                        throw new Opus.Core.Exception("Unrecognized exception handler option");
                }
            }
            return returnVal;
        }
        #endregion
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            base.SetDelegates(node);
            this["ExceptionHandler"].PrivateData = new PrivateData(ExceptionHandlerCommandLineProcessor,ExceptionHandlerVisualStudioProcessor);
        }
    }
}
