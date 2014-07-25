// <copyright file="CCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXECommon package</summary>
// <author>Mark Final</author>
namespace ComposerXECommon
{
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection :
        C.CompilerOptionCollection,
        C.ICCompilerOptions,
        ICCompilerOptions
    {
        protected override void
        SetDefaultOptionValues(
            Opus.Core.DependencyNode node)
        {
            var compilerInterface = this as ICCompilerOptions;
            compilerInterface.AllWarnings = true;
            compilerInterface.StrictDiagnostics = true;
            compilerInterface.EnableRemarks = true;

            base.SetDefaultOptionValues(node);

            var cOptions = this as C.ICCompilerOptions;

            // there is too much of a headache with include paths to enable this!
            cOptions.IgnoreStandardIncludePaths = false;

            var target = node.Target;
            compilerInterface.SixtyFourBit = Opus.Core.OSUtilities.Is64Bit(target);

            if (target.HasConfiguration(Opus.Core.EConfiguration.Debug))
            {
                compilerInterface.StrictAliasing = false;
                compilerInterface.InlineFunctions = false;
            }
            else
            {
                compilerInterface.StrictAliasing = true;
                compilerInterface.InlineFunctions = true;
            }

            compilerInterface.PositionIndependentCode = false;

            var compilerTool = target.Toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            cOptions.SystemIncludePaths.AddRange(compilerTool.IncludePaths((Opus.Core.BaseTarget)target));

            cOptions.TargetLanguage = C.ETargetLanguage.C;
        }

        public
        CCompilerOptionCollection(
            Opus.Core.DependencyNode node) : base(node)
        {}
    }
}
