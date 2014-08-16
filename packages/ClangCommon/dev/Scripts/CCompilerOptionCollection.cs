// <copyright file="CCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ClangCommon package</summary>
// <author>Mark Final</author>
namespace ClangCommon
{
    public partial class CCompilerOptionCollection :
        C.CompilerOptionCollection,
        C.ICCompilerOptions,
        ICCompilerOptions
    {
        public
        CCompilerOptionCollection(
            Bam.Core.DependencyNode owningNode) : base(owningNode)
        {}

        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            // preferrable for Clang to find the include paths
            (this as C.ICCompilerOptions).IgnoreStandardIncludePaths = false;

            var clangOptions = this as ICCompilerOptions;
            clangOptions.PositionIndependentCode = false;

            var target = node.Target;
            clangOptions.SixtyFourBit = Bam.Core.OSUtilities.Is64Bit(target);

            // use C99 by default with clang
            if (!(this is C.ICxxCompilerOptions))
            {
                (this as C.ICCompilerOptions).LanguageStandard = C.ELanguageStandard.C99;
            }
        }
    }
}
