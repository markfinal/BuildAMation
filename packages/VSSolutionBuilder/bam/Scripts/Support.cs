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
using System.Linq;
namespace VSSolutionBuilder
{
    static public partial class Support
    {
        static public void
        AddCustomBuildStep(
            System.Collections.Generic.IEnumerable<Bam.Core.TokenizedString> inputs,
            System.Collections.Generic.IEnumerable<Bam.Core.TokenizedString> outputs,
            VSProjectConfiguration config,
            string message,
            Bam.Core.StringArray commandList,
            bool commandIsForFirstInputOnly)
        {
            var is_first_input = true;
            foreach (var input in inputs)
            {
                config.AddOtherFile(input);
                if (!is_first_input && commandIsForFirstInputOnly)
                {
                    continue;
                }
                var customBuild = config.GetSettingsGroup(
                    VSSolutionBuilder.VSSettingsGroup.ESettingsGroup.CustomBuild,
                    include: input,
                    uniqueToProject: true
                );
                customBuild.AddSetting("Command", commandList.ToString(System.Environment.NewLine), condition: config.ConditionText);
                customBuild.AddSetting("Message", message, condition: config.ConditionText);
                customBuild.AddSetting("Outputs", outputs, condition: config.ConditionText);
                customBuild.AddSetting("AdditionalInputs", inputs, condition: config.ConditionText); // TODO: incorrectly adds all inputs
                is_first_input = false;
            }
        }

        static public void
        AddCustomBuildStepForCommandLineTool(
            Bam.Core.Module module,
            Bam.Core.TokenizedString outputPath,
            string messagePrefix)
        {
            var encapsulating = module.GetEncapsulatingReferencedModule();

            var solution = Bam.Core.Graph.Instance.MetaData as VSSolutionBuilder.VSSolution;
            var project = solution.EnsureProjectExists(encapsulating);
            var config = project.GetConfiguration(encapsulating);

            var commands = new Bam.Core.StringArray();
            foreach (var dir in module.OutputDirectories)
            {
                commands.Add(
                    System.String.Format(
                        "IF NOT EXIST {0} MKDIR {0}",
                        dir.ToStringQuoteIfNecessary()
                    )
                );
            }

            var args = new Bam.Core.StringArray();
            foreach (var envVar in (module.Tool as Bam.Core.ICommandLineTool).EnvironmentVariables)
            {
                args.Add("set");
                var content = new System.Text.StringBuilder();
                content.AppendFormat("{0}=", envVar.Key);
                foreach (var value in envVar.Value)
                {
                    content.AppendFormat("{0};", value.ToStringQuoteIfNecessary());
                }
                content.AppendFormat("%{0}%", envVar.Key);
                args.Add(content.ToString());
                args.Add("&&");
            }
            args.Add(CommandLineProcessor.Processor.StringifyTool(module.Tool as Bam.Core.ICommandLineTool));
            args.AddRange(
                CommandLineProcessor.NativeConversion.Convert(
                    module.Settings,
                    module
                )
            );
            commands.Add(args.ToString(' '));

            System.Diagnostics.Debug.Assert(1 == module.InputModules.Count());
            var firstInput = module.InputModules.First();
            var sourcePath = firstInput.Value.GeneratedPaths[firstInput.Key];
            AddCustomBuildStep(
                System.Linq.Enumerable.Repeat(sourcePath, 1),
                System.Linq.Enumerable.Repeat(outputPath, 1),
                config,
                System.String.Format(
                    "{0} {1} into {2}",
                    messagePrefix,
                    sourcePath.ToString(),
                    outputPath.ToString()
                ),
                commands,
                true
            );
        }

        static public void
        AddPostBuildStep(
            VSProjectConfiguration config,
            Bam.Core.Module module,
            Bam.Core.StringArray commands)
        {
            var shellCommands = new Bam.Core.StringArray();
            foreach (var dir in module.OutputDirectories)
            {
                shellCommands.Add(System.String.Format("IF NOT EXIST {0} MKDIR {0}", dir.ToString()));
            }
            shellCommands.AddRange(commands);
            config.AddPostBuildCommands(shellCommands);
        }
    }
}
