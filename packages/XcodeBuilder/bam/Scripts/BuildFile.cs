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
    public sealed class BuildFile :
        Object
    {
        public BuildFile(
            FileReference fileRef)
        {
            this.IsA = "PBXBuildFile";
            this.FileRef = fileRef;
        }

        private FileReference TheFileRef;
        public FileReference FileRef
        {
            get
            {
                return this.TheFileRef;
            }
            private set
            {
                this.TheFileRef = value;
                this.Name = System.IO.Path.GetFileName(value.Path.Parse());
            }
        }

        public Bam.Core.StringArray Settings
        {
            get;
            set;
        }

        public Object Parent
        {
            get;
            set;
        }

        public override void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            text.AppendFormat("{0}{1} /* {3} in {4} */ = {{isa = {5}; fileRef = {2} /* {3} */; ",
                indent, this.GUID, this.TheFileRef.GUID,
                this.Name,
                (null != this.Parent) ? this.Parent.Name : "Unknown",
                this.IsA);
            if (this.Settings != null)
            {
                text.AppendFormat("settings = {{COMPILER_FLAGS = \"{0}\"; }}; ", this.Settings.ToString(' '));
            }
            text.AppendFormat("}};", indent, this.GUID, this.TheFileRef.GUID);
            text.AppendLine();
        }
    }
}
