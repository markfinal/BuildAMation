// Automatically generated file from OpusOptionCodeGenerator.
// Command line arguments:
//     -i=ICCompilerOptions.cs
//     -n=Gcc
//     -c=CCompilerOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs
//     -pv=GccCommon.PrivateData
//     -e

namespace Gcc
{
    public partial class CCompilerOptionCollection
    {
        #region ICCompilerOptions Option delegates
        private static void
        VisibilityCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            // requires gcc 4.0
            var enumOption = option as Bam.Core.ValueTypeOption<EVisibility>;
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
                    throw new Bam.Core.Exception("Unrecognized visibility option");
            }
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            base.SetDelegates(node);
            this["Visibility"].PrivateData = new GccCommon.PrivateData(VisibilityCommandLineProcessor);
        }
    }
}
