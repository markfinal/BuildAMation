#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
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
