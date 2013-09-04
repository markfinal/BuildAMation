// <copyright file="PBXBuildFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed class PBXBuildFile : XCodeNodeData, IWriteableNode
    {
        public PBXBuildFile(string name)
            : base(name)
        {}

        public PBXFileReference FileReference
        {
            get;
            set;
        }

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
            writer.WriteLine("\t\t{0} /* {1} in TODO */ = {{ isa = PBXBuildFile; fileRef = {2} /* {1} */; }};", this.UUID, this.FileReference.Path, this.FileReference.UUID);
        }

#endregion
    }
}
