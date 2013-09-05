// <copyright file="PBXFileReference.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed class PBXFileReference : XCodeNodeData, IWriteableNode
    {
        public PBXFileReference(string name, string path)
            : base(name)
        {
            // TODO: should this always be stripped?
            this.Path = System.IO.Path.GetFileName(path);
        }

        public string Path
        {
            get;
            private set;
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
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; explicitFileType = \"compiled.mach-o.executable\"; includeInIndex = 0; path = {2}; sourceTree = BUILT_PRODUCTS_DIR; }};", this.UUID, this.Name, this.Path);
            }
            else if (this.IsSourceCode)
            {
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; lastKnownFileType = sourcecode.cpp.cpp; path = {2}; sourceTree = \"<group>\"; }};", this.UUID, this.Name, this.Path);
            }
            else
            {
                throw new Opus.Core.Exception("Unknown PBXFileReference type");
            }
        }

#endregion
    }
}
