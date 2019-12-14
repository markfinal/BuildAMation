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
    /// Class representing a PBXShellScriptBuildPhase in an Xcode project
    /// </summary>
    sealed class ShellScriptBuildPhase :
        BuildPhase
    {
        // There appears to be a limit on the number of output files associated with these
        // build phases. When this is exceeded, you get this useful error:
        // "Build operation failed without specifying any errors. Individual build tasks may have failed for unknown reasons."
        // 1634 seems to be the good number for single configuration, but 2 configurations seems to need at most 1600 (not sure why)
        // Continuous integration failed at 1600.
        const int maxOutputFiles = 1000;

        /// <summary>
        /// Construct an instance.
        /// </summary>
        /// <param name="configuration">Configuration for this build phase</param>
        /// <param name="name">Name of the build phase</param>
        public ShellScriptBuildPhase(
            Configuration configuration,
            string name)
            :
            base(configuration.Project, name, "PBXShellScriptBuildPhase", configuration.GUID)
        {
            this.ShellPath = "/bin/sh";
            this.ShowEnvironmentInLog = true;
            this.InputPaths = new Bam.Core.TokenizedStringArray();
            this.OutputPaths = new Bam.Core.TokenizedStringArray();
            this.AssociatedConfiguration = configuration;
        }

        /// <summary>
        /// Get the build action mask.
        /// </summary>
        protected override string BuildActionMask => "2147483647";

        /// <summary>
        /// Whether the build phase runs only for deployment post processing
        /// </summary>
        protected override bool RunOnlyForDeploymentPostprocessing => false;

        /// <summary>
        /// Get the shell script path
        /// </summary>
        public string ShellPath { get; private set; }

        /// <summary>
        /// Does the environment show in the log?
        /// </summary>
        public bool ShowEnvironmentInLog { get; private set; }

        private Bam.Core.TokenizedStringArray InputPaths { get; set; }
        private Bam.Core.TokenizedStringArray OutputPaths { get; set; }

        private Configuration AssociatedConfiguration { get; set; }

        private Bam.Core.StringArray DirectoriesToCreate { get; set; } = new Bam.Core.StringArray();
        private Bam.Core.StringArray CommandLines { get; set; } = new Bam.Core.StringArray();

        /// <summary>
        /// Add directories to create to the build phase.
        /// These are added uniquely, to reduce duplicate calls to directory creation.
        /// </summary>
        /// <param name="dirs">Enumeration of strings of directories to create.</param>
        public void
        AddDirectories(
            System.Collections.Generic.IEnumerable<string> dirs)
        {
            lock (this.DirectoriesToCreate)
            {
                this.DirectoriesToCreate.AddRangeUnique(dirs);
            }
        }

        /// <summary>
        /// Add lines of commands to the build phase.
        /// </summary>
        /// <param name="commands">Array of strings representing command lines to add.</param>
        public void
        AddCommands(
            Bam.Core.StringArray commands)
        {
            lock (this.CommandLines)
            {
                foreach (var cmd in commands)
                {
                    this.CommandLines.Add(cmd);
                }
            }
        }

        /// <summary>
        /// Add output paths to the build phase.
        /// </summary>
        /// <param name="outputPaths">Array of paths to add</param>
        public void
        AddOutputPaths(
            Bam.Core.TokenizedStringArray outputPaths)
        {
            if (null == outputPaths)
            {
                return;
            }
            lock (this.OutputPaths)
            {
                this.OutputPaths.AddRangeUnique(outputPaths);
            }
        }

        /// <summary>
        /// Query whether the build phase has sufficient space to add new data.
        /// </summary>
        /// <param name="numOutputPaths">Number of output paths to add.</param>
        /// <returns>True if there is enough space, false if not.</returns>
        public bool
        HasSufficientSpace(
            int numOutputPaths)
        {
            return this.OutputPaths.Count + numOutputPaths <= maxOutputFiles;
        }

        /// <summary>
        /// Serialize the build phase.
        /// </summary>
        /// <param name="text">StringBuilder to write to</param>
        /// <param name="indentLevel">Number of tabs to indent by.</param>
        public override void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            var indent3 = new string('\t', indentLevel + 2);
            text.AppendLine($"{indent}{this.GUID} /* {this.Name} */ = {{");
            text.AppendLine($"{indent2}isa = {this.IsA};");
            text.AppendLine($"{indent2}buildActionMask = {this.BuildActionMask};");
            if (this.BuildFiles.Any())
            {
                text.AppendLine($"{indent2}files = (");
                foreach (var file in this.BuildFiles)
                {
                    text.AppendLine($"{indent3}{file.GUID} /* FILLMEIN */,");
                }
                text.AppendLine($"{indent2});");
            }
            if (this.InputPaths.Any())
            {
                text.AppendLine($"{indent2}inputPaths = (");
                foreach (var path in this.InputPaths)
                {
                    text.AppendLine($"{indent3}\"{this.CleansePaths(path.ToString())}\",");
                }
                text.AppendLine($"{indent2});");
            }
            text.AppendLine($"{indent2}name = \"{this.Name}\";");
            if (this.OutputPaths.Any())
            {
                text.AppendLine($"{indent2}outputPaths = (");
                foreach (var path in this.OutputPaths)
                {
                    text.AppendLine($"{indent3}\"{this.CleansePaths(path.ToString())}\",");
                }
                text.AppendLine($"{indent2});");
            }
            var runOnly = this.RunOnlyForDeploymentPostprocessing ? "1" : "0";
            text.AppendLine($"{indent2}runOnlyForDeploymentPostprocessing = {runOnly};");
            text.AppendLine($"{indent2}shellPath = {this.ShellPath};");
            var scriptContent = new System.Text.StringBuilder();
            scriptContent.AppendLine("set -e"); // set -e on bash will fail the script if any command returns a non-zero exit code
            scriptContent.AppendLine("set -x"); // set -x on bash will trace the commands
            scriptContent.AppendLine($"if [ \\\"$CONFIGURATION\\\" != \\\"{this.AssociatedConfiguration.Config.ToString()}\\\" ]; then exit 0; fi");
            foreach (var dir in this.DirectoriesToCreate)
            {
                scriptContent.AppendLine($"mkdir -p {dir}");
            }
            foreach (var command in this.CommandLines)
            {
                scriptContent.AppendLine($"{command.Replace("\\", "\\\\").Replace("\"", "\\\"")}");
            }
            text.AppendLine($"{indent2}shellScript = \"{this.CleansePaths(scriptContent.ToString())}\";");
            if (!this.ShowEnvironmentInLog)
            {
                text.AppendLine($"{indent2}showEnvVarsInLog = 0;");
            }
            text.AppendLine($"{indent}}};");
        }
    }
}
