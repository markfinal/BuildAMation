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
