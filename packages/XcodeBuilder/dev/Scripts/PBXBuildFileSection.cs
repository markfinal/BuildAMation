// <copyright file="PBXBuildFileSection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class PBXBuildFileSection : IWriteableNode, System.Collections.IEnumerable
    {
        public PBXBuildFileSection()
        {
            this.BuildFiles = new Opus.Core.Array<PBXBuildFile>();
        }

        public PBXBuildFile Get(string name, PBXFileReference fileRef)
        {
            lock (this.BuildFiles)
            {
                foreach (var buildFile in this.BuildFiles)
                {
                    if ((buildFile.Name == name) && (buildFile.FileReference == fileRef))
                    {
                        return buildFile;
                    }
                }

                var newBuildFile = new PBXBuildFile(name, fileRef);
                this.BuildFiles.Add(newBuildFile);
                return newBuildFile;
            }
        }

        private Opus.Core.Array<PBXBuildFile> BuildFiles
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void IWriteableNode.Write (System.IO.TextWriter writer)
        {
            if (this.BuildFiles.Count == 0)
            {
                return;
            }

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXBuildFile section */");
            foreach (var buildFile in this.BuildFiles)
            {
                (buildFile as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXBuildFile section */");
        }
#endregion

#region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return this.BuildFiles.GetEnumerator();
        }

#endregion
    }
}
