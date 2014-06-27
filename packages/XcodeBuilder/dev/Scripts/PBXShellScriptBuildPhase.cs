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
            writer.WriteLine("\t\t\trunOnlyForDeploymentPostprocessing = 0;");
            writer.WriteLine("\t\t};");
        }

#endregion
    }
}
