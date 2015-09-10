#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
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
            Bam.Core.Location location,
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
