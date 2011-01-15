// <copyright file="ToolchainOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualC package</summary>
// <author>Mark Final</author>
namespace VisualC
{
    public sealed partial class ToolchainOptionCollection : VisualCCommon.ToolchainOptionCollection
    {
        public ToolchainOptionCollection()
            : base()
        {
        }

        public ToolchainOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }
    }
}