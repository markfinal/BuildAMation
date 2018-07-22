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
namespace Publisher
{
#if BAM_V2
    public static partial class MakeFileSupport
    {
        public static void
        Strip(
            StripModule module)
        {
            var meta = new MakeFileBuilder.MakeFileMeta(module);

            foreach (var dir in module.OutputDirectories)
            {
                meta.CommonMetaData.AddDirectory(dir.ToString());
            }

            var rule = meta.AddRule();

            var variableName = new System.Text.StringBuilder();
            variableName.Append((module as ICollatedObject).SourceModule.GetType().Name);

            rule.AddTarget(
                module.GeneratedPaths[StripModule.StripBinaryKey],
                variableName: "Strip" + variableName
            );
            foreach (var input in module.InputModules)
            {
                rule.AddPrerequisite(input.Value, input.Key);
            }

            rule.AddShellCommand(System.String.Format("{0} {1} {2}",
                CommandLineProcessor.Processor.StringifyTool(module.Tool as Bam.Core.ICommandLineTool),
                CommandLineProcessor.NativeConversion.Convert(
                    module.Settings,
                    module
                ).ToString(' '),
                CommandLineProcessor.Processor.TerminatingArgs(module.Tool as Bam.Core.ICommandLineTool))
            );
        }
    }
#else
    public sealed class MakeFileStrip :
        IStripToolPolicy
    {
        void
        IStripToolPolicy.Strip(
            StripModule sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString originalPath,
            Bam.Core.TokenizedString copiedPath)
        {
            var meta = new MakeFileBuilder.MakeFileMeta(sender);
            var rule = meta.AddRule();

            var sourceFilename = System.IO.Path.GetFileName(originalPath.ToString());

            var dir = sender.CreateTokenizedString("@dir($(0))", copiedPath);
            dir.Parse();
            meta.CommonMetaData.AddDirectory(dir.ToString());
            rule.AddTarget(copiedPath, variableName: "strip_" + sourceFilename);
            rule.AddPrerequisite(originalPath);

            var commandLine = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(commandLine);

            rule.AddShellCommand(System.String.Format("{0} {1} {2} -o {3} {4}",
                CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool),
                commandLine.ToString(' '),
                originalPath.ToString(),
                copiedPath.ToString(),
                CommandLineProcessor.Processor.TerminatingArgs(sender.Tool as Bam.Core.ICommandLineTool)));
        }
    }
#endif
}
