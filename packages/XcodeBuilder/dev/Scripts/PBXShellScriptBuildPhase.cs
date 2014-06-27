// <copyright file="PBXShellScriptBuildPhase.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class PBXShellScriptBuildPhase : BuildPhase, IWriteableNode
    {
        public PBXShellScriptBuildPhase(string name, string moduleName)
            : base(name, moduleName)
        {
            this.InputPaths = new Opus.Core.StringArray();
            this.OutputPaths = new Opus.Core.StringArray();
            this.ShellPath = "/bin/sh";
            this.ShellScriptLines = new Opus.Core.StringArray();
        }

        public Opus.Core.StringArray InputPaths
        {
            get;
            private set;
        }

        public Opus.Core.StringArray OutputPaths
        {
            get;
            private set;
        }

        public string ShellPath
        {
            get;
            set;
        }

        public Opus.Core.StringArray ShellScriptLines
        {
            get;
            private set;
        }

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
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
                script.AppendFormat("{0}\n", line);
            }
            writer.WriteLine("\t\t\tshellScript = \"{0}\";", script.ToString());
            writer.WriteLine("\t\t};");
        }

#endregion
    }
}
