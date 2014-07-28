// Automatically generated file from OpusOptionCodeGenerator.
// Command line:
// -i=ICCompilerOptions.cs -n=LLVMGcc -c=CCompilerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs:../../../XcodeProjectProcessor/dev/Scripts/Delegate.cs -pv=GccCommon.PrivateData -e

namespace LLVMGcc
{
    public partial class CCompilerOptionCollection
    {
        #region ICCompilerOptions Option delegates
        private static void VisibilityCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var enumOption = option as Opus.Core.ValueTypeOption<EVisibility>;
            switch (enumOption.Value)
            {
                case EVisibility.Default:
                    commandLineBuilder.Add("-fvisibility=default");
                    break;
                case EVisibility.Hidden:
                    commandLineBuilder.Add("-fvisibility=hidden");
                    break;
                case EVisibility.Internal:
                    commandLineBuilder.Add("-fvisibility=internal");
                    break;
                case EVisibility.Protected:
                    commandLineBuilder.Add("-fvisibility=protected");
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized visibility option");
            }
        }
        private static void VisibilityXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XcodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var visibility = option as Opus.Core.ValueTypeOption<EVisibility>;
            var visibilityOption = configuration.Options["GCC_SYMBOLS_PRIVATE_EXTERN"];
            if (visibility.Value == EVisibility.Default)
            {
                visibilityOption.AddUnique("NO");
            }
            else
            {
                visibilityOption.AddUnique("YES");
            }

            if (visibilityOption.Count != 1)
            {
                throw new Opus.Core.Exception("More than one visibility option has been set");
            }
        }
        #endregion
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            base.SetDelegates(node);
            this["Visibility"].PrivateData = new GccCommon.PrivateData(VisibilityCommandLineProcessor,VisibilityXcodeProjectProcessor);
        }
    }
}
