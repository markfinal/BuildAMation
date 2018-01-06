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
            if (sender.Ignore)
            {
                return;
            }
            var collatedInterface = sender as ICollatedObject;
            var copySourcePath = sender.SourcePath;

            // post-fix with a directory separator to enforce that this is a directory destination
            var destinationDir = System.String.Format("{0}{1}",
                collatedInterface.PublishingDirectory.ToString(),
                System.IO.Path.DirectorySeparatorChar);

            if (null == sender.PreExistingSourcePath)
            {
                Bam.Core.Log.DebugMessage("** {0}[{1}]:\t'{2}' -> '{3}'",
                    collatedInterface.SourceModule.ToString(),
                    collatedInterface.SourcePathKey.ToString(),
                    copySourcePath.ToString(),
                    destinationDir);
            }
            else
            {
                Bam.Core.Log.DebugMessage("** {0}: '{1}' -> '{2}'",
                    sender,
                    copySourcePath.ToString(),
                    destinationDir);
            }

            var meta = new MakeFileBuilder.MakeFileMeta(sender);
            meta.CommonMetaData.AddDirectory(destinationDir);
            var rule = meta.AddRule();

            var topLevel = sender.GetEncapsulatingReferencedModule().GetType().Name;
            var senderType = sender.GetType().Name;
            var sourceType = (null != collatedInterface.SourceModule) ? collatedInterface.SourceModule.GetType().FullName : "publishroot";
            var basename = sourceType + "_" + topLevel + "_" + senderType + "_" + sender.BuildEnvironment.Configuration.ToString() + "_";
            var sourceFilename = System.IO.Path.GetFileName(copySourcePath.ToString());
            var isPosixLeafRename = (sourceFilename == "*");

            Bam.Core.TokenizedString prerequisitePath;
            var destinationPath = sender.CreateTokenizedString("$(0)/$(1)", new Bam.Core.TokenizedString[] { collatedInterface.PublishingDirectory, Bam.Core.TokenizedString.CreateVerbatim(sourceFilename) });

            if (isPosixLeafRename)
            {
                sourceFilename = "all_files";
                prerequisitePath = sender.CreateTokenizedString("@dir($(0))", copySourcePath);
            }
            else
            {
                prerequisitePath = copySourcePath;
            }
            rule.AddTarget(destinationPath, variableName: basename + sourceFilename);

            var commandLine = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(commandLine);

            if (isPosixLeafRename)
            {
                rule.AddShellCommand(System.String.Format(@"{0} {1} $</* $(dir $@) {2}",
                    CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool),
                    commandLine.ToString(' '),
                    CommandLineProcessor.Processor.TerminatingArgs(sender.Tool as Bam.Core.ICommandLineTool)));
            }
            else
            {
                rule.AddShellCommand(System.String.Format(@"{0} {1} $< $(dir $@) {2}",
                    CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool),
                    commandLine.ToString(' '),
                    CommandLineProcessor.Processor.TerminatingArgs(sender.Tool as Bam.Core.ICommandLineTool)));
            }
            rule.AddPrerequisite(prerequisitePath);
        }
    }
}
