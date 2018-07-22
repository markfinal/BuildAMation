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
namespace Installer
{
#if BAM_V2
    public static partial class MakeFileSupport
    {
        public static void
        CreateTarBall(
            TarBall module)
        {
            var meta = new MakeFileBuilder.MakeFileMeta(module);

            foreach (var dir in module.OutputDirectories)
            {
                meta.CommonMetaData.AddDirectory(dir.ToString());
            }

            var rule = meta.AddRule();

            rule.AddTarget(
                module.GeneratedPaths[TarBall.TarBallKey]
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
    public sealed class NativeTarBall :
        ITarPolicy
    {
        void
        ITarPolicy.CreateTarBall(
            TarBall sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.ICommandLineTool compiler,
            Bam.Core.TokenizedString scriptPath,
            Bam.Core.TokenizedString outputPath)
        {
            var tarPath = outputPath.ToString();
            var tarDir = System.IO.Path.GetDirectoryName(tarPath);
            Bam.Core.IOWrapper.CreateDirectoryIfNotExists(tarDir);

            var commandLine = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(commandLine);

            commandLine.Add("-c");
            commandLine.Add("-v");
            commandLine.Add("-T");
            commandLine.Add(scriptPath.ToStringQuoteIfNecessary());
            commandLine.Add("-f");
            commandLine.Add(outputPath.ToStringQuoteIfNecessary()); // tarPath
            CommandLineProcessor.Processor.Execute(context, compiler, commandLine);
        }
    }
#endif
}
