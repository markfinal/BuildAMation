// <copyright file="ObjCCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>LLVMGcc package</summary>
// <author>Mark Final</author>
namespace LLVMGcc
{
    // Not sealed since the ObjC++ compiler inherits from it
    public partial class ObjCCompilerOptionCollection : GccCommon.ObjCCompilerOptionCollection, ICCompilerOptions
    {
        public ObjCCompilerOptionCollection()
            : base()
        {
        }

        public ObjCCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            // requires gcc 4.0
            (this as ICCompilerOptions).Visibility = EVisibility.Hidden;
        }
    }
}
