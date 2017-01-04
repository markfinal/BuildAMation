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
using Bam.Core;
namespace C
{
    public sealed class NativeProceduralHeaderFromToolOutput :
        IProceduralHeaderFromToolOutputPolicy
    {
        void
        IProceduralHeaderFromToolOutputPolicy.HeaderFromToolOutput(
            ProceduralHeaderFileFromToolOutput sender,
            ExecutionContext context,
            TokenizedString outputPath,
            ICommandLineTool tool)
        {
            var toolPath = tool.Executable.Parse();
            CommandLineProcessor.Processor.Execute(context, toolPath);

            var destPath = outputPath.Parse();
            var destDir = System.IO.Path.GetDirectoryName(destPath);
            Bam.Core.IOWrapper.CreateDirectoryIfNotExists(destDir);

            var tempPath = System.IO.Path.GetTempFileName();
            using (System.IO.TextWriter writeFile = new System.IO.StreamWriter(tempPath))
            {
                writeFile.Write(context.OutputStringBuilder.ToString());
            }

            var moveFile = true;
            if (System.IO.File.Exists(destPath))
            {
                // compare contents
                using (System.IO.TextReader existingFile = new System.IO.StreamReader(destPath))
                {
                    var contents = existingFile.ReadToEnd();
                    var contentsL = contents.Length;
                    var oldL = context.OutputStringBuilder.ToString().Length;
                    if (contents.Equals(context.OutputStringBuilder.ToString()))
                    {
                        moveFile = false;
                    }
                }
            }
            if (moveFile)
            {
                Bam.Core.Log.Info("Written procedurally generated header : {0}, from the output of {1}", destPath, toolPath);
                System.IO.File.Delete(destPath);
                System.IO.File.Move(tempPath, destPath);
            }
            else
            {
                Bam.Core.Log.Info("{0} contents have not changed", destPath);
            }
            context.OutputStringBuilder.Clear();
        }
    }
}
