// <copyright file="LinkerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Intel package</summary>
// <author>Mark Final</author>
namespace Intel
{
    public sealed partial class LinkerOptionCollection : GccCommon.LinkerOptionCollection
    {
        public LinkerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }
    }
}