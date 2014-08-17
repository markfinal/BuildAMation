#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion
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
