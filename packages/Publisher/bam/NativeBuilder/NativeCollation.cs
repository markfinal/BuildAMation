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
    public sealed class NativeCollation :
        ICollationPolicy
    {
        static private void
        CopyFile(
            Collation sender,
            Bam.Core.ExecutionContext context,
            string sourcePath,
            string destinationDir)
        {
            // TODO: convert this to a command line tool as well
            // but it would require a module to have more than one tool
            if (!System.IO.Directory.Exists(destinationDir))
            {
                System.IO.Directory.CreateDirectory(destinationDir);
            }
            var destinationPath = System.IO.Path.Combine(destinationDir, System.IO.Path.GetFileName(sourcePath));

            var commandLine = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(sender, commandLine);

            commandLine.Add(sourcePath);
            commandLine.Add(destinationPath);
            CommandLineProcessor.Processor.Execute(context, sender.Tool as Bam.Core.ICommandLineTool, commandLine);
        }

        void
        ICollationPolicy.Collate(
            Collation sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString packageRoot,
            System.Collections.ObjectModel.ReadOnlyDictionary<Bam.Core.Module, System.Collections.Generic.Dictionary<Bam.Core.TokenizedString, PackageReference>> packageObjects)
        {
            var root = packageRoot.Parse();
            foreach (var module in packageObjects)
            {
                foreach (var path in module.Value)
                {
                    var sourcePath = path.Key.ToString();
                    if (path.Value.IsMarker)
                    {
                        var destinationDir = root;
                        if (null != path.Value.SubDirectory)
                        {
                            destinationDir = System.IO.Path.Combine(destinationDir, path.Value.SubDirectory);
                        }
                        CopyFile(sender, context, sourcePath, destinationDir);
                        path.Value.DestinationDir = destinationDir;
                    }
                    else
                    {
                        var subdir = path.Value.SubDirectory;
                        foreach (var reference in path.Value.References)
                        {
                            var destinationDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(reference.DestinationDir, path.Value.SubDirectory));
                            CopyFile(sender, context, sourcePath, destinationDir);
                            path.Value.DestinationDir = destinationDir;
                        }
                    }
                }
            }
        }
    }
}
