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
using Bam.Core;
namespace Publisher
{
    public sealed class CollatedSymbolicLink :
        CollatedObject
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<MakeLinkWin>();
            }
            else
            {
                this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<MakeLinkPosix>();
            }
        }

        public void
        AssignLinkTarget(
            TokenizedString path = null)
        {
#if __MonoCS__
            if (path == null)
            {
                var symlink = new Mono.Unix.UnixSymbolicLinkInfo(this.SourcePath.Parse());
                this.Macros["LinkTarget"] = Bam.Core.TokenizedString.CreateVerbatim(symlink.ContentsPath);
            }
            else
            {
                this.Macros["LinkTarget"] = path;
            }
#else
            throw new System.NotSupportedException("Unable to get symbolic link target on Windows");
#endif
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
            var copiedPath = this.GeneratedPaths[Key].Parse();
            var exists = System.IO.File.Exists(copiedPath);
            if (!exists)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
                return;
            }
        }
    }
}
