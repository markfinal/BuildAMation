// <copyright file="ToolchainOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Intel package</summary>
// <author>Mark Final</author>
namespace Intel
{
    public sealed partial class ToolchainOptionCollection : GccCommon.ToolchainOptionCollection
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
