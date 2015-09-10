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
    public sealed class PBXCopyFilesBuildPhaseSection :
        IWriteableNode,
        System.Collections.IEnumerable
    {
        public
        PBXCopyFilesBuildPhaseSection()
        {
            this.CopyFilesBuildPhases = new System.Collections.Generic.List<PBXCopyFilesBuildPhase>();
        }

        public void
        Add(
            PBXCopyFilesBuildPhase buildPhase)
        {
            lock (this.CopyFilesBuildPhases)
            {
                this.CopyFilesBuildPhases.Add(buildPhase);
            }
        }

        public PBXCopyFilesBuildPhase
        Get(
            string name,
            string moduleName)
        {
            lock (this.CopyFilesBuildPhases)
            {
                foreach (var buildPhase in this.CopyFilesBuildPhases)
                {
                    if ((buildPhase.Name == name) && (buildPhase.ModuleName == moduleName))
                    {
                        return buildPhase;
                    }
                }

                var newBuildPhase = new PBXCopyFilesBuildPhase(name, moduleName);
                this.CopyFilesBuildPhases.Add(newBuildPhase);
                return newBuildPhase;
            }
        }

        private System.Collections.Generic.List<PBXCopyFilesBuildPhase> CopyFilesBuildPhases
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (this.CopyFilesBuildPhases.Count == 0)
            {
                return;
            }

            var orderedList = new System.Collections.Generic.List<PBXCopyFilesBuildPhase>(this.CopyFilesBuildPhases);
            orderedList.Sort(
                delegate(PBXCopyFilesBuildPhase p1, PBXCopyFilesBuildPhase p2)
                {
                    return p1.UUID.CompareTo(p2.UUID);
                }
            );

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXCopyFilesBuildPhase section */");
            foreach (var buildPhase in orderedList)
            {
                (buildPhase as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXCopyFilesBuildPhase section */");
        }
#endregion

#region IEnumerable implementation

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.CopyFilesBuildPhases.GetEnumerator();
        }

#endregion
    }
}
