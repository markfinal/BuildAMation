// <copyright file="LinkerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    public class LinkerOptionCollection :
        MingwCommon.LinkerOptionCollection
    {
        public
        LinkerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
