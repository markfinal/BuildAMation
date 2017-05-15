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
namespace Publisher
{
    public sealed class MakeFileCollatedObject :
        ICollatedObjectPolicy
    {
        void
        ICollatedObjectPolicy.Collate(
            CollatedObject sender,
            Bam.Core.ExecutionContext context)
        {
            var meta = new MakeFileBuilder.MakeFileMeta(sender);
            var rule = meta.AddRule();

            var sourcePath = sender.SourcePath;
            var sourceFilename = System.IO.Path.GetFileName(sourcePath.Parse());

            var topLevel = sender.GetEncapsulatingReferencedModule().GetType().Name;
            var senderType = sender.GetType().Name;
            var sourceType = (null != sender.SourceModule) ? sender.SourceModule.GetType().FullName : "publishroot";
            var basename = sourceType + "_" + topLevel + "_" + senderType + "_" + sender.BuildEnvironment.Configuration.ToString() + "_";

            var isSymLink = sender is CollatedSymbolicLink;
            var isDir = sender is CollatedDirectory;
            var isRenamedDir = sender.Tool is CopyFilePosix & sender.Macros["CopiedFilename"].IsAliased;
            if (isSymLink)
            {
                rule.AddTarget(sender.GeneratedPaths[CollatedObject.Key], variableName: basename + sourceFilename, isPhony: true);
            }
            else
            {
                if (isDir)
                {
                    if (isRenamedDir)
                    {
                        var rename = sender.Macros["CopiedFilename"].Parse();
                        rule.AddTarget(Bam.Core.TokenizedString.CreateVerbatim(basename + rename), isPhony: true);
                    }
                    else
                    {
                        var targetName = sender.CreateTokenizedString("$(0)/@filename($(1))", sender.Macros["CopyDir"], sourcePath);
                        rule.AddTarget(targetName, variableName: basename + sourceFilename);
                    }
                }
                else
                {
                    rule.AddTarget(sender.GeneratedPaths[CollatedObject.Key], variableName: basename + sourceFilename);
                }
            }

            meta.CommonMetaData.AddDirectory(sender.Macros["CopyDir"].Parse());

            var commandLine = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(commandLine);

            if (isSymLink)
            {
                rule.AddShellCommand(System.String.Format(@"{0} {1} {2} $@ {3}",
                    CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool),
                    commandLine.ToString(' '),
                    sender.Macros["LinkTarget"].Parse(),
                    CommandLineProcessor.Processor.TerminatingArgs(sender.Tool as Bam.Core.ICommandLineTool)));
            }
            else
            {
                if (isDir)
                {
                    if (isRenamedDir)
                    {
                        rule.AddShellCommand(System.String.Format(@"{0} {1} $</* {2} {3}",
                            CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool),
                            commandLine.ToString(' '),
                            sender.Macros["CopyDir"].Parse(),
                            CommandLineProcessor.Processor.TerminatingArgs(sender.Tool as Bam.Core.ICommandLineTool)));
                    }
                    else
                    {
                        rule.AddShellCommand(System.String.Format(@"{0} {1} $< $(dir $@) {2}",
                            CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool),
                            commandLine.ToString(' '),
                            CommandLineProcessor.Processor.TerminatingArgs(sender.Tool as Bam.Core.ICommandLineTool)));
                    }
                }
                else
                {
                    rule.AddShellCommand(System.String.Format(@"{0} {1} $< $(dir $@) {2}",
                        CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool),
                        commandLine.ToString(' '),
                        CommandLineProcessor.Processor.TerminatingArgs(sender.Tool as Bam.Core.ICommandLineTool)),
                        ignoreErrors: !(sender as CollatedFile).FailWhenSourceDoesNotExist);
                }
                rule.AddPrerequisite(sourcePath);
            }
        }
    }
}
