// <copyright file="CPlusPlusCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public abstract partial class CPlusPlusCompilerOptionCollection : CCompilerOptionCollection, C.ICxxCompilerOptions
    {
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            base.SetDelegates(node);

            this["ExceptionHandler"].PrivateData = new PrivateData(ExceptionHandlerCommandLine, ExceptionHandlerVisualStudio);
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            C.ICCompilerOptions cInterfaceOptions = this as C.ICCompilerOptions;
            C.ICxxCompilerOptions cxxInterfaceOptions = this as C.ICxxCompilerOptions;

            cInterfaceOptions.TargetLanguage = C.ETargetLanguage.Cxx;
            cxxInterfaceOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Disabled;
        }

        public CPlusPlusCompilerOptionCollection()
            : base()
        {
        }

        public CPlusPlusCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        private static void ExceptionHandlerCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
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

        private static VisualStudioProcessor.ToolAttributeDictionary ExceptionHandlerVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            Opus.Core.ValueTypeOption<C.Cxx.EExceptionHandler> exceptionHandlerOption = option as Opus.Core.ValueTypeOption<C.Cxx.EExceptionHandler>;
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                switch (exceptionHandlerOption.Value)
                {
                    case C.Cxx.EExceptionHandler.Disabled:
                    case C.Cxx.EExceptionHandler.Asynchronous:
                    case C.Cxx.EExceptionHandler.Synchronous:
                    case C.Cxx.EExceptionHandler.SyncWithCExternFunctions:
                        dictionary.Add("ExceptionHandling", System.String.Format("{0}", (int)exceptionHandlerOption.Value));
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
                        dictionary.Add("ExceptionHandling", "false");
                        break;

                    case C.Cxx.EExceptionHandler.Asynchronous:
                        dictionary.Add("ExceptionHandling", "Async");
                        break;

                    case C.Cxx.EExceptionHandler.Synchronous:
                        dictionary.Add("ExceptionHandling", "Sync");
                        break;

                    case C.Cxx.EExceptionHandler.SyncWithCExternFunctions:
                        dictionary.Add("ExceptionHandling", "SyncCThrow");
                        break;

                    default:
                        throw new Opus.Core.Exception("Unrecognized exception handler option");
                }
            }
            return dictionary;
        }
    }
}
