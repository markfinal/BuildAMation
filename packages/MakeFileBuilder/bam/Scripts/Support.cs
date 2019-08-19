#region License
// Copyright (c) 2010-2019, Mark Final
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
using System.Linq;
namespace MakeFileBuilder
{
    /// <summary>
    /// Helper class for writing MakeFiles
    /// </summary>
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
            foreach (var (inputModule,inputPathKey) in module.InputModulePaths)
            {
                rule.AddPrerequisite(inputModule, inputPathKey);
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
                            rule.AddPrerequisite(target);
                        }
                    });
                }
            }

            var commands = new Bam.Core.StringArray
            {
                executable.ToString(),
                arguments.ToString(' ')
            };
            rule.AddShellCommand(commands.ToString(' '));
        }

        public static void
        AddCheckpoint(
            Bam.Core.Module module,
            string excludingGeneratedPath = null)
        {
            if (module.GeneratedPaths.Any(item => (null == excludingGeneratedPath) || !item.Key.Equals(excludingGeneratedPath, System.StringComparison.Ordinal)))
            {
                var message = new System.Text.StringBuilder();
                message.AppendLine("A checkpoint must have no outputs");
                foreach (var genPath in module.GeneratedPaths)
                {
                    message.AppendLine($"\t{genPath.Value.ToString()} [{genPath.Key}]");
                }
                throw new Bam.Core.Exception(message.ToString());
            }
            if (!Bam.Core.Graph.Instance.IsReferencedModule(module))
            {
                throw new Bam.Core.Exception(
                    $"A checkpoint must be a referenced module, {module.ToString()} is not"
                );
            }
            var meta = new MakeFileBuilder.MakeFileMeta(module);
            var rule = meta.AddRule();
            rule.AddTarget(
                Bam.Core.TokenizedString.CreateVerbatim(Target.MakeUniqueVariableName(module, null))
            );
            foreach (var dep in module.Requirements)
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
                            rule.AddPrerequisite(target);
                        }
                    });
                }
            }
            // no shell commands
        }

        public static void
        Add(
            Bam.Core.Module module,
            Bam.Core.TokenizedString redirectOutputToFile = null,
            bool isDependencyOfAll = false,
            Bam.Core.Module moduleToAppendTo = null)
        {
            MakeFileMeta meta = null;
            Rule rule = null;
            if (null == moduleToAppendTo)
            {
                meta = new MakeFileBuilder.MakeFileMeta(module);
                rule = meta.AddRule();
                foreach (var output in module.GeneratedPaths)
                {
                    rule.AddTarget(
                        output.Value,
                        keyName: output.Key,
                        isDependencyOfAll: isDependencyOfAll
                    );
                }
            }
            else
            {
                meta = moduleToAppendTo.MetaData as MakeFileMeta;
                rule = meta.Rules[0];
            }

            foreach (var dir in module.OutputDirectories)
            {
                meta.CommonMetaData.AddDirectory(dir.ToString());
            }
            foreach (var (inputModule,inputPathKey) in module.InputModulePaths)
            {
                rule.AddPrerequisite(inputModule, inputPathKey);
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
                            rule.AddPrerequisite(target);
                        }
                    });
                }
            }
            foreach (var dep in module.Requirements)
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
                            rule.AddOrderOnlyDependency(target);
                        }
                    });
                }
            }

            var tool = module.Tool as Bam.Core.ICommandLineTool;
            if (null == tool)
            {
                // no shell commands, just target & prerequisites
                return;
            }
            if (null != tool.EnvironmentVariables)
            {
                meta.CommonMetaData.ExtendEnvironmentVariables(tool.EnvironmentVariables);
            }

            var shellCommands = new Bam.Core.StringArray();
            if (module.WorkingDirectory != null)
            {
                if (MakeFileBuilder.MakeFileCommonMetaData.IsNMAKE)
                {
                    shellCommands.Add($"cd /D {module.WorkingDirectory.ToStringQuoteIfNecessary()} &&");
                }
                else
                {
                    shellCommands.Add($"cd {module.WorkingDirectory.ToString()} &&");
                }
            }
            shellCommands.Add(CommandLineProcessor.Processor.StringifyTool(tool));
            shellCommands.Add(
                CommandLineProcessor.NativeConversion.Convert(
                    module.Settings,
                    module
                ).ToString(' ')
            );
            shellCommands.Add(CommandLineProcessor.Processor.TerminatingArgs(tool));
            if (null != redirectOutputToFile)
            {
                shellCommands.Add(">");
                shellCommands.Add(redirectOutputToFile.ToString());
            }
            rule.AddShellCommand(shellCommands.ToString(' '));
        }
    }
}
