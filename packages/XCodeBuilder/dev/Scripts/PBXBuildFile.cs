// <copyright file="PBXBuildFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
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

        public PBXSourcesBuildPhase BuildPhase
        {
            get;
            set;
        }

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
            if (this.FileReference == null)
            {
                throw new Opus.Core.Exception("File reference not set on this build file");
            }
            if (this.BuildPhase == null)
            {
                throw new Opus.Core.Exception("Build phase not set on this build file");
            }

            writer.WriteLine("\t\t{0} /* {1} in {2} */ = {{isa = PBXBuildFile; fileRef = {3} /* {1} */; }};", this.UUID, this.FileReference.ShortPath, this.BuildPhase.Name, this.FileReference.UUID);
        }

#endregion
    }
}
