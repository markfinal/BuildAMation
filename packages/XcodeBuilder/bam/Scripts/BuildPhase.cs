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
using System.Linq;
namespace XcodeBuilder
{
    public abstract class BuildPhase :
        Object
    {
        protected BuildPhase(
            Project project,
            string name,
            string isa,
            params string[] hashComponents)
            :
            base(project, name, isa, hashComponents) => this.BuildFiles = new Bam.Core.Array<BuildFile>();

        public void
        AddBuildFile(
            BuildFile other)
        {
            lock (this.BuildFiles)
            {
                var existingBuildFile = this.BuildFiles.FirstOrDefault(item => item.GUID.Equals(other.GUID, System.StringComparison.Ordinal));
                if (null == existingBuildFile)
                {
                    this.BuildFiles.Add(other);
                    other.Parent = this;
                }
            }
        }

        protected abstract string BuildActionMask { get; }
        protected abstract bool RunOnlyForDeploymentPostprocessing { get; }
        public Bam.Core.Array<BuildFile> BuildFiles { get; protected set; }

        public override void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            var indent3 = new string('\t', indentLevel + 2);
            text.AppendLine($"{indent}{this.GUID} /* {this.Name} */ = {{");
            text.AppendLine($"{indent2}isa = {this.IsA};");
            text.AppendLine($"{indent2}buildActionMask = {this.BuildActionMask};");
            if (this.BuildFiles.Any())
            {
                text.AppendLine($"{indent2}files = (");
                foreach (var file in this.BuildFiles)
                {
                    text.AppendLine($"{indent3}{file.GUID} /* {file.Name} in {this.Name} */,");
                }
                text.AppendLine($"{indent2});");
            }
            var deployment = this.RunOnlyForDeploymentPostprocessing ? "1" : "0";
            text.AppendLine($"{indent2}runOnlyForDeploymentPostprocessing = {deployment};");
            text.AppendLine($"{indent}}};");
        }
    }
}
