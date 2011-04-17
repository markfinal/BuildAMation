// <copyright file="CPlusPlusCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    public abstract partial class CPlusPlusCompilerOptionCollection : CCompilerOptionCollection, C.ICPlusPlusCompilerOptions
    {
        private void SetDelegates(Opus.Core.Target target)
        {
            this["ExceptionHandler"].PrivateData = new PrivateData(ExceptionHandlerCommandLine);
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

        private static void ExceptionHandlerCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.CPlusPlus.EExceptionHandler> exceptionHandlerOption = option as Opus.Core.ValueTypeOption<C.CPlusPlus.EExceptionHandler>;
            switch (exceptionHandlerOption.Value)
            {
                case C.CPlusPlus.EExceptionHandler.Disabled:
                    commandLineBuilder.Add("-fno-exceptions");
                    break;

                case C.CPlusPlus.EExceptionHandler.Asynchronous:
                case C.CPlusPlus.EExceptionHandler.Synchronous:
                    commandLineBuilder.Add("-fexceptions");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized exception handler option");
            }
        }
    }
}
