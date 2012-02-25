// <copyright file="ArchiverOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Intel package</summary>
// <author>Mark Final</author>
namespace Intel
{
    public sealed partial class ArchiverOptionCollection : GccCommon.ArchiverOptionCollection
    {
        public ArchiverOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }
    }
}