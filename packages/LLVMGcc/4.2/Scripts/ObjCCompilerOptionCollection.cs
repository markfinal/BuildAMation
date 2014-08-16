// <copyright file="ObjCCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>LLVMGcc package</summary>
// <author>Mark Final</author>
namespace LLVMGcc
{
    // Not sealed since the ObjC++ compiler inherits from it
    public partial class ObjCCompilerOptionCollection :
        GccCommon.ObjCCompilerOptionCollection,
        ICCompilerOptions
    {
        public
        ObjCCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            // requires gcc 4.0
            (this as ICCompilerOptions).Visibility = EVisibility.Hidden;
        }
    }
}
