// <copyright file="CCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    public partial class CCompilerOptionCollection : MingwCommon.CCompilerOptionCollection
    {
        public CCompilerOptionCollection()
            : base()
        {
        }

        public CCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            // requires gcc 4.0, and only works on ELFs
            //this.Visibility = EVisibility.Hidden;
        }

        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            // common compiler options

            // compiler specific options
            //this["Visibility"].PrivateData = new MingwCommon.PrivateData(VisibilityCommandLine);
        }

        // requires gcc 4.0
        private static void VisibilityCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
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
    }
}