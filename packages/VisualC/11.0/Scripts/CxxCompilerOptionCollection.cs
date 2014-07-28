// <copyright file="CxxCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualC package</summary>
// <author>Mark Final</author>
namespace VisualC
{
    public sealed partial class CxxCompilerOptionCollection :
        VisualCCommon.CxxCompilerOptionCollection
    {
        public
        CxxCompilerOptionCollection(
            Opus.Core.DependencyNode node) : base(node)
        {}
    }
}
