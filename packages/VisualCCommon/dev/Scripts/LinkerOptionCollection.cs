// <copyright file="LinkerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public abstract partial class LinkerOptionCollection : C.LinkerOptionCollection, C.ILinkerOptions, ILinkerOptions, VisualStudioProcessor.IVisualStudioSupport
    {
        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            ILinkerOptions linkerInterface = this as ILinkerOptions;

            linkerInterface.NoLogo = true;
            linkerInterface.StackReserveAndCommit = null;
            linkerInterface.IgnoredLibraries = new Opus.Core.StringArray();
            this.ProgamDatabaseDirectoryPath = this.OutputDirectoryPath.Clone() as string;

            Opus.Core.Target target = node.Target;
            linkerInterface.IncrementalLink = target.HasConfiguration(Opus.Core.EConfiguration.Debug);

            C.ILinkerTool linkerTool = target.Toolset.Tool(typeof(C.ILinkerTool)) as C.ILinkerTool;

            foreach (string libPath in linkerTool.LibPaths((Opus.Core.BaseTarget)target))
            {
                (this as C.ILinkerOptions).LibraryPaths.Add(libPath);
            }
        }

        public LinkerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        public string ProgamDatabaseDirectoryPath
        {
            get;
            set;
        }

        public string ProgramDatabaseFilePath
        {
            get
            {
                return this.OutputPaths[C.OutputFileFlags.LinkerProgramDatabase];
            }

            set
            {
                this.OutputPaths[C.OutputFileFlags.LinkerProgramDatabase] = value;
            }
        }

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
            C.ILinkerOptions options = this as C.ILinkerOptions;

            if (options.DebugSymbols && !this.OutputPaths.Has(C.OutputFileFlags.LinkerProgramDatabase))
            {
                string pdbPathName = System.IO.Path.Combine(this.ProgamDatabaseDirectoryPath, this.OutputName) + ".pdb";
                this.ProgramDatabaseFilePath = pdbPathName;
            }

            base.FinalizeOptions(node);
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            if (this.OutputPaths.Has(C.OutputFileFlags.Executable))
            {
                string outputPathName = this.OutputFilePath;
                directoriesToCreate.Add(System.IO.Path.GetDirectoryName(outputPathName));
            }

            if (this.OutputPaths.Has(C.OutputFileFlags.StaticImportLibrary))
            {
                string libraryPathName = this.StaticImportLibraryFilePath;
                directoriesToCreate.Add(System.IO.Path.GetDirectoryName(libraryPathName));
            }

            if (this.OutputPaths.Has(C.OutputFileFlags.LinkerProgramDatabase))
            {
                string programDatabasePathName = this.ProgramDatabaseFilePath;
                directoriesToCreate.Add(System.IO.Path.GetDirectoryName(programDatabasePathName));
            }

            return directoriesToCreate;
        }

        VisualStudioProcessor.ToolAttributeDictionary VisualStudioProcessor.IVisualStudioSupport.ToVisualStudioProjectAttributes(Opus.Core.Target target)
        {
            VisualStudioProcessor.EVisualStudioTarget vsTarget = (target.Toolset as VisualStudioProcessor.IVisualStudioTargetInfo).VisualStudioTarget;
            switch (vsTarget)
            {
                case VisualStudioProcessor.EVisualStudioTarget.VCPROJ:
                case VisualStudioProcessor.EVisualStudioTarget.MSBUILD:
                    break;

                default:
                    throw new Opus.Core.Exception("Unsupported VisualStudio target, '{0}'", vsTarget);
            }
            VisualStudioProcessor.ToolAttributeDictionary dictionary = VisualStudioProcessor.ToVisualStudioAttributes.Execute(this, target, vsTarget);
            return dictionary;
        }
    }
}