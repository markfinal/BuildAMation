// <copyright file="LinkerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public abstract partial class LinkerOptionCollection : C.LinkerOptionCollection, C.ILinkerOptions, ILinkerOptions, VisualStudioProcessor.IVisualStudioSupport
    {
        protected override void SetDefaultOptionValues(Opus.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            var linkerInterface = this as ILinkerOptions;

            linkerInterface.NoLogo = true;
            linkerInterface.StackReserveAndCommit = null;
            linkerInterface.IgnoredLibraries = new Opus.Core.StringArray();
#if true
            // TODO: pdb later
#else
            this.ProgamDatabaseDirectoryPath = this.OutputDirectoryPath.Clone() as string;
#endif

            var target = node.Target;
            linkerInterface.IncrementalLink = target.HasConfiguration(Opus.Core.EConfiguration.Debug);

            var linkerTool = target.Toolset.Tool(typeof(C.ILinkerTool)) as C.ILinkerTool;

            foreach (var libPath in linkerTool.LibPaths((Opus.Core.BaseTarget)target))
            {
                (this as C.ILinkerOptions).LibraryPaths.Add(libPath);
            }
        }

        public LinkerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

#if true
#else
        public string ProgamDatabaseDirectoryPath
        {
            get;
            set;
        }
#endif

#if true
#else
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
#endif

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
#if true
            var options = this as C.ILinkerOptions;
            if (options.DebugSymbols)
            {
                var locationMap = node.Module.Locations;
                var pdbDir = locationMap[Linker.PDBDir] as Opus.Core.ScaffoldLocation;
                if (!pdbDir.IsValid)
                {
                    pdbDir.SetReference(locationMap[C.Application.OutputDir]);
                }

                var pdbFile = locationMap[Linker.PDBFile] as Opus.Core.ScaffoldLocation;
                if (!pdbFile.IsValid)
                {
                    pdbFile.SpecifyStub(pdbDir, this.OutputName + ".pdb", Opus.Core.Location.EExists.WillExist);
                }
            }

            base.FinalizeOptions(node);
#else
            var options = this as C.ILinkerOptions;

            if (options.DebugSymbols && !this.OutputPaths.Has(C.OutputFileFlags.LinkerProgramDatabase))
            {
                string pdbPathName = System.IO.Path.Combine(this.ProgamDatabaseDirectoryPath, this.OutputName) + ".pdb";
                this.ProgramDatabaseFilePath = pdbPathName;
            }

            base.FinalizeOptions(node);
#endif
        }

        VisualStudioProcessor.ToolAttributeDictionary VisualStudioProcessor.IVisualStudioSupport.ToVisualStudioProjectAttributes(Opus.Core.Target target)
        {
            var vsTarget = (target.Toolset as VisualStudioProcessor.IVisualStudioTargetInfo).VisualStudioTarget;
            switch (vsTarget)
            {
                case VisualStudioProcessor.EVisualStudioTarget.VCPROJ:
                case VisualStudioProcessor.EVisualStudioTarget.MSBUILD:
                    break;

                default:
                    throw new Opus.Core.Exception("Unsupported VisualStudio target, '{0}'", vsTarget);
            }
            var dictionary = VisualStudioProcessor.ToVisualStudioAttributes.Execute(this, target, vsTarget);
            return dictionary;
        }
    }
}