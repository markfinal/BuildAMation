#region License
// Copyright (c) 2010-2018, Mark Final
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
    public class CollatedFile :
        CollatedObject
    {
        protected override void
        EvaluateInternal()
        {
            this.ReasonToExecute = null;
            var copiedPath = this.GeneratedPaths[Key].ToString();
            var exists = System.IO.File.Exists(copiedPath);
            if (!exists)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
                return;
            }
            var sourceModule = (this as ICollatedObject).SourceModule;
            if (null != sourceModule)
            {
                if (null != sourceModule.EvaluationTask)
                {
                    sourceModule.EvaluationTask.Wait();
                }
                if (null != sourceModule.ReasonToExecute && null != sourceModule.ReasonToExecute.OutputFilePath)
                {
                    if (sourceModule.ReasonToExecute.OutputFilePath.ToString().Equals(this.SourcePath.ToString()))
                    {
                        this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], this.SourcePath);
                        return;
                    }
                    else
                    {
                        // there may be multiple files used as a source of a copy - not just that file which was the primary build output
                        var destinationLastWriteTime = System.IO.File.GetLastWriteTime(copiedPath);
                        var sourceLastWriteTime = System.IO.File.GetLastWriteTime(this.SourcePath.ToString());
                        if (sourceLastWriteTime > destinationLastWriteTime)
                        {
                            this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], this.SourcePath);
                            return;
                        }
                    }
                }
            }

            var destLastWriteTime = System.IO.File.GetLastWriteTime(copiedPath);
            var srcLastWriteTime = System.IO.File.GetLastWriteTime(this.SourcePath.ToString());
            if (srcLastWriteTime > destLastWriteTime)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], this.SourcePath);
                return;
            }
        }
    }
}
