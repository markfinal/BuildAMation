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
    public sealed class NativeCompilation :
        ICompilationPolicy
    {
        private static bool
        DeferredEvaluationRequiresBuild(
            ObjectFile sender)
        {
            var objectFileWriteTime = System.IO.File.GetLastWriteTime(sender.GeneratedPaths[C.ObjectFile.Key].Parse());
            foreach (var dep in sender.Dependents)
            {
                if (dep is C.HeaderFile)
                {
                    var dependencyWriteTime = System.IO.File.GetLastWriteTime((dep as C.HeaderFile).InputPath.Parse());
                    if (dependencyWriteTime > objectFileWriteTime)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        void
        ICompilationPolicy.Compile(
            ObjectFile sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString objectFilePath,
            Bam.Core.Module source)
        {
            if (!sender.PerformCompilation)
            {
                return;
            }
            if (sender.ReasonToExecute.Reason == Bam.Core.ExecuteReasoning.EReason.DeferredEvaluation)
            {
                if (!DeferredEvaluationRequiresBuild(sender))
                {
                    return;
                }
            }

            var objectFileDir = System.IO.Path.GetDirectoryName(objectFilePath.ToString());
            Bam.Core.IOWrapper.CreateDirectoryIfNotExists(objectFileDir);

            var commandLine = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(commandLine);
            commandLine.Add(source.GeneratedPaths[SourceFile.Key].ParseAndQuoteIfNecessary());
            CommandLineProcessor.Processor.Execute(context, sender.Tool as Bam.Core.ICommandLineTool, commandLine);
        }
    }
}
