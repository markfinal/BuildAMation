// <copyright file="CCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Clang package</summary>
// <author>Mark Final</author>
namespace Clang
{
    public partial class CCompilerOptionCollection : C.CompilerOptionCollection, C.ICCompilerOptions
    {
        public CCompilerOptionCollection()
        {
        }

        public CCompilerOptionCollection(Opus.Core.DependencyNode owningNode)
            : base(owningNode)
        {
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            return null;
        }

        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            // TODO
        }
    }
}
