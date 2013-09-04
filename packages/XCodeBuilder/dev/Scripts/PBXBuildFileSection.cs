// <copyright file="PBXBuildFileSection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed class PBXBuildFileSection : IWriteableNode, System.Collections.IEnumerable
    {
        public PBXBuildFileSection()
        {
            this.BuildFiles = new System.Collections.Generic.List<PBXBuildFile>();
        }

        public void Add(PBXBuildFile buildFile)
        {
            lock (this.BuildFiles)
            {
                this.BuildFiles.Add(buildFile);
            }
        }

        private System.Collections.Generic.List<PBXBuildFile> BuildFiles
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
