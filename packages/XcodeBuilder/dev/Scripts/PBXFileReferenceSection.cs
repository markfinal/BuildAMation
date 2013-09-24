// <copyright file="PBXFileReferenceSection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class PBXFileReferenceSection : IWriteableNode
    {
        public PBXFileReferenceSection()
        {
            this.FileReferences = new System.Collections.Generic.List<PBXFileReference>();
        }

        public PBXFileReference Get(string name, PBXFileReference.EType type, string path, System.Uri rootPath)
        {
            lock (this.FileReferences)
            {
                foreach (var fileRef in this.FileReferences)
                {
                    if ((fileRef.Name == name) && (fileRef.Type == type) && (fileRef.ShortPath == System.IO.Path.GetFileName(path)))
                    {
                        Opus.Core.Log.MessageAll("Matched file ref {0} {1}", name, path);
                        return fileRef;
                    }
                }

                Opus.Core.Log.MessageAll("New file ref {0} {1}", name, path);
                var newFileRef = new PBXFileReference(name, type, path, rootPath);
                this.FileReferences.Add(newFileRef);
                return newFileRef;
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
