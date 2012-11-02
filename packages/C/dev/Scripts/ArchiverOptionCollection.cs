// <copyright file="ArchiverOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public abstract class ArchiverOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport
    {
        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            this.OutputName = node.ModuleName;

            // NEW STYLE
#if true
            Opus.Core.Target target = node.Target;

#if true
            Opus.Core.IToolset toolset = target.Toolset;
            IArchiverTool archiverTool = toolset.Tool(typeof(IArchiverTool)) as IArchiverTool;
#else
            Opus.Core.IToolset toolset = Opus.Core.State.Get("Toolset", target.Toolchain) as Opus.Core.IToolset;
            if (null == toolset)
            {
                throw new Opus.Core.Exception(System.String.Format("Toolset information for '{0}' is missing", target.Toolchain), false);
            }

            IArchiverInfo archiverTool = toolset as IArchiverInfo;
            if (null == archiverTool)
            {
                throw new Opus.Core.Exception(System.String.Format("Toolset information '{0}' does not implement the '{1}' interface for toolchain '{2}'", toolset.GetType().ToString(), typeof(IArchiverInfo).ToString(), target.Toolchain), false);
            }
#endif

            this.OutputDirectoryPath = node.GetTargettedModuleBuildDirectory(archiverTool.StaticLibraryOutputSubDirectory);
#else
            this.OutputDirectoryPath = node.GetTargettedModuleBuildDirectory(C.Toolchain.LibraryOutputSubDirectory);
#endif

            IArchiverOptions archiverOptions = this as IArchiverOptions;
#if false
            archiverOptions.ToolchainOptionCollection = ToolchainOptionCollection.GetSharedFromNode(node);
#endif
            archiverOptions.AdditionalOptions = "";
        }

        public ArchiverOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
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

        public string LibraryFilePath
        {
            get
            {
                return this.OutputPaths[C.OutputFileFlags.StaticLibrary];
            }

            set
            {
                this.OutputPaths[C.OutputFileFlags.StaticLibrary] = value;
            }
        }

        public override void FinalizeOptions(Opus.Core.Target target)
        {
            if (null == this.LibraryFilePath)
            {
                // NEW STYLE
#if true
#if true
                Opus.Core.IToolset toolset = target.Toolset;
                IArchiverTool archiverTool = toolset.Tool(typeof(IArchiverTool)) as IArchiverTool;
#else
                Opus.Core.IToolset toolset = Opus.Core.State.Get("Toolset", target.Toolchain) as Opus.Core.IToolset;
                if (null == toolset)
                {
                    throw new Opus.Core.Exception(System.String.Format("Toolset information for '{0}' is missing", target.Toolchain), false);
                }

                IArchiverInfo archiverTool = toolset as IArchiverInfo;
                if (null == archiverTool)
                {
                    throw new Opus.Core.Exception(System.String.Format("Toolset information '{0}' does not implement the '{1}' interface for toolchain '{2}'", toolset.GetType().ToString(), typeof(IArchiverInfo).ToString(), target.Toolchain), false);
                }
#endif

                string libraryPathname = System.IO.Path.Combine(this.OutputDirectoryPath, archiverTool.StaticLibraryPrefix + this.OutputName + archiverTool.StaticLibrarySuffix);
#else
                Toolchain toolchain = ToolchainFactory.GetTargetInstance(target);
                string libraryPathname = System.IO.Path.Combine(this.OutputDirectoryPath, toolchain.StaticLibraryPrefix + this.OutputName + toolchain.StaticLibrarySuffix);
#endif
                this.LibraryFilePath = libraryPathname;
            }

            base.FinalizeOptions(target);
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target);
        }

        public abstract Opus.Core.DirectoryCollection DirectoriesToCreate();
    }
}