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

            Opus.Core.IToolsetInfo toolsetInfo = Opus.Core.State.Get("ToolsetInfo", target.Toolchain) as Opus.Core.IToolsetInfo;
            if (null == toolsetInfo)
            {
                throw new Opus.Core.Exception(System.String.Format("Toolset information for '{0}' is missing", target.Toolchain), false);
            }

            IArchiverInfo archiverInfo = toolsetInfo as IArchiverInfo;
            if (null == archiverInfo)
            {
                throw new Opus.Core.Exception(System.String.Format("Archiver information for '{0}' is missing", target.Toolchain), false);
            }

            this.OutputDirectoryPath = node.GetTargettedModuleBuildDirectory(archiverInfo.StaticLibraryOutputSubDirectory);
#else
            this.OutputDirectoryPath = node.GetTargettedModuleBuildDirectory(C.Toolchain.LibraryOutputSubDirectory);
#endif

            IArchiverOptions archiverOptions = this as IArchiverOptions;
            archiverOptions.ToolchainOptionCollection = ToolchainOptionCollection.GetSharedFromNode(node);
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
            Toolchain toolchain = ToolchainFactory.GetTargetInstance(target);

            if (null == this.LibraryFilePath)
            {
                // NEW STYLE
#if true
                Opus.Core.IToolsetInfo toolsetInfo = Opus.Core.State.Get("ToolsetInfo", target.Toolchain) as Opus.Core.IToolsetInfo;
                if (null == toolsetInfo)
                {
                    throw new Opus.Core.Exception(System.String.Format("Toolset information for '{0}' is missing", target.Toolchain), false);
                }

                IArchiverInfo archiverInfo = toolsetInfo as IArchiverInfo;
                if (null == archiverInfo)
                {
                    throw new Opus.Core.Exception(System.String.Format("Archiver information for '{0}' is missing", target.Toolchain), false);
                }

                string libraryPathname = System.IO.Path.Combine(this.OutputDirectoryPath, archiverInfo.StaticLibraryPrefix + this.OutputName + archiverInfo.StaticLibrarySuffix);
#else
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