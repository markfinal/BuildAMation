// <copyright file="PBXFileReferenceSection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class PBXFileReferenceSection :
        IWriteableNode
    {
        public
        PBXFileReferenceSection()
        {
            this.FileReferences = new System.Collections.Generic.List<PBXFileReference>();
        }

        public PBXFileReference
        Get(
            string name,
            PBXFileReference.EType type,
            string path,
            System.Uri rootPath)
        {
            lock (this.FileReferences)
            {
                foreach (var fileRef in this.FileReferences)
                {
                    var shortPath = PBXFileReference.CalculateShortPath(type, path);
                    if ((fileRef.Name == name) && (fileRef.Type == type) && (fileRef.ShortPath == shortPath))
                    {
                        return fileRef;
                    }
                }

                var newFileRef = new PBXFileReference(name, type, path, rootPath);
                this.FileReferences.Add(newFileRef);
                return newFileRef;
            }
        }

        public PBXFileReference
        Get(
            string name,
            PBXFileReference.EType type,
            Opus.Core.Location location,
            System.Uri rootPath)
        {
            var path = location.GetSinglePath();
            return this.Get(name, type, path, rootPath);
        }

        private System.Collections.Generic.List<PBXFileReference> FileReferences
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (this.FileReferences.Count == 0)
            {
                return;
            }

            var orderedList = new System.Collections.Generic.List<PBXFileReference>(this.FileReferences);
            orderedList.Sort(
                delegate(PBXFileReference p1, PBXFileReference p2)
                {
                    return p1.UUID.CompareTo(p2.UUID);
                }
            );

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXFileReference section */");
            foreach (var FileRef in orderedList)
            {
                (FileRef as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXFileReference section */");
        }
#endregion
    }
}
