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
namespace C
{
    public sealed class MakeFileCompilation :
        ICompilationPolicy
    {
        void
        ICompilationPolicy.Compile(
            ObjectFile sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString objectFilePath,
            Bam.Core.Module source)
        {
            if (!sender.PerformCompilation)
            {
                return;
            }

            var commandLineArgs = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(commandLineArgs);

            var meta = new MakeFileBuilder.MakeFileMeta(sender);
            var rule = meta.AddRule();
            rule.AddTarget(objectFilePath);
            rule.AddPrerequisite(source, C.SourceFile.Key);

            var tool = sender.Tool as Bam.Core.ICommandLineTool;
            var command = new System.Text.StringBuilder();
            command.AppendFormat("{0} {1} $< {2}",
                CommandLineProcessor.Processor.StringifyTool(tool),
                commandLineArgs.ToString(' '),
                CommandLineProcessor.Processor.TerminatingArgs(tool));
            rule.AddShellCommand(command.ToString());

            var objectFileDir = System.IO.Path.GetDirectoryName(objectFilePath.ToString());
            meta.CommonMetaData.AddDirectory(objectFileDir);
            meta.CommonMetaData.ExtendEnvironmentVariables(tool.EnvironmentVariables);

            // add dependencies, such as procedurally generated headers
            foreach (var dep in sender.Dependents)
            {
                if (null == dep.MetaData)
                {
                    continue;
                }
                if (dep is C.SourceFile)
                {
                    continue;
                }
                var depMeta = dep.MetaData as MakeFileBuilder.MakeFileMeta;
                foreach (var depRule in depMeta.Rules)
                {
                    depRule.ForEachTarget(target =>
                        {
                            if (!target.IsPhony)
                            {
                                rule.AddPrerequisite(target.Path);
                            }
                        });
                }
            }
        }
    }
}

