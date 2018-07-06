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
namespace C
{
    public sealed class XcodeExternalSourceGenerator :
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

            var workspace = Bam.Core.Graph.Instance.MetaData as XcodeBuilder.WorkspaceMeta;
            var target = workspace.EnsureTargetExists(encapsulating);
            if (encapsulating == sender)
            {
                target.SetType(XcodeBuilder.Target.EProductType.Utility);
            }
            var configuration = target.GetConfiguration(encapsulating);
            if (encapsulating == sender)
            {
                configuration.SetProductName(Bam.Core.TokenizedString.CreateVerbatim("${TARGET_NAME}"));
            }

            var commands = new Bam.Core.StringArray();

            commands.Add(
                System.String.Format(
                    "[[ ! -d {0} ]] && mkdir -p {0}",
                    Bam.Core.IOWrapper.EscapeSpacesInPath(output_directory.ToString())
                )
            );

            var condition_text = new System.Text.StringBuilder();
            condition_text.Append("if [[ ");
            var last_output = expected_output_files.Values.Last();
            foreach (var output in expected_output_files.Values)
            {
                var output_path = Bam.Core.IOWrapper.EscapeSpacesInPath(output.ToString());
                condition_text.AppendFormat("! -e {0} ", output_path);
                foreach (var input in input_files.Values)
                {
                    var input_path = Bam.Core.IOWrapper.EscapeSpacesInPath(input.ToString());
                    condition_text.AppendFormat("|| {1} -nt {0} ", output_path, input_path);
                }
                if (output != last_output)
                {
                    condition_text.AppendFormat("|| ");
                }
            }
            condition_text.AppendLine("]]");
            condition_text.AppendLine("then");

            var cmd_line = System.String.Format("{0} {1}", executable.ToStringQuoteIfNecessary(), arguments.ToString(' '));
            condition_text.AppendLine(System.String.Format("\techo {0}", cmd_line));
            condition_text.AppendLine(System.String.Format("\t{0}", cmd_line));
            condition_text.AppendLine("fi");
            commands.Add(condition_text.ToString());

            target.AddPreBuildCommands(commands, configuration);
        }
    }
}
