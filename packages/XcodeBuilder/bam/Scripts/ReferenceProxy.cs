#region License
// Copyright (c) 2010-2019, Mark Final
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
    /// <summary>
    /// Class representing a PBXReferenceProxy in an Xcode project
    /// </summary>
    sealed class ReferenceProxy :
        Object
    {
        /// <summary>
        /// Create an instance
        /// </summary>
        /// <param name="project">Project to add the proxy to</param>
        /// <param name="fileType">File type of the file reference</param>
        /// <param name="path">TokenizedString for the file reference</param>
        /// <param name="remoteRef">Remote reference object</param>
        /// <param name="sourceTree">Source tree</param>
        public ReferenceProxy(
            Project project,
            FileReference.EFileType fileType,
            Bam.Core.TokenizedString path,
            Object remoteRef,
            FileReference.ESourceTree sourceTree)
            :
            base(project, null, "PBXReferenceProxy", project.GUID, fileType.ToString(), path.ToString(), remoteRef.GUID, sourceTree.ToString())
        {
            this.FileType = fileType;
            this.Path = path;
            this.RemoteRef = remoteRef;
            this.SourceTree = sourceTree;

            project.AppendReferenceProxy(this);
        }

        /// <summary>
        /// Get the file type
        /// </summary>
        public FileReference.EFileType FileType { get; private set; }

        /// <summary>
        /// Get the path of the file reference
        /// </summary>
        public Bam.Core.TokenizedString Path { get; private set; }

        /// <summary>
        /// Get the remote reference
        /// </summary>
        public Object RemoteRef { get; private set; }

        /// <summary>
        /// Get the source tree
        /// </summary>
        public FileReference.ESourceTree SourceTree { get; private set; }

        /// <summary>
        /// Seralize the proxy.
        /// </summary>
        /// <param name="text">StringBuilder to write to</param>
        /// <param name="indentLevel">Number of tabs to indent by</param>
        public override void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            if (null != this.Name)
            {
                text.AppendLine($"{indent}{this.GUID} /* {this.Name} */ = {{");
            }
            else
            {
                text.AppendLine($"{indent}{this.GUID} = {{");
            }
            text.AppendLine($"{indent2}isa = {this.IsA};");
            text.AppendLine($"{indent2}fileType = {this.FileType.AsString()};");
            text.AppendLine($"{indent2}path = {this.Path.ToStringQuoteIfNecessary()};");
            text.AppendLine($"{indent2}remoteRef = {this.RemoteRef.GUID} /* {this.RemoteRef.Name} */;");
            text.AppendLine($"{indent2}sourceTree = {this.SourceTree.AsString()};");
            text.AppendLine($"{indent}}};");
        }
    }
}
