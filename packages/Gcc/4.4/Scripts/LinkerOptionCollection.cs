// <copyright file="LinkerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
namespace Gcc
{
    public sealed partial class LinkerOptionCollection : GccCommon.LinkerOptionCollection
    {
        public LinkerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }
    }
}