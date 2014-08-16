// <copyright file="CCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>LLVMGcc package</summary>
// <author>Mark Final</author>
namespace LLVMGcc
{
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection :
        GccCommon.CCompilerOptionCollection,
        ICCompilerOptions
    {
        public
        CCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            // requires gcc 4.0
            (this as ICCompilerOptions).Visibility = EVisibility.Hidden;

            // use C99 by default with llvm-gcc
            if (!(this is C.ICxxCompilerOptions))
            {
                (this as C.ICCompilerOptions).LanguageStandard = C.ELanguageStandard.C99;
            }
        }
    }
}
