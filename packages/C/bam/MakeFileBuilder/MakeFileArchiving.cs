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
namespace C
{
#if BAM_V2
    public static partial class MakeFileSupport
    {
        public static void
        Archive(
            StaticLibrary module)
        {
            var output_path = (module.Settings as ICommonHasOutputPath).OutputPath;

            var meta = new MakeFileBuilder.MakeFileMeta(module);
            var rule = meta.AddRule();
            rule.AddTarget(output_path);
            foreach (var input in module.ObjectFiles)
            {
                if (!(input as C.ObjectFileBase).PerformCompilation)
                {
                    continue;
                }
                rule.AddPrerequisite(input, C.ObjectFile.Key);
            }

            var tool = module.Tool as Bam.Core.ICommandLineTool;
            var command = new System.Text.StringBuilder();
            command.AppendFormat("{0} {1} $^ {2}",
                CommandLineProcessor.Processor.StringifyTool(tool),
                CommandLineProcessor.NativeConversion.Convert(module).ToString(' '),
                CommandLineProcessor.Processor.TerminatingArgs(tool));
            rule.AddShellCommand(command.ToString());

            var output_dir = System.IO.Path.GetDirectoryName(output_path.ToString());
            meta.CommonMetaData.AddDirectory(output_dir);
            meta.CommonMetaData.ExtendEnvironmentVariables(tool.EnvironmentVariables);
        }
    }
#else
    public sealed class MakeFileLibrarian :
        IArchivingPolicy
    {
        void
        IArchivingPolicy.Archive(
            StaticLibrary sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString libraryPath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> objectFiles,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> headers)
        {
            var commandLineArgs = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(commandLineArgs);

            var meta = new MakeFileBuilder.MakeFileMeta(sender);
            var rule = meta.AddRule();
            rule.AddTarget(libraryPath);
            foreach (var input in objectFiles)
            {
                if (!(input as C.ObjectFileBase).PerformCompilation)
                {
                    continue;
                }
                rule.AddPrerequisite(input, C.ObjectFile.Key);
            }

            var tool = sender.Tool as Bam.Core.ICommandLineTool;
            var command = new System.Text.StringBuilder();
            command.AppendFormat("{0} {1} $^ {2}",
                CommandLineProcessor.Processor.StringifyTool(tool),
                commandLineArgs.ToString(' '),
                CommandLineProcessor.Processor.TerminatingArgs(tool));
            rule.AddShellCommand(command.ToString());

            var libraryFileDir = System.IO.Path.GetDirectoryName(libraryPath.ToString());
            meta.CommonMetaData.AddDirectory(libraryFileDir);
            meta.CommonMetaData.ExtendEnvironmentVariables(tool.EnvironmentVariables);
        }
    }
#endif
}
