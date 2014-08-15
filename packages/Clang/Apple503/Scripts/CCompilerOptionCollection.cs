// <copyright file="CCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Clang package</summary>
// <author>Mark Final</author>
namespace Clang
{
    public partial class CCompilerOptionCollection :
        ClangCommon.CCompilerOptionCollection,
        C.ICCompilerOptionsOSX
    {
        public
        CCompilerOptionCollection(
            Opus.Core.DependencyNode owningNode) : base(owningNode)
        {}

        protected override void
        SetDefaultOptionValues(
            Opus.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            var options = this as C.ICCompilerOptionsOSX;
            options.FrameworkSearchDirectories = new Opus.Core.DirectoryCollection();
        }
    }
}
