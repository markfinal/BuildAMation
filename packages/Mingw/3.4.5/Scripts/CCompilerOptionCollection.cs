// <copyright file="CCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    public partial class CCompilerOptionCollection : MingwCommon.CCompilerOptionCollection
    {
        public CCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }
    }
}