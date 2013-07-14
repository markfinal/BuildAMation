// Automatically generated file from OpusOptionCodeGenerator.
// Command line:
// -i=ICCompilerOptions.cs -n=Gcc -c=ObjCCompilerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs -pv=GccCommon.PrivateData -e

namespace Gcc
{
    public partial class ObjCCompilerOptionCollection
    {
        #region ICCompilerOptions Option delegates
        private static void VisibilityCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            // requires gcc 4.0
            Opus.Core.ValueTypeOption<EVisibility> enumOption = option as Opus.Core.ValueTypeOption<EVisibility>;
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
        #endregion
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            base.SetDelegates(node);
            this["Visibility"].PrivateData = new GccCommon.PrivateData(VisibilityCommandLineProcessor);
        }
    }
}
