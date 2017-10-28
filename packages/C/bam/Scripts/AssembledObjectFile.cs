#region License
// Copyright (c) 2010-2017, Mark Final
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
namespace C
{
    /// <summary>
    /// Object file assembled from preprocessed assembly.
    /// </summary>
    public class AssembledObjectFile :
        ObjectFileBase
    {
        private IAssemblerPolicy Policy = null;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.Assembler = DefaultToolchain.Assembler(this.BitDepth);
        }

        public AssemblerTool Assembler
        {
            get
            {
                return this.Tool as AssemblerTool;
            }
            set
            {
                this.Tool = value;
            }
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            var sourceFile = this.SourceModule;
            var objectFile = this.GeneratedPaths[Key];
            this.Policy.Assemble(this, context, objectFile, sourceFile);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            var className = "C." + mode + "Assembly";
            this.Policy = Bam.Core.ExecutionPolicyUtilities<IAssemblerPolicy>.Create(className);
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
            if (!this.PerformCompilation)
            {
                return;
            }
            // does the object file exist?
            var objectFilePath = this.GeneratedPaths[Key].ToString();
            if (!System.IO.File.Exists(objectFilePath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
                return;
            }
            var objectFileWriteTime = System.IO.File.GetLastWriteTime(objectFilePath);

            // has the source file been evaluated to be rebuilt?
            if ((this as IRequiresSourceModule).Source.ReasonToExecute != null)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], this.InputPath);
                return;
            }

            // is the source file newer than the object file?
            var sourcePath = this.InputPath.ToString();
            var sourceWriteTime = System.IO.File.GetLastWriteTime(sourcePath);
            if (sourceWriteTime > objectFileWriteTime)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], this.InputPath);
                return;
            }
        }
    }
}
