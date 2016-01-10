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
namespace Publisher
{
    public sealed class NativeCollatedObject :
        ICollatedObjectPolicy
    {
        void
        ICollatedObjectPolicy.Collate(
            CollatedObject sender,
            Bam.Core.ExecutionContext context)
        {
            if (sender is CollatedFile)
            {
                if (!(sender as CollatedFile).FailWhenSourceDoesNotExist)
                {
                    var source = sender.SourcePath.Parse();
                    if (!System.IO.File.Exists(source))
                    {
                        Bam.Core.Log.Detail("File {0} cannot be copied as it does not exist. Ignoring.", source);
                        return;
                    }
                }
            }

            var isSymLink = (sender is CollatedSymbolicLink);
            var sourcePath = isSymLink ? sender.Macros["LinkTarget"] : sender.SourcePath;

            var destinationPath = isSymLink ? sender.GeneratedPaths[CollatedObject.Key].Parse() : sender.Macros["CopyDir"].Parse();

            if (!isSymLink)
            {
                // synchronize, so that multiple modules don't try to create the same directories simultaneously
                lock ((sender.Reference != null) ? sender.Reference : sender)
                {
                    if (!System.IO.Directory.Exists(destinationPath))
                    {
                        System.IO.Directory.CreateDirectory(destinationPath);
                    }
                }
            }

            var copySource = sourcePath.ParseAndQuoteIfNecessary();
            if (sender is CollatedDirectory && sender.Tool is CopyFilePosix & sender.Macros["CopiedFilename"].IsAliased)
            {
                copySource = System.String.Format("{0}/*", copySource);
            }

            var commandLine = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(commandLine);

            commandLine.Add(copySource);
            commandLine.Add(destinationPath);
            CommandLineProcessor.Processor.Execute(context, sender.Tool as Bam.Core.ICommandLineTool, commandLine);
        }
    }
}
