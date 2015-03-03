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
    public sealed class PBXBuildFileSection :
        IWriteableNode,
        System.Collections.IEnumerable
    {
        public
        PBXBuildFileSection()
        {
            this.BuildFiles = new Bam.Core.Array<PBXBuildFile>();
        }

        public PBXBuildFile
        Get(
            string name,
            PBXFileReference fileRef,
            BuildPhase buildPhase)
        {
            lock (this.BuildFiles)
            {
                foreach (var buildFile in this.BuildFiles)
                {
                    if ((buildFile.Name == name) &&
                        (buildFile.FileReference == fileRef) &&
                        (buildFile.BuildPhase == buildPhase))
                    {
                        return buildFile;
                    }
                }

                var newBuildFile = new PBXBuildFile(name, fileRef, buildPhase);
                this.BuildFiles.Add(newBuildFile);
                buildPhase.Files.AddUnique(newBuildFile);
                return newBuildFile;
            }
        }

        private Bam.Core.Array<PBXBuildFile> BuildFiles
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (this.BuildFiles.Count == 0)
            {
                return;
            }

            var orderedList = new System.Collections.Generic.List<PBXBuildFile>(this.BuildFiles);
            orderedList.Sort(
                delegate(PBXBuildFile p1, PBXBuildFile p2)
                {
                    return p1.UUID.CompareTo(p2.UUID);
                }
            );

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXBuildFile section */");
            foreach (var buildFile in orderedList)
            {
                (buildFile as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXBuildFile section */");
        }
#endregion

#region IEnumerable implementation

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.BuildFiles.GetEnumerator();
        }

#endregion
    }
}
