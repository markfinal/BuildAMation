// <copyright file="CCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection :
        C.CompilerOptionCollection,
        C.ICCompilerOptions,
        ICCompilerOptions,
        VisualStudioProcessor.IVisualStudioSupport
    {
        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            var target = node.Target;

            var compilerInterface = this as ICCompilerOptions;
            compilerInterface.NoLogo = true;

            if (target.HasConfiguration(Bam.Core.EConfiguration.Debug))
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
            (this as C.ICCompilerOptions).SystemIncludePaths.AddRange(compilerTool.IncludePaths((Bam.Core.BaseTarget)target));

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
            compilerInterface.ForcedInclude = new Bam.Core.StringArray();
        }

        public
        CCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        public override void
        FinalizeOptions(
            Bam.Core.DependencyNode node)
        {
            var options = this as ICCompilerOptions;
            if (options.DebugType != EDebugType.Embedded)
            {
                var locationMap = node.Module.Locations;
                var pdbDirLoc = locationMap[CCompiler.PDBDir] as Bam.Core.ScaffoldLocation;
                if (!pdbDirLoc.IsValid)
                {
                    pdbDirLoc.SetReference(locationMap[C.Application.OutputDir]);
                }

                var pdbFileLoc = locationMap[CCompiler.PDBFile] as Bam.Core.ScaffoldLocation;
                if (!pdbFileLoc.IsValid)
                {
                    pdbFileLoc.SpecifyStub(pdbDirLoc, this.OutputName + ".pdb", Bam.Core.Location.EExists.WillExist);
                }
            }

            base.FinalizeOptions(node);
        }

        VisualStudioProcessor.ToolAttributeDictionary
        VisualStudioProcessor.IVisualStudioSupport.ToVisualStudioProjectAttributes(
            Bam.Core.Target target)
        {
            var vsTarget = (target.Toolset as VisualStudioProcessor.IVisualStudioTargetInfo).VisualStudioTarget;
            switch (vsTarget)
            {
                case VisualStudioProcessor.EVisualStudioTarget.VCPROJ:
                case VisualStudioProcessor.EVisualStudioTarget.MSBUILD:
                    break;

                default:
                    throw new Bam.Core.Exception("Unsupported VisualStudio target, '{0}'", vsTarget);
            }
            var dictionary = VisualStudioProcessor.ToVisualStudioAttributes.Execute(this, target, vsTarget);
            return dictionary;
        }
    }
}
