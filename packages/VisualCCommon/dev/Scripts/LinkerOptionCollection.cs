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
                (this as C.ILinkerOptions).LibraryPaths.AddAbsoluteDirectory(libPath, true);
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

#if false
        // TODO: this function is not called by anything... I think that the user should be responsible for adding runtime libraries if they have overridden the default
        private static void RuntimeLibraryCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            C.ILinkerOptions options = sender as C.ILinkerOptions;
            Opus.Core.ValueTypeOption<ERuntimeLibrary> runtimeLibraryOption = option as Opus.Core.ValueTypeOption<ERuntimeLibrary>;
            switch (runtimeLibraryOption.Value)
            {
                case ERuntimeLibrary.MultiThreaded:
                    options.StandardLibraries.Add("LIBCMT.lib");
                    break;

                case ERuntimeLibrary.MultiThreadedDebug:
                    options.StandardLibraries.Add("LIBCMTD.lib");
                    break;

                case ERuntimeLibrary.MultiThreadedDLL:
                    options.StandardLibraries.Add("MSVCRT.lib");
                    break;

                case ERuntimeLibrary.MultiThreadedDebugDLL:
                    options.StandardLibraries.Add("MSVCRTD.lib");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized ERuntimeLibrary option");
            }
        }

        // TODO: this function is not called by anything... I think that the user should be responsible for adding runtime libraries if they have overridden the default
        private static VisualStudioProcessor.ToolAttributeDictionary RuntimeLibraryVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            C.ILinkerOptions options = sender as C.ILinkerOptions;
            Opus.Core.ValueTypeOption<ERuntimeLibrary> runtimeLibraryOption = option as Opus.Core.ValueTypeOption<ERuntimeLibrary>;
            switch (runtimeLibraryOption.Value)
            {
                case ERuntimeLibrary.MultiThreaded:
                    options.StandardLibraries.Add("LIBCMT.lib");
                    break;

                case ERuntimeLibrary.MultiThreadedDebug:
                    options.StandardLibraries.Add("LIBCMTD.lib");
                    break;

                case ERuntimeLibrary.MultiThreadedDLL:
                    options.StandardLibraries.Add("MSVCRT.lib");
                    break;

                case ERuntimeLibrary.MultiThreadedDebugDLL:
                    options.StandardLibraries.Add("MSVCRTD.lib");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized ERuntimeLibrary option");
            }
            return null;
        }
#endif

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            if (this.OutputPaths.Has(C.OutputFileFlags.Executable))
            {
                string outputPathName = this.OutputFilePath;
                directoriesToCreate.AddAbsoluteDirectory(System.IO.Path.GetDirectoryName(outputPathName), false);
            }

            if (this.OutputPaths.Has(C.OutputFileFlags.StaticImportLibrary))
            {
                string libraryPathName = this.StaticImportLibraryFilePath;
                directoriesToCreate.AddAbsoluteDirectory(System.IO.Path.GetDirectoryName(libraryPathName), false);
            }

            if (this.OutputPaths.Has(C.OutputFileFlags.LinkerProgramDatabase))
            {
                string programDatabasePathName = this.ProgramDatabaseFilePath;
                directoriesToCreate.AddAbsoluteDirectory(System.IO.Path.GetDirectoryName(programDatabasePathName), false);
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