// <copyright file="PBXFileReferenceSection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed class PBXFileReferenceSection : IWriteableNode
    {
        public PBXFileReferenceSection()
        {
            this.FileReferences = new System.Collections.Generic.List<PBXFileReference>();
        }

        public void Add(PBXFileReference fileRef)
        {
            lock (this.FileReferences)
            {
                this.FileReferences.Add(fileRef);
            }
        }

        private System.Collections.Generic.List<PBXFileReference> FileReferences
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void IWriteableNode.Write (System.IO.TextWriter writer)
        {
            if (this.FileReferences.Count == 0)
            {
                return;
            }

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXFileReference section */");
            foreach (var FileRef in this.FileReferences)
            {
                (FileRef as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXFileReference section */");
        }
#endregion
    }
}
