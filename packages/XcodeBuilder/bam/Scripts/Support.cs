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
namespace XcodeBuilder
{
    public static partial class Support
    {
        private static void
        AddModuleDirectoryCreationShellCommands(
            Bam.Core.Module module,
            Bam.Core.StringArray shellCommandLines)
        {
            foreach (var dir in module.OutputDirectories)
            {
                shellCommandLines.Add(
                    System.String.Format(
                        "[[ ! -d {0} ]] && mkdir -p {0}",
                        Bam.Core.IOWrapper.EscapeSpacesInPath(dir.ToString())
                    )
                );
            }
        }

        private static void
        AddNewerThanPreamble(
            Bam.Core.Module module,
            Bam.Core.StringArray shellCommandLines)
        {
            var condition_text = new System.Text.StringBuilder();
            condition_text.Append("if [[ ");
            var last_output = module.GeneratedPaths.Values.Last();
            foreach (var output in module.GeneratedPaths.Values)
            {
                var output_path = Bam.Core.IOWrapper.EscapeSpacesInPath(output.ToString());
                condition_text.AppendFormat("! -e {0} ", output_path);
                foreach (var input in module.InputModules)
                {
                    var input_path = Bam.Core.IOWrapper.EscapeSpacesInPath(input.Value.GeneratedPaths[input.Key].ToString());
                    condition_text.AppendFormat("|| {1} -nt {0} ", output_path, input_path);
                }
                if (output != last_output)
                {
                    condition_text.AppendFormat("|| ");
                }
            }
            condition_text.AppendLine("]]");
            shellCommandLines.Add(condition_text.ToString());
            shellCommandLines.Add("then");
        }

        private static void
        AddNewerThanPostamble(
            Bam.Core.Module module,
            Bam.Core.StringArray shellCommandLines)
        {
            shellCommandLines.Add("fi");
        }

        public static void
        AddPreBuildCommands(
            Bam.Core.Module module,
            Target target,
            Configuration configuration,
            string commandLine)
        {
            var shellCommandLines = new Bam.Core.StringArray();
            AddModuleDirectoryCreationShellCommands(module, shellCommandLines);
            AddNewerThanPreamble(module, shellCommandLines);
            shellCommandLines.Add(System.String.Format("\techo {0}", commandLine));
            shellCommandLines.Add(System.String.Format("\t{0}", commandLine));
            AddNewerThanPostamble(module, shellCommandLines);

            target.AddPreBuildCommands(
                shellCommandLines,
                configuration
            );
        }

        public static void
        AddPostBuildCommands(
            Bam.Core.Module module,
            Target target,
            Configuration configuration,
            Bam.Core.StringArray customCommands)
        {
            var shellCommandLines = new Bam.Core.StringArray();
            AddModuleDirectoryCreationShellCommands(module, shellCommandLines);
            shellCommandLines.AddRange(customCommands);

            target.AddPostBuildCommands(
                shellCommandLines,
                configuration
            );
        }
    }
}
