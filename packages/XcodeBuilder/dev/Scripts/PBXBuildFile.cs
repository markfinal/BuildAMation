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
    public sealed class PBXBuildFile :
        XcodeNodeData,
        IWriteableNode
    {
        public
        PBXBuildFile(
            string name,
            PBXFileReference fileRef,
            BuildPhase buildPhase) : base(name)
        {
            this.FileReference = fileRef;
            this.Settings = new OptionsDictionary();
            this.BuildPhase = buildPhase;
        }

        public PBXFileReference FileReference
        {
            get;
            private set;
        }

        public BuildPhase BuildPhase
        {
            get;
            private set;
        }

        public OptionsDictionary Settings
        {
            get;
            private set;
        }

#region IWriteableNode implementation

        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (this.FileReference == null)
            {
                throw new Bam.Core.Exception("File reference not set on this build file");
            }
            if (this.BuildPhase == null)
            {
                throw new Bam.Core.Exception("Build phase not set on this build file");
            }

            if (this.Settings.Count > 0)
            {
                writer.WriteLine("\t\t{0} /* {1} in {2} */ = {{isa = PBXBuildFile; fileRef = {3} /* {1} */; settings = {4} }};", this.UUID, this.FileReference.ShortPath, this.BuildPhase.Name, this.FileReference.UUID, this.Settings.ToString());
            }
            else
            {
                writer.WriteLine("\t\t{0} /* {1} in {2} */ = {{isa = PBXBuildFile; fileRef = {3} /* {1} */; }};", this.UUID, this.FileReference.ShortPath, this.BuildPhase.Name, this.FileReference.UUID);
            }
        }

#endregion
    }
}
