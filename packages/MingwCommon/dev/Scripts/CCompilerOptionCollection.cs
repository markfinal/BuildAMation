// <copyright file="CCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection :
        C.CompilerOptionCollection,
        C.ICCompilerOptions,
        ICCompilerOptions
    {
        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            var localCompilerOptions = this as ICCompilerOptions;
            localCompilerOptions.AllWarnings = true;
            localCompilerOptions.ExtraWarnings = true;

            base.SetDefaultOptionValues(node);

            var target = node.Target;
            localCompilerOptions.SixtyFourBit = target.HasPlatform(Bam.Core.EPlatform.Win64);

            if (target.HasConfiguration(Bam.Core.EConfiguration.Debug))
            {
                localCompilerOptions.StrictAliasing = false;
                localCompilerOptions.InlineFunctions = false;
            }
            else
            {
                localCompilerOptions.StrictAliasing = true;
                localCompilerOptions.InlineFunctions = true;
            }

            var cCompilerOptions = this as C.ICCompilerOptions;
            cCompilerOptions.TargetLanguage = C.ETargetLanguage.C;

            var toolset = target.Toolset;
            var compilerTool = toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            cCompilerOptions.SystemIncludePaths.AddRange(compilerTool.IncludePaths((Bam.Core.BaseTarget)node.Target));

            localCompilerOptions.Pedantic = true;
        }

        public
        CCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
