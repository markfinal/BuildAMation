// <copyright file="CCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ClangCommon package</summary>
// <author>Mark Final</author>
namespace ClangCommon
{
    public partial class CCompilerOptionCollection : C.CompilerOptionCollection, C.ICCompilerOptions, ICCompilerOptions
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

            var clangOptions = this as ICCompilerOptions;
            clangOptions.PositionIndependentCode = false;

            var target = node.Target;
            clangOptions.SixtyFourBit = Opus.Core.OSUtilities.Is64Bit(target);

            // use C99 by default with clang
            if (!(this is C.ICxxCompilerOptions))
            {
                (this as C.ICCompilerOptions).LanguageStandard = C.ELanguageStandard.C99;
            }
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
