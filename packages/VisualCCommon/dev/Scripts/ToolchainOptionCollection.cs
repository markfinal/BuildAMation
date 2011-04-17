// <copyright file="ToolChainOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public abstract partial class ToolchainOptionCollection : C.ToolchainOptionCollection, C.IToolchainOptions, IToolchainOptions, VisualStudioProcessor.IVisualStudioSupport
    {
        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            this.RuntimeLibrary = ERuntimeLibrary.MultiThreadedDLL;

            this.SetDelegates(node.Target);
        }

        private void SetDelegates(Opus.Core.Target target)
        {
            // common toolchain options
            this["IsCPlusPlus"].PrivateData = new PrivateData(null, null);
            this["CharacterSet"].PrivateData = new PrivateData(CharacterSetCommandLine, null);

            // toolchain specific options
            this["RuntimeLibrary"].PrivateData = new PrivateData(RuntimeLibraryCommandLine, RuntimeLibraryVisualStudio);
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

        private static void RuntimeLibraryCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<ERuntimeLibrary> runtimeLibraryOption = option as Opus.Core.ValueTypeOption<ERuntimeLibrary>;
            switch (runtimeLibraryOption.Value)
            {
                case ERuntimeLibrary.MultiThreaded:
                    commandLineBuilder.Add("/MT");
                    break;

                case ERuntimeLibrary.MultiThreadedDebug:
                    commandLineBuilder.Add("/MTd");
                    break;

                case ERuntimeLibrary.MultiThreadedDLL:
                    commandLineBuilder.Add("/MD");
                    break;

                case ERuntimeLibrary.MultiThreadedDLLDebug:
                    commandLineBuilder.Add("/MDd");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized runtime library option");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary RuntimeLibraryVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<ERuntimeLibrary> runtimeLibraryOption = option as Opus.Core.ValueTypeOption<ERuntimeLibrary>;
            switch (runtimeLibraryOption.Value)
            {
                case ERuntimeLibrary.MultiThreaded:
                case ERuntimeLibrary.MultiThreadedDebug:
                case ERuntimeLibrary.MultiThreadedDLL:
                case ERuntimeLibrary.MultiThreadedDLLDebug:
                    {
                        VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
                        dictionary.Add("RuntimeLibrary", runtimeLibraryOption.Value.ToString("D"));
                        return dictionary;
                    }

                default:
                    throw new Opus.Core.Exception("Unrecognized runtime library option");
            }
        }

        VisualStudioProcessor.ToolAttributeDictionary VisualStudioProcessor.IVisualStudioSupport.ToVisualStudioProjectAttributes(Opus.Core.Target target)
        {
            VisualStudioProcessor.ToolAttributeDictionary dictionary = VisualStudioProcessor.ToVisualStudioAttributes.Execute(this, target);
            return dictionary;
        }
    }
}