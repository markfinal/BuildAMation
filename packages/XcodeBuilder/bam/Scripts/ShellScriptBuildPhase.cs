#region License
// Copyright (c) 2010-2015, Mark Final
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
namespace XcodeBuilder
{
    public sealed class ShellScriptBuildPhase :
        BuildPhase
    {
        public delegate string GenerateScriptDelegate(Target target);

        public ShellScriptBuildPhase(
            Target target,
            string name,
            GenerateScriptDelegate generateScript)
        {
            this.Name = name;
            this.IsA = "PBXShellScriptBuildPhase";
            this.ShellPath = "/bin/sh";
            this.ShowEnvironmentInLog = true;
            this.InputPaths = new Bam.Core.StringArray();
            this.OutputPaths = new Bam.Core.StringArray();
            this.AssociatedTarget = target;
            this.GenerateScript = generateScript;
        }

        private GenerateScriptDelegate GenerateScript;

        protected override string BuildActionMask
        {
            get
            {
                return "2147483647";
            }
        }

        protected override bool RunOnlyForDeploymentPostprocessing
        {
            get
            {
                return false;
            }
        }

        public string ShellPath
        {
            get;
            private set;
        }

        public bool ShowEnvironmentInLog
        {
            get;
            set;
        }

        private Bam.Core.StringArray InputPaths
        {
            get;
            set;
        }

        private Bam.Core.StringArray OutputPaths
        {
            get;
            set;
        }

        public Target AssociatedTarget
        {
            get;
            private set;
        }

        public override void Serialize(System.Text.StringBuilder text, int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            var indent3 = new string('\t', indentLevel + 2);
            text.AppendFormat("{0}{1} /* {2} */ = {{", indent, this.GUID, this.Name);
            text.AppendLine();
            text.AppendFormat("{0}isa = {1};", indent2, this.IsA);
            text.AppendLine();
            text.AppendFormat("{0}buildActionMask = {1};", indent2, this.BuildActionMask);
            text.AppendLine();
            if (this.BuildFiles.Count > 0)
            {
                text.AppendFormat("{0}files = (", indent2);
                text.AppendLine();
                foreach (var file in this.BuildFiles)
                {
                    text.AppendFormat("{0}{1} /* FILLMEIN */,", indent3, file.GUID);
                    text.AppendLine();
                }
                text.AppendFormat("{0});", indent2);
                text.AppendLine();
            }
            if (this.InputPaths.Count > 0)
            {
                text.AppendFormat("{0}inputPaths = (", indent2);
                text.AppendLine();
                foreach (var path in this.InputPaths)
                {
                    text.AppendFormat("{0}\"{1}\",", indent3, path);
                    text.AppendLine();
                }
                text.AppendFormat("{0});", indent2);
                text.AppendLine();
            }
            text.AppendFormat("{0}name = \"{1}\";", indent2, this.Name);
            text.AppendLine();
            if (this.OutputPaths.Count > 0)
            {
                text.AppendFormat("{0}outputPaths = (", indent2);
                text.AppendLine();
                foreach (var path in this.OutputPaths)
                {
                    text.AppendFormat("{0}\"{1}\",", indent3, path);
                    text.AppendLine();
                }
                text.AppendFormat("{0});", indent2);
                text.AppendLine();
            }
            text.AppendFormat("{0}runOnlyForDeploymentPostprocessing = {1};", indent2, this.RunOnlyForDeploymentPostprocessing ? "1" : "0");
            text.AppendLine();
            text.AppendFormat("{0}shellPath = {1};", indent2, this.ShellPath);
            text.AppendLine();
            var scriptContent = this.GenerateScript(this.AssociatedTarget);
            text.AppendFormat("{0}shellScript = \"{1}\";", indent2, scriptContent);
            text.AppendLine();
            if (!this.ShowEnvironmentInLog)
            {
                text.AppendFormat("{0}showEnvVarsInLog = 0;", indent2);
                text.AppendLine();
            }
            text.AppendFormat("{0}}};", indent);
            text.AppendLine();
        }
    }
}
