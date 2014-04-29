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

            Opus.Core.Target target = node.Target;

            ICCompilerOptions compilerInterface = this as ICCompilerOptions;
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

            C.ICompilerTool compilerTool = target.Toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
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

            this.ProgamDatabaseDirectoryPath = this.OutputDirectoryPath.Clone() as string;
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

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
            ICCompilerOptions options = this as ICCompilerOptions;

            if (options.DebugType != EDebugType.Embedded)
            {
                string pdbPathName = System.IO.Path.Combine(this.ProgamDatabaseDirectoryPath, this.OutputName) + ".pdb";
                this.ProgramDatabaseFilePath = pdbPathName;
            }

            base.FinalizeOptions(node);
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            if (this.OutputPaths.Has(C.OutputFileFlags.ObjectFile))
            {
                string objPathName = this.ObjectFilePath;
                directoriesToCreate.Add(System.IO.Path.GetDirectoryName(objPathName));
            }

            if (this.OutputPaths.Has(C.OutputFileFlags.CompilerProgramDatabase))
            {
                string pdbPathName = this.ProgramDatabaseFilePath;
                directoriesToCreate.Add(System.IO.Path.GetDirectoryName(pdbPathName));
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