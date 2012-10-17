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

            IToolchainOptions toolchainInterface = this as IToolchainOptions;
            toolchainInterface.RuntimeLibrary = ERuntimeLibrary.MultiThreadedDLL;
        }

        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            // common toolchain options
            this["IsCPlusPlus"].PrivateData = new PrivateData(null, null);
#if false
            this["CharacterSet"].PrivateData = new PrivateData(CharacterSetCommandLine, null);
#endif

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

#if false
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
#endif

        private static void RuntimeLibraryCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            if (!target.HasToolchain("visualc"))
            {
                return;
            }

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

                case ERuntimeLibrary.MultiThreadedDebugDLL:
                    commandLineBuilder.Add("/MDd");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized runtime library option");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary RuntimeLibraryVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            if (!target.HasToolchain("visualc"))
            {
                return dictionary;
            }

            Opus.Core.ValueTypeOption<ERuntimeLibrary> runtimeLibraryOption = option as Opus.Core.ValueTypeOption<ERuntimeLibrary>;
            switch (runtimeLibraryOption.Value)
            {
                case ERuntimeLibrary.MultiThreaded:
                case ERuntimeLibrary.MultiThreadedDebug:
                case ERuntimeLibrary.MultiThreadedDLL:
                case ERuntimeLibrary.MultiThreadedDebugDLL:
                    {
                        if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
                        {
                            dictionary.Add("RuntimeLibrary", runtimeLibraryOption.Value.ToString("D"));
                        }
                        else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
                        {
                            dictionary.Add("RuntimeLibrary", runtimeLibraryOption.Value.ToString());
                        }
                        return dictionary;
                    }

                default:
                    throw new Opus.Core.Exception("Unrecognized runtime library option");
            }
        }

        VisualStudioProcessor.ToolAttributeDictionary VisualStudioProcessor.IVisualStudioSupport.ToVisualStudioProjectAttributes(Opus.Core.Target target)
        {
            // NEW STYLE
#if true
            Opus.Core.IToolsetInfo info = Opus.Core.ToolsetInfoFactory.CreateToolsetInfo(typeof(VisualC.ToolsetInfo));
            VisualStudioProcessor.IVisualStudioTargetInfo vsInfo = info as VisualStudioProcessor.IVisualStudioTargetInfo;
            VisualStudioProcessor.EVisualStudioTarget vsTarget = vsInfo.VisualStudioTarget;
#else
            VisualCCommon.Toolchain toolchain = C.ToolchainFactory.GetTargetInstance(target) as VisualCCommon.Toolchain;
            VisualStudioProcessor.EVisualStudioTarget vsTarget = toolchain.VisualStudioTarget;
#endif
            switch (vsTarget)
            {
                case VisualStudioProcessor.EVisualStudioTarget.VCPROJ:
                case VisualStudioProcessor.EVisualStudioTarget.MSBUILD:
                    break;

                default:
                    throw new Opus.Core.Exception(System.String.Format("Unsupported VisualStudio target, '{0}'", vsTarget));
            }
            VisualStudioProcessor.ToolAttributeDictionary dictionary = VisualStudioProcessor.ToVisualStudioAttributes.Execute(this, target, vsTarget);
            return dictionary;
        }
    }
}