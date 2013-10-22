// <copyright file="PBXBuildFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class PBXBuildFile : XCodeNodeData, IWriteableNode
    {
        public PBXBuildFile(string name, PBXFileReference fileRef, BuildPhase buildPhase)
            : base(name)
        {
            this.FileReference = fileRef;
            this.Settings = new OptionsDictionary();
            this.BuildPhase = buildPhase;
        }

        public PBXFileReference FileReference
        {
            get;
            private set;
        }

        public BuildPhase BuildPhase
        {
            get;
            private set;
        }

        public OptionsDictionary Settings
        {
            get;
            private set;
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

            if (this.Settings.Count > 0)
            {
                writer.WriteLine("\t\t{0} /* {1} in {2} */ = {{isa = PBXBuildFile; fileRef = {3} /* {1} */; settings = {4} }};", this.UUID, this.FileReference.ShortPath, this.BuildPhase.Name, this.FileReference.UUID, this.Settings.ToString());
            }
            else
            {
                writer.WriteLine("\t\t{0} /* {1} in {2} */ = {{isa = PBXBuildFile; fileRef = {3} /* {1} */; }};", this.UUID, this.FileReference.ShortPath, this.BuildPhase.Name, this.FileReference.UUID);
            }
        }

#endregion
    }
}
