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
namespace Publisher
{
    public sealed class MakeFileCollatedFile :
        ICollatedFilePolicy
    {
        void
        ICollatedFilePolicy.Collate(
            CollatedFile sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString packageRoot)
        {
            var sourcePath = sender.SourcePath;
            var sourceFilename = System.IO.Path.GetFileName(sourcePath.Parse());

            var meta = new MakeFileBuilder.MakeFileMeta(sender);
            var rule = meta.AddRule();

            var destinationPath = (sender.Reference != null) ? sender.Reference.DestinationDirectory : packageRoot.Parse();
            if (null != sender.SubDirectory)
            {
                destinationPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(destinationPath, sender.SubDirectory));
            }
            destinationPath += System.IO.Path.DirectorySeparatorChar;
            sender.DestinationDirectory = destinationPath;
            meta.CommonMetaData.Directories.AddUnique(destinationPath);

            rule.AddTarget(Bam.Core.TokenizedString.Create(destinationPath + sourceFilename, null, verbatim: true), variableName: "CopyFile_" + sourceFilename);

            var commandLine = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(sender, commandLine);

            rule.AddShellCommand(System.String.Format(@"{0} {1} $< $(dir $@)", (sender.Tool as Bam.Core.ICommandLineTool).Executable, commandLine.ToString(' ')));
            rule.AddPrerequisite(sourcePath);
        }
    }
}
