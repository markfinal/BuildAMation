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
namespace C
{
#if BAM_V2
    public static partial class MakeFileSupport
    {
        public static void
        SymLink(
            SharedObjectSymbolicLink module)
        {
            var meta = new MakeFileBuilder.MakeFileMeta(module);

            foreach (var dir in module.OutputDirectories)
            {
                meta.CommonMetaData.AddDirectory(dir.ToString());
            }

            var rule = meta.AddRule();

            // since this is not a referenced type, need to specify a variable name for the MakeFile
            var variableName = new System.Text.StringBuilder();
            variableName.Append(module.SharedObject.GetType().Name); // this is what it's building the symlink for
            variableName.Append("_");
            variableName.Append(module.Macros["SymlinkUsage"].ToString()); // intended usage

            rule.AddTarget(module.GeneratedPaths[SharedObjectSymbolicLink.SOSymLinkKey], variableName:variableName.ToString());
            foreach (var input in module.InputModules)
            {
                rule.AddPrerequisite(input.Value, input.Key);
            }

            var tool = module.Tool as Bam.Core.ICommandLineTool;
            var command = new System.Text.StringBuilder();
            command.AppendFormat(
                "{0} {1} {2}",
                CommandLineProcessor.Processor.StringifyTool(tool),
                CommandLineProcessor.NativeConversion.Convert(
                    module.Settings,
                    module
                ).ToString(' '),
                CommandLineProcessor.Processor.TerminatingArgs(tool)
            );
            rule.AddShellCommand(command.ToString());
        }
    }
#else
    public sealed class MakeFileSharedObjectSymbolicLink :
        ISharedObjectSymbolicLinkPolicy
    {
        void
        ISharedObjectSymbolicLinkPolicy.Symlink(
            SharedObjectSymbolicLink sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.PreBuiltTool tool,
            ConsoleApplication target)
        {
            var meta = new MakeFileBuilder.MakeFileMeta(sender);
            var rule = meta.AddRule();

            // since this is not a referenced type, need to specify a variable name for the MakeFile
            var variableName = new System.Text.StringBuilder();
            variableName.Append(target.GetType().Name); // this is what it's building the symlink for
            variableName.Append("_");
            variableName.Append(sender.Macros["SymlinkUsage"].ToString()); // intended usage

            rule.AddTarget(sender.GeneratedPaths[SharedObjectSymbolicLink.Key], variableName:variableName.ToString());
            rule.AddPrerequisite(target, C.ConsoleApplication.Key);

            var commandLineArgs = new Bam.Core.StringArray();
            commandLineArgs.Add("-s");
            commandLineArgs.Add("-f");

            var command = new System.Text.StringBuilder();
            command.AppendFormat("{0} {1} $(notdir $<) $@ {2}",
                                 CommandLineProcessor.Processor.StringifyTool(tool),
                                 commandLineArgs.ToString(' '),
                                 CommandLineProcessor.Processor.TerminatingArgs(tool));
            rule.AddShellCommand(command.ToString());
        }
    }
#endif
}
