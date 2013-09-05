// <copyright file="PBXFileReference.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed class PBXFileReference : XCodeNodeData, IWriteableNode
    {
        public PBXFileReference(string name, string path, System.Uri rootPath)
            : base(name)
        {
            // TODO: should this always be stripped?
            this.ShortPath = System.IO.Path.GetFileName(path);

            var relative = Opus.Core.RelativePathUtilities.GetPath(path, rootPath);
            this.RelativePath = relative;
            Opus.Core.Log.MessageAll("path {0}: {1}", path, relative);
        }

        public string ShortPath
        {
            get;
            private set;
        }

        private string RelativePath
        {
            get;
            set;
        }

        public bool IsExecutable
        {
            get;
            set;
        }

        public bool IsSourceCode
        {
            get;
            set;
        }

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
            if (this.IsExecutable)
            {
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; explicitFileType = \"compiled.mach-o.executable\"; includeInIndex = 0; path = {2}; sourceTree = BUILT_PRODUCTS_DIR; }};", this.UUID, this.Name, this.ShortPath);
            }
            else if (this.IsSourceCode)
            {
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; lastKnownFileType = sourcecode.c; path = {2}; sourceTree = SOURCE_ROOT; }};", this.UUID, this.ShortPath, this.RelativePath);
            }
            else
            {
                throw new Opus.Core.Exception("Unknown PBXFileReference type");
            }
        }

#endregion
    }
}
