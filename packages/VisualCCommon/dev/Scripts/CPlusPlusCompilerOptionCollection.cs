// <copyright file="CPlusPlusCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public abstract partial class CPlusPlusCompilerOptionCollection : CCompilerOptionCollection, C.ICPlusPlusCompilerOptions
    {
        private void SetDelegates(Opus.Core.Target target)
        {
            this["ExceptionHandler"].PrivateData = new PrivateData(ExceptionHandlerCommandLine, ExceptionHandlerVisualStudio);
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            (this.ToolchainOptionCollection as C.IToolchainOptions).IsCPlusPlus = true;
            this.TargetLanguage = C.ETargetLanguage.CPlusPlus;
            this.ExceptionHandler = C.CPlusPlus.EExceptionHandler.Disabled;

            this.SetDelegates(node.Target);
        }

        public CPlusPlusCompilerOptionCollection()
            : base()
        {
        }

        public CPlusPlusCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        private static void ExceptionHandlerCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.CPlusPlus.EExceptionHandler> exceptionHandlerOption = option as Opus.Core.ValueTypeOption<C.CPlusPlus.EExceptionHandler>;
            switch (exceptionHandlerOption.Value)
            {
                case C.CPlusPlus.EExceptionHandler.Disabled:
                    // nothing
                    break;

                case C.CPlusPlus.EExceptionHandler.Asynchronous:
                    commandLineBuilder.Append("/EHa ");
                    break;

                case C.CPlusPlus.EExceptionHandler.Synchronous:
                    commandLineBuilder.Append("/EHsc ");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized exception handler option");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary ExceptionHandlerVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            Opus.Core.ValueTypeOption<C.CPlusPlus.EExceptionHandler> exceptionHandlerOption = option as Opus.Core.ValueTypeOption<C.CPlusPlus.EExceptionHandler>;
            switch (exceptionHandlerOption.Value)
            {
                case C.CPlusPlus.EExceptionHandler.Disabled:
                case C.CPlusPlus.EExceptionHandler.Asynchronous:
                case C.CPlusPlus.EExceptionHandler.Synchronous:
                    dictionary.Add("ExceptionHandling", System.String.Format("{0}", (int)exceptionHandlerOption.Value));
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized exception handler option");
            }
            return dictionary;
        }
    }
}
