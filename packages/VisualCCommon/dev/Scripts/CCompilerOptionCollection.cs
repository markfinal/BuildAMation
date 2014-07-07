// <copyright file="CCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection : C.CompilerOptionCollection, C.ICCompilerOptions, ICCompilerOptions, VisualStudioProcessor.IVisualStudioSupport
    {
        protected override void SetDefaultOptionValues(Opus.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            var target = node.Target;

            var compilerInterface = this as ICCompilerOptions;
            compilerInterface.NoLogo = true;

            if (target.HasConfiguration(Opus.Core.EConfiguration.Debug))
            {
                compilerInterface.MinimalRebuild = true;
                compilerInterface.BasicRuntimeChecks = EBasicRuntimeChecks.StackFrameAndUninitializedVariables;
                compilerInterface.SmallerTypeConversionRuntimeCheck = true;
                compilerInterface.InlineFunctionExpansion = EInlineFunctionExpansion.None;
                compilerInterface.EnableIntrinsicFunctions = false;
            }
            else
            {
                compilerInterface.MinimalRebuild = false;
                compilerInterface.BasicRuntimeChecks = EBasicRuntimeChecks.None;
                compilerInterface.SmallerTypeConversionRuntimeCheck = false;
                compilerInterface.InlineFunctionExpansion = EInlineFunctionExpansion.AnySuitable;
                compilerInterface.EnableIntrinsicFunctions = true;
            }

            var compilerTool = target.Toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            (this as C.ICCompilerOptions).SystemIncludePaths.AddRange(compilerTool.IncludePaths((Opus.Core.BaseTarget)target));

            (this as C.ICCompilerOptions).TargetLanguage = C.ETargetLanguage.C;

            compilerInterface.WarningLevel = EWarningLevel.Level4;

            compilerInterface.DebugType = EDebugType.Embedded;

            // disable browse information to improve build speed
            compilerInterface.BrowseInformation = EBrowseInformation.None;

            compilerInterface.StringPooling = true;
            compilerInterface.DisableLanguageExtensions = false;
            compilerInterface.ForceConformanceInForLoopScope = true;
            compilerInterface.UseFullPaths = true;
            compilerInterface.CompileAsManaged = EManagedCompilation.NoCLR;
            compilerInterface.RuntimeLibrary = ERuntimeLibrary.MultiThreadedDLL;
            compilerInterface.ForcedInclude = new Opus.Core.StringArray();

#if true
#else
            this.ProgamDatabaseDirectoryPath = this.OutputDirectoryPath.Clone() as string;
#endif
        }

        public CCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        public string ProgamDatabaseDirectoryPath
        {
            get;
            set;
        }

#if true
#else
        public string ProgramDatabaseFilePath
        {
            get
            {
                return this.OutputPaths[C.OutputFileFlags.CompilerProgramDatabase];
            }

            set
            {
                this.OutputPaths[C.OutputFileFlags.CompilerProgramDatabase] = value;
            }
        }
#endif

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
#if true
            var options = this as ICCompilerOptions;
            if (options.DebugType != EDebugType.Embedded)
            {
                var locationMap = node.Module.Locations;
                var pdbDirLoc = locationMap[CCompiler.PDBDir] as Opus.Core.ScaffoldLocation;
                if (!pdbDirLoc.IsValid)
                {
                    pdbDirLoc.SetReference(locationMap[C.Application.OutputDir]);
                }

                var pdbFileLoc = locationMap[CCompiler.PDBFile] as Opus.Core.ScaffoldLocation;
                if (!pdbFileLoc.IsValid)
                {
                    pdbFileLoc.SpecifyStub(pdbDirLoc, this.OutputName + ".pdb", Opus.Core.Location.EExists.WillExist);
                }
            }

            base.FinalizeOptions(node);
#else
            var options = this as ICCompilerOptions;

            if (options.DebugType != EDebugType.Embedded)
            {
                var pdbPathName = System.IO.Path.Combine(this.ProgamDatabaseDirectoryPath, this.OutputName) + ".pdb";
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