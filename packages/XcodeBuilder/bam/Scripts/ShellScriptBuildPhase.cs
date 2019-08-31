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
        /// <summary>
        /// Delegate for generating scripts.
        /// </summary>
        /// <param name="target">Target associated with the script</param>
        /// <returns>Delegate return</returns>
        public delegate string GenerateScriptDelegate(Target target);

        /// <summary>
        /// Construct an instance.
        /// </summary>
        /// <param name="target">Target containing the build phase.</param>
        /// <param name="name">Name of the build phase</param>
        /// <param name="generateScript">Delegate to generate the script.</param>
        public ShellScriptBuildPhase(
            Target target,
            string name,
            GenerateScriptDelegate generateScript)
            :
            base(target.Project, name, "PBXShellScriptBuildPhase", target.GUID, generateScript.ToString())
        {
            this.ShellPath = "/bin/sh";
            this.ShowEnvironmentInLog = true;
            this.InputPaths = new Bam.Core.TokenizedStringArray();
            this.OutputPaths = new Bam.Core.TokenizedStringArray();
            this.AssociatedTarget = target;
            this.GenerateScript = generateScript;
        }

        private readonly GenerateScriptDelegate GenerateScript;

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

        /// <summary>
        /// Get the associated Target to the build phase.
        /// </summary>
        public Target AssociatedTarget { get; private set; }

        /// <summary>
        /// Add output paths to the build phase.
        /// </summary>
        /// <param name="outputPaths">Paths to add</param>
        public void
        AddOutputPaths(
            Bam.Core.TokenizedStringArray outputPaths)
        {
            lock (this.OutputPaths)
            {
                this.OutputPaths.AddRangeUnique(outputPaths);
            }
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
            scriptContent.AppendLine(this.GenerateScript(this.AssociatedTarget));
            text.AppendLine($"{indent2}shellScript = \"{this.CleansePaths(scriptContent.ToString())}\";");
            if (!this.ShowEnvironmentInLog)
            {
                text.AppendLine($"{indent2}showEnvVarsInLog = 0;");
            }
            text.AppendLine($"{indent}}};");
        }
    }
}
