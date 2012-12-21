// Automatically generated file from OpusOptionInterfacePropertyGenerator.
// Command line:
// -i=ICCompilerOptions.cs -n=Mingw -c=CCompilerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs -pv=MingwCommon.PrivateData -e

namespace Mingw
{
    public partial class CCompilerOptionCollection
    {
        #region ICCompilerOptions Option delegates
        private static void VisibilityCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
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
            // TODO: only works on ELF executables
            //this["Visibility"].PrivateData = new MingwCommon.PrivateData(VisibilityCommandLineProcessor);
        }
    }
}
