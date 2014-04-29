// <copyright file="ObjCCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
namespace Gcc
{
    // Not sealed since the ObjC++ compiler inherits from it
    public partial class ObjCCompilerOptionCollection : GccCommon.ObjCCompilerOptionCollection, ICCompilerOptions
    {
        public ObjCCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        protected override void SetDefaultOptionValues(Opus.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            // requires gcc 4.0
            (this as ICCompilerOptions).Visibility = EVisibility.Hidden;
        }
    }
}
