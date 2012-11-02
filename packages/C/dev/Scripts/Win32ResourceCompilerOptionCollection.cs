// <copyright file="Win32ResourceCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public sealed partial class Win32ResourceCompilerOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport, VisualStudioProcessor.IVisualStudioSupport
    {
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            // do nothing yet
        }

        public Win32ResourceCompilerOptionCollection()
            : base()
        {
        }

        public Win32ResourceCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode owningNode)
        {
            // do nothing
        }

        public override void SetNodeOwnership(Opus.Core.DependencyNode node)
        {
            Win32Resource resourceModule = node.Module as Win32Resource;
            if (null != resourceModule)
            {
                string sourcePathName = resourceModule.ResourceFile.AbsolutePath;
                this.OutputName = System.IO.Path.GetFileNameWithoutExtension(sourcePathName);
            }

            // NEW STYLE
#if true
            Opus.Core.Target target = node.Target;
            Opus.Core.IToolset toolset = target.Toolset;
            ICompilerTool compilerTool = toolset.Tool(typeof(ICompilerTool)) as ICompilerTool;
            this.OutputDirectoryPath = node.GetTargettedModuleBuildDirectory(compilerTool.ObjectFileOutputSubDirectory);
#else
            this.OutputDirectoryPath = node.GetTargettedModuleBuildDirectory(C.Toolchain.ObjectFileOutputSubDirectory);
#endif
        }

        public override void FinalizeOptions(Opus.Core.Target target)
        {
            if (null == this.CompiledResourceFilePath)
            {
                // NEW STYLE
#if true
#if true
                Opus.Core.IToolset toolset = target.Toolset;
                IWinResourceCompilerTool resourceCompilerTool = toolset.Tool(typeof(IWinResourceCompilerTool)) as IWinResourceCompilerTool;
#else
                Opus.Core.IToolset toolset = Opus.Core.State.Get("Toolset", target.Toolchain) as Opus.Core.IToolset;
                if (null == toolset)
                {
                    throw new Opus.Core.Exception(System.String.Format("Toolset information for '{0}' is missing", target.Toolchain), false);
                }

                IWinResourceCompilerInfo resourceCompilerTool = toolset as IWinResourceCompilerInfo;
                if (null == resourceCompilerInfo)
                {
                    throw new Opus.Core.Exception(System.String.Format("Toolset information '{0}' does not implement the '{1}' interface for toolchain '{2}'", toolset.GetType().ToString(), typeof(IWinResourceCompilerInfo).ToString(), target.Toolchain), false);
                }
#endif

                string objectPathname = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + resourceCompilerTool.CompiledResourceSuffix;
#else
                Toolchain toolchain = ToolchainFactory.GetTargetInstance(target);

                string objectPathname = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + toolchain.Win32CompiledResourceSuffix;
#endif
                this.CompiledResourceFilePath = objectPathname;
            }

            base.FinalizeOptions(target);
        }

        public string OutputName
        {
            get;
            set;
        }

        public string OutputDirectoryPath
        {
            get;
            set;
        }

        public string CompiledResourceFilePath
        {
            get
            {
                return this.OutputPaths[C.OutputFileFlags.Win32CompiledResource];
            }

            set
            {
                this.OutputPaths[C.OutputFileFlags.Win32CompiledResource] = value;
            }
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target);
        }

        Opus.Core.DirectoryCollection CommandLineProcessor.ICommandLineSupport.DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directories = new Opus.Core.DirectoryCollection();
            directories.Add(System.IO.Path.GetDirectoryName(this.CompiledResourceFilePath), false);
            return directories;
        }

        VisualStudioProcessor.ToolAttributeDictionary VisualStudioProcessor.IVisualStudioSupport.ToVisualStudioProjectAttributes(Opus.Core.Target target)
        {
            // NEW STYLE
#if true
            Opus.Core.IToolset info = Opus.Core.ToolsetFactory.CreateToolset(typeof(VisualC.Toolset));
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