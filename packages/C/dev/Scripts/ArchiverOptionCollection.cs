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

            Opus.Core.Target target = node.Target;
            IArchiverTool archiverTool = target.Toolset.Tool(typeof(IArchiverTool)) as IArchiverTool;
            this.OutputDirectoryPath = node.GetTargettedModuleBuildDirectory(archiverTool.StaticLibraryOutputSubDirectory);

            IArchiverOptions archiverOptions = this as IArchiverOptions;
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

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
            if (!this.OutputPaths.Has(C.OutputFileFlags.StaticLibrary))
            {
                Opus.Core.Target target = node.Target;
                IArchiverTool archiverTool = target.Toolset.Tool(typeof(IArchiverTool)) as IArchiverTool;
                string libraryPathname = System.IO.Path.Combine(this.OutputDirectoryPath, archiverTool.StaticLibraryPrefix + this.OutputName + archiverTool.StaticLibrarySuffix);
                this.LibraryFilePath = libraryPathname;
            }

            base.FinalizeOptions(node);
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target, Opus.Core.StringArray excludedOptionNames)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target, excludedOptionNames);
        }

        public abstract Opus.Core.DirectoryCollection DirectoriesToCreate();
    }
}