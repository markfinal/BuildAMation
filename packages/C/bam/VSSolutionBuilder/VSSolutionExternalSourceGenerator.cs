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
    public static partial class VSSolutionSupport
    {
        public static void
        GenerateSource(
            ExternalSourceGenerator module)
        {
            var encapsulating = module.GetEncapsulatingReferencedModule();

            var solution = Bam.Core.Graph.Instance.MetaData as VSSolutionBuilder.VSSolution;
            var project = solution.EnsureProjectExists(encapsulating);
            var config = project.GetConfiguration(encapsulating);

            var args = new Bam.Core.StringArray();
            args.Add(
                System.String.Format(
                    "{0} {1}",
                    module.Executable.ToStringQuoteIfNecessary(),
                    module.Arguments.ToString(' ')
                )
            );

            VSSolutionBuilder.Support.AddCustomBuildStep(
                module.InputFiles,
                module.ExpectedOutputFiles.Values,
                config,
                System.String.Format("Running '{0}'", args.ToString(' ')),
                args.ToString(' '),
                true
            );
        }
    }
#else
    public sealed class VSSolutionExternalSourceGenerator :
        IExternalSourceGeneratorPolicy
    {
        void
        IExternalSourceGeneratorPolicy.GenerateSource(
            ExternalSourceGenerator sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString executable,
            Bam.Core.TokenizedStringArray arguments,
            Bam.Core.TokenizedString output_directory,
            System.Collections.Generic.IReadOnlyDictionary<string, Bam.Core.TokenizedString> expected_output_files,
            System.Collections.Generic.IReadOnlyDictionary<string, Bam.Core.TokenizedString> input_files
        )
        {
            var encapsulating = sender.GetEncapsulatingReferencedModule();

            var solution = Bam.Core.Graph.Instance.MetaData as VSSolutionBuilder.VSSolution;
            var project = solution.EnsureProjectExists(encapsulating);
            var config = project.GetConfiguration(encapsulating);

            var args = new Bam.Core.StringArray();
            args.Add(System.String.Format("{0} {1}", executable.ToStringQuoteIfNecessary(), arguments.ToString(' ')));

            foreach (var input in input_files.Values)
            {
                config.AddOtherFile(input);
                var customBuild = config.GetSettingsGroup(
                    VSSolutionBuilder.VSSettingsGroup.ESettingsGroup.CustomBuild,
                    include: input,
                    uniqueToProject: true
                );
                customBuild.AddSetting(
                    "Command",
                    args.ToString(' '),
                    condition: config.ConditionText
                );
                customBuild.AddSetting(
                    "Message",
                    System.String.Format("Generating outputs from {0}", input.ToString()),
                    condition: config.ConditionText
                );
                customBuild.AddSetting(
                    "Outputs",
                    expected_output_files.Values,
                    condition: config.ConditionText
                );
                customBuild.AddSetting(
                    "AdditionalInputs",
                    input_files.Values,
                    condition: config.ConditionText
                );
            }
        }
    }
#endif
}
