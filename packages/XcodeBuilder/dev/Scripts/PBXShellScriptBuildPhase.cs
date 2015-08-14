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
    public sealed class PBXShellScriptBuildPhase :
        BuildPhase,
        IWriteableNode
    {
        public
        PBXShellScriptBuildPhase(
            string name,
            string moduleName) : base(name, moduleName)
        {
            this.InputPaths = new Bam.Core.StringArray();
            this.OutputPaths = new Bam.Core.StringArray();
            this.ShellPath = "/bin/sh";
            this.ShellScriptLines = new Bam.Core.StringArray();
            this.ShowEnvironmentVariablesInLog = true;
        }

        public Bam.Core.StringArray InputPaths
        {
            get;
            private set;
        }

        public Bam.Core.StringArray OutputPaths
        {
            get;
            private set;
        }

        public string ShellPath
        {
            get;
            set;
        }

        public Bam.Core.StringArray ShellScriptLines
        {
            get;
            private set;
        }

        public bool ShowEnvironmentVariablesInLog
        {
            get;
            set;
        }

#region IWriteableNode implementation

        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            writer.WriteLine("\t\t{0} /* {1} */ = {{", this.UUID, this.Name);
            writer.WriteLine("\t\t\tisa = PBXShellScriptBuildPhase;");
            writer.WriteLine("\t\t\tbuildActionMask = {0};", 2147483647); // ((2<<31)-1)
            writer.WriteLine("\t\t\tfiles = (");
            foreach (var file in this.Files)
            {
                var buildFile = file as PBXBuildFile;
                writer.WriteLine("\t\t\t\t{0} /* {1} in {2} */,", file.UUID, System.IO.Path.GetFileName(buildFile.FileReference.ShortPath), buildFile.BuildPhase.Name);
            }
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\tinputPaths = (");
            foreach (var path in this.InputPaths)
            {
                writer.WriteLine("\t\t\t\t\"{0}\",", path);
            }
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\tname = \"{0}\";", this.Name);
            writer.WriteLine("\t\t\toutputPaths = (");
            foreach (var path in this.OutputPaths)
            {
                writer.WriteLine("\t\t\t\t\"{0}\",", path);
            }
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\trunOnlyForDeploymentPostprocessing = 0;");
            writer.WriteLine("\t\t\tshellPath = {0};", this.ShellPath);
            var script = new System.Text.StringBuilder();
            foreach (var line in this.ShellScriptLines)
            {
                script.AppendFormat("{0}\\n", line);
            }
            writer.WriteLine("\t\t\tshellScript = \"{0}\";", script.ToString());
            if (!this.ShowEnvironmentVariablesInLog)
            {
                writer.WriteLine("\t\t\tshowEnvVarsInLog = 0;");
            }
            writer.WriteLine("\t\t};");
        }

#endregion
    }
}
