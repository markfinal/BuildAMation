// <copyright file="CPlusPlusCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualC package</summary>
// <author>Mark Final</author>
namespace VisualC
{
    public sealed partial class CPlusPlusCompilerOptionCollection : VisualCCommon.CPlusPlusCompilerOptionCollection
    {
        public CPlusPlusCompilerOptionCollection()
            : base()
        {
        }

        public CPlusPlusCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }
    }
}
