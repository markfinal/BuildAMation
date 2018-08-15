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
            if (!module.GeneratedPaths.Any())
            {
                return;
            }
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
            if (!module.GeneratedPaths.Any())
            {
                return;
            }
            shellCommandLines.Add("fi");
        }

        static private void
        AddModuleCommandLineShellCommand(
            Bam.Core.Module module,
            Bam.Core.StringArray shellCommandLines,
            bool allowNonZeroSuccessfulExitCodes)
        {
            if (null == module.Tool)
            {
                throw new Bam.Core.Exception(
                    "Command line tool passed with module '{0}' is invalid",
                    module.ToString()
                );
            }
            System.Diagnostics.Debug.Assert(module.Tool is Bam.Core.ICommandLineTool);

            var args = new Bam.Core.StringArray();
            if (module.WorkingDirectory != null)
            {
                args.Add(
                    System.String.Format(
                        "cd {0} &&",
                        module.WorkingDirectory.ToStringQuoteIfNecessary()
                    )
                );
            }
            var tool = module.Tool as Bam.Core.ICommandLineTool;
            args.Add(CommandLineProcessor.Processor.StringifyTool(tool));
            args.AddRange(
                CommandLineProcessor.NativeConversion.Convert(
                    module.Settings,
                    module
                )
            );
            args.Add(CommandLineProcessor.Processor.TerminatingArgs(tool));
            if (allowNonZeroSuccessfulExitCodes)
            {
                args.Add("|| true");
            }
            shellCommandLines.Add(args.ToString(' '));
        }

        private static void
        GetTargetAndConfiguration(
            Bam.Core.Module module,
            out Target target,
            out Configuration configuration)
        {
            var encapsulating = module.GetEncapsulatingReferencedModule();
#if D_PACKAGE_PUBLISHER
            if (encapsulating is Publisher.Collation)
            {
                (encapsulating as Publisher.Collation).ForEachAnchor(
                    (collation, anchor, customData) =>
                    {
                        encapsulating = anchor.SourceModule;
                    },
                    null
                );
            }
#endif

            var workspace = Bam.Core.Graph.Instance.MetaData as XcodeBuilder.WorkspaceMeta;
            target = workspace.EnsureTargetExists(encapsulating);
            configuration = target.GetConfiguration(encapsulating);
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
        AddPreBuildStepForCommandLineTool(
            Bam.Core.Module module,
            out Target target,
            out Configuration configuration,
            bool checkForNewer,
            bool allowNonZeroSuccessfulExitCodes,
            bool addOrderOnlyDependencyOnTool = false)
        {
            GetTargetAndConfiguration(
                module,
                out target,
                out configuration
            );

            var shellCommandLines = new Bam.Core.StringArray();
            AddModuleDirectoryCreationShellCommands(module, shellCommandLines);
            if (checkForNewer)
            {
                AddNewerThanPreamble(module, shellCommandLines);
            }
            AddModuleCommandLineShellCommand(
                module,
                shellCommandLines,
                allowNonZeroSuccessfulExitCodes
            );
            if (checkForNewer)
            {
                AddNewerThanPostamble(module, shellCommandLines);
            }

            target.AddPreBuildCommands(
                shellCommandLines,
                configuration
            );

            if (addOrderOnlyDependencyOnTool)
            {
                var tool = module.Tool;
#if D_PACKAGE_PUBLISHER
                // note the custom handler here, which checks to see if we're running a tool
                // that has been collated
                if (tool is Publisher.CollatedCommandLineTool)
                {
                    tool = (tool as Publisher.ICollatedObject).SourceModule;
                }
#endif
                var toolTarget = tool.MetaData as Target;
                if (null != toolTarget)
                {
                    target.Requires(toolTarget);
                }
            }
        }

        public static void
        AddPreBuildStepForCommandLineTool(
            Bam.Core.Module module,
            out Target target,
            out Configuration configuration,
            FileReference.EFileType inputFileType,
            bool checkForNewer,
            bool allowNonZeroSuccessfulExitCodes,
            bool addOrderOnlyDependencyOnTool = false)
        {
            AddPreBuildStepForCommandLineTool(
                module,
                out target,
                out configuration,
                checkForNewer,
                allowNonZeroSuccessfulExitCodes,
                addOrderOnlyDependencyOnTool: allowNonZeroSuccessfulExitCodes
            );
            foreach (var input in module.InputModules)
            {
                target.EnsureFileOfTypeExists(
                    input.Value.GeneratedPaths[input.Key],
                    inputFileType
                );
            }
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

        public static void
        AddPostBuildStepForCommandLineTool(
            Bam.Core.Module module,
            Bam.Core.Module moduleToAddBuildStepTo,
            out Target target,
            out Configuration configuration)
        {
            GetTargetAndConfiguration(
                moduleToAddBuildStepTo,
                out target,
                out configuration
            );

            var shellCommandLines = new Bam.Core.StringArray();
            AddModuleDirectoryCreationShellCommands(module, shellCommandLines);
            AddNewerThanPreamble(module, shellCommandLines);
            AddModuleCommandLineShellCommand(
                module,
                shellCommandLines,
                false
            );
            AddNewerThanPostamble(module, shellCommandLines);

            target.AddPostBuildCommands(
                shellCommandLines,
                configuration
            );
        }
    }
}
