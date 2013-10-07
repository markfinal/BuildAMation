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

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            // preferrable for Clang to find the include paths
            (this as C.ICCompilerOptions).IgnoreStandardIncludePaths = false;
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            var directoriesToCreate = new Opus.Core.DirectoryCollection();

            var objPathName = this.ObjectFilePath;
            if (null != objPathName)
            {
                directoriesToCreate.AddAbsoluteDirectory(System.IO.Path.GetDirectoryName(objPathName), false);
            }

            return directoriesToCreate;
        }
    }
}
