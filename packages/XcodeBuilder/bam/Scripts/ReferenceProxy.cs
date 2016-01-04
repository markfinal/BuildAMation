#region License
// Copyright (c) 2010-2016, Mark Final
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
    public sealed class ReferenceProxy :
        Object
    {
        public ReferenceProxy(
            Project project,
            FileReference.EFileType fileType,
            Bam.Core.TokenizedString path,
            Object remoteRef,
            FileReference.ESourceTree sourceTree)
        {
            this.IsA = "PBXReferenceProxy";
            this.Name = "PBXReferenceProxy";
            this.FileType = fileType;
            this.Path = path;
            this.RemoteRef = remoteRef;
            this.SourceTree = sourceTree;

            project.ReferenceProxies.AddUnique(this);
        }

        public FileReference.EFileType FileType
        {
            get;
            private set;
        }

        public Bam.Core.TokenizedString Path
        {
            get;
            private set;
        }

        public Object RemoteRef
        {
            get;
            private set;
        }

        public FileReference.ESourceTree SourceTree
        {
            get;
            private set;
        }

        public override void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            if (null != this.Name)
            {
                text.AppendFormat("{0}{1} /* {2} */ = {{", indent, this.GUID, this.Name);
            }
            else
            {
                text.AppendFormat("{0}{1} = {{", indent, this.GUID);
            }
            text.AppendLine();
            text.AppendFormat("{0}isa = {1};", indent2, this.IsA);
            text.AppendLine();
            text.AppendFormat("{0}fileType = {1};", indent2, this.FileType.AsString());
            text.AppendLine();
            text.AppendFormat("{0}path = {1};", indent2, this.Path);
            text.AppendLine();
            text.AppendFormat("{0}remoteRef = {1} /* {2} */;", indent2, this.RemoteRef.GUID, this.RemoteRef.Name);
            text.AppendLine();
            text.AppendFormat("{0}sourceTree = {1};", indent2, this.SourceTree.AsString());
            text.AppendLine();
            text.AppendFormat("{0}}};", indent);
            text.AppendLine();
        }
    }
}
