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
namespace XcodeBuilder
{
    /// <summary>
    /// Support classes for Xcode projet generation
    /// </summary>
    static partial class Support
    {
        private static void
        AddModuleDirectoryCreationShellCommands(
            Bam.Core.Module module,
            Bam.Core.StringArray shellCommandLines)
        {
            foreach (var dir in module.OutputDirectories)
            {
                var escapedDir = Bam.Core.IOWrapper.EscapeSpacesInPath(dir.ToString());
                shellCommandLines.Add($"[[ ! -d {escapedDir} ]] && mkdir -p {escapedDir}");
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
                condition_text.Append($"! -e {output_path} ");
                foreach (var (inputModule, inputPathKey) in module.InputModulePaths)
                {
                    if (!inputModule.GeneratedPaths.Any())
                    {
                        continue;
                    }
                    var input_path = Bam.Core.IOWrapper.EscapeSpacesInPath(inputModule.GeneratedPaths[inputPathKey].ToString());
                    condition_text.Append($"|| {input_path} -nt {output_path} ");
                }
                if (output != last_output)
                {
                    condition_text.Append("|| ");
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

        private static void
        AddRedirectToFile(
            Bam.Core.TokenizedString outputFile,
            Bam.Core.StringArray shellCommandLines)
        {
            var command = shellCommandLines.Last();
            shellCommandLines.Remove(command);
            var redirectedCommand = command + $" > {outputFile.ToString()}";
            shellCommandLines.Add(redirectedCommand);
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
                    $"Command line tool passed with module '{module.ToString()}' is invalid"
                );
            }
            System.Diagnostics.Debug.Assert(module.Tool is Bam.Core.ICommandLineTool);

            var args = new Bam.Core.StringArray();
            if (module.WorkingDirectory != null)
            {
                args.Add(
                    $"cd {module.WorkingDirectory.ToStringQuoteIfNecessary()} &&"
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
            var encapsulating = module.EncapsulatingModule;
#if D_PACKAGE_PUBLISHER
            if (encapsulating is Publisher.Collation asCollation)
            {
                asCollation.ForEachAnchor(
                    (collation, anchor, customData) =>
                    {
                        encapsulating = anchor.SourceModule;
                    },
                    null
                );
            }
#endif

            var workspace = Bam.Core.Graph.Instance.MetaData as XcodeBuilder.WorkspaceMeta;
            var project = workspace.EnsureProjectExists(encapsulating, encapsulating.PackageDefinition.FullName);
            target = workspace.EnsureTargetExists(encapsulating, project);
            configuration = target.GetConfiguration(encapsulating);
        }

        /// <summary>
        /// Add pre build commands to the target.
        /// </summary>
        /// <param name="module">Module associated with</param>
        /// <param name="target">Target to add the commands to</param>
        /// <param name="configuration">Configuration within the Target</param>
        /// <param name="commandLine">Command line to add</param>
        /// <param name="outputPaths">Any output paths to add.</param>
        public static void
        AddPreBuildCommands(
            Bam.Core.Module module,
            Target target,
            Configuration configuration,
            string commandLine,
            Bam.Core.TokenizedStringArray outputPaths = null)
        {
            var shellCommandLines = new Bam.Core.StringArray();
            AddModuleDirectoryCreationShellCommands(module, shellCommandLines);
            AddNewerThanPreamble(module, shellCommandLines);
            shellCommandLines.Add($"\techo {commandLine}");
            shellCommandLines.Add($"\t{commandLine}");
            AddNewerThanPostamble(module, shellCommandLines);

            target.AddPreBuildCommands(
                shellCommandLines,
                configuration,
                outputPaths
            );
        }

        /// <summary>
        /// Add a pre build step corresponding to a command line tool.
        /// </summary>
        /// <param name="module">Module associated with</param>
        /// <param name="target">Target written to</param>
        /// <param name="configuration">Configuration written to</param>
        /// <param name="checkForNewer">Perform a newer check on files</param>
        /// <param name="allowNonZeroSuccessfulExitCodes">Allow a non-zero exit code to be successful</param>
        /// <param name="addOrderOnlyDependencyOnTool">Adding an order only dependency</param>
        /// <param name="outputPaths">Add output paths</param>
        /// <param name="redirectToFile">Redirect to a file</param>
        public static void
        AddPreBuildStepForCommandLineTool(
            Bam.Core.Module module,
            out Target target,
            out Configuration configuration,
            bool checkForNewer,
            bool allowNonZeroSuccessfulExitCodes,
            bool addOrderOnlyDependencyOnTool = false,
            Bam.Core.TokenizedStringArray outputPaths = null,
            Bam.Core.TokenizedString redirectToFile = null)
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
            if (null != redirectToFile)
            {
                AddRedirectToFile(redirectToFile, shellCommandLines);
            }
            if (checkForNewer)
            {
                AddNewerThanPostamble(module, shellCommandLines);
            }

            target.AddPreBuildCommands(
                shellCommandLines,
                configuration,
                outputPaths
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
                if (tool.MetaData is Target toolTarget)
                {
                    target.Requires(toolTarget);
                }
            }
        }

        /// <summary>
        /// Add a prebuild step for command line tool.
        /// </summary>
        /// <param name="module">Module associated with</param>
        /// <param name="target">Target written to</param>
        /// <param name="configuration">Configuration written to</param>
        /// <param name="inputFileType">The file type of the input</param>
        /// <param name="checkForNewer">Is there a check for a newer file?</param>
        /// <param name="allowNonZeroSuccessfulExitCodes">Allow non-zero exit codes to be successful</param>
        /// <param name="addOrderOnlyDependencyOnTool">Add an order only dependency</param>
        /// <param name="outputPaths">Add output paths</param>
        public static void
        AddPreBuildStepForCommandLineTool(
            Bam.Core.Module module,
            out Target target,
            out Configuration configuration,
            FileReference.EFileType inputFileType,
            bool checkForNewer,
            bool allowNonZeroSuccessfulExitCodes,
            bool addOrderOnlyDependencyOnTool = false,
            Bam.Core.TokenizedStringArray outputPaths = null)
        {
            AddPreBuildStepForCommandLineTool(
                module,
                out target,
                out configuration,
                checkForNewer,
                allowNonZeroSuccessfulExitCodes,
                addOrderOnlyDependencyOnTool: addOrderOnlyDependencyOnTool,
                outputPaths: outputPaths
            );
            foreach (var (inputModule,inputPathKey) in module.InputModulePaths)
            {
                target.EnsureFileOfTypeExists(
                    inputModule.GeneratedPaths[inputPathKey],
                    inputFileType
                );
            }
        }

        /// <summary>
        /// Add post build commands
        /// </summary>
        /// <param name="module">Module associated with</param>
        /// <param name="target">Target added to</param>
        /// <param name="configuration">Configuration added to</param>
        /// <param name="customCommands">Custom command lines</param>
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

        /// <summary>
        /// Add a post build step for a command line tool
        /// </summary>
        /// <param name="module">Module associated with</param>
        /// <param name="moduleToAddBuildStepTo">Module to add the build step to</param>
        /// <param name="target">Target written to</param>
        /// <param name="configuration">Configuration written to</param>
        /// <param name="redirectToFile">Is the output redirected to file?</param>
        public static void
        AddPostBuildStepForCommandLineTool(
            Bam.Core.Module module,
            Bam.Core.Module moduleToAddBuildStepTo,
            out Target target,
            out Configuration configuration,
            Bam.Core.TokenizedString redirectToFile = null)
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
            if (null != redirectToFile)
            {
                AddRedirectToFile(redirectToFile, shellCommandLines);
            }
            AddNewerThanPostamble(module, shellCommandLines);

            target.AddPostBuildCommands(
                shellCommandLines,
                configuration
            );
        }
    }
}
