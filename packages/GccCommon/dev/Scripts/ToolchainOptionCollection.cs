// <copyright file="ToolchainOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
#if false
namespace GccCommon
{
    public abstract partial class ToolchainOptionCollection : C.ToolchainOptionCollection, C.IToolchainOptions
    {
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
#if false
            // common toolchain options
            this["IsCPlusPlus"].PrivateData = new PrivateData(null);
            this["CharacterSet"].PrivateData = new PrivateData(CharacterSetCommandLine);
#endif
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);
        }

        public ToolchainOptionCollection()
            : base()
        {
        }

        public ToolchainOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection dirsToCreate = new Opus.Core.DirectoryCollection();
            return dirsToCreate;
        }

        private static void CharacterSetCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.ECharacterSet> enumOption = option as Opus.Core.ValueTypeOption<C.ECharacterSet>;
            ToolchainOptionCollection options = sender as ToolchainOptionCollection;
            C.ICCompilerOptions compilerOptions = options.CCompilerOptionsInterface;
            switch (enumOption.Value)
            {
                case C.ECharacterSet.NotSet:
                    // do nothing
                    break;

                case C.ECharacterSet.Unicode:
                    compilerOptions.Defines.Add("_UNICODE");
                    compilerOptions.Defines.Add("UNICODE");
                    break;

                case C.ECharacterSet.MultiByte:
                    compilerOptions.Defines.Add("_MBCS");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized C.ECharacterSet option");
            }
        }
    }
}
#endif
