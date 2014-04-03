// <copyright file="CCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualC package</summary>
// <author>Mark Final</author>
namespace VisualC
{
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection : VisualCCommon.CCompilerOptionCollection
    {
        public CCompilerOptionCollection()
            : base()
        {
        }

        public CCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }
    }
}