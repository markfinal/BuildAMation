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
namespace MakeFileBuilder
{
    public static partial class Support
    {
        public static void
        AddArbitraryTool(
            Bam.Core.Module module,
            Bam.Core.TokenizedString executable,
            Bam.Core.TokenizedStringArray arguments)
        {
            var meta = new MakeFileBuilder.MakeFileMeta(module);

            foreach (var dir in module.OutputDirectories)
            {
                meta.CommonMetaData.AddDirectory(dir.ToString());
            }

            var rule = meta.AddRule();
            foreach (var output in module.GeneratedPaths)
            {
                rule.AddTarget(
                    output.Value,
                    keyName: output.Key
                );
            }
            foreach (var input in module.InputModules)
            {
                rule.AddPrerequisite(input.Value, input.Key);
            }
            foreach (var dep in module.Dependents)
            {
                if (null == dep.MetaData)
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

            var commands = new Bam.Core.StringArray();
            commands.Add(executable.ToString());
            commands.Add(arguments.ToString(' '));
            rule.AddShellCommand(commands.ToString(' '));
        }

        public static void
        Add(
            Bam.Core.Module module)
        {
            var meta = new MakeFileBuilder.MakeFileMeta(module);

            foreach (var dir in module.OutputDirectories)
            {
                meta.CommonMetaData.AddDirectory(dir.ToString());
            }

            var rule = meta.AddRule();
            foreach (var output in module.GeneratedPaths)
            {
                rule.AddTarget(
                    output.Value,
                    keyName: output.Key
                );
            }
            foreach (var input in module.InputModules)
            {
                rule.AddPrerequisite(input.Value, input.Key);
            }
            foreach (var dep in module.Dependents)
            {
                if (null == dep.MetaData)
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

            var tool = module.Tool as Bam.Core.ICommandLineTool;
            if (null != tool.EnvironmentVariables)
            {
                meta.CommonMetaData.ExtendEnvironmentVariables(tool.EnvironmentVariables);
            }

            var commands = new Bam.Core.StringArray();
            commands.Add(CommandLineProcessor.Processor.StringifyTool(tool));
            commands.Add(
                CommandLineProcessor.NativeConversion.Convert(
                    module.Settings,
                    module
                ).ToString(' ')
            );
            commands.Add(CommandLineProcessor.Processor.TerminatingArgs(tool));
            rule.AddShellCommand(commands.ToString(' '));
        }
    }
}
