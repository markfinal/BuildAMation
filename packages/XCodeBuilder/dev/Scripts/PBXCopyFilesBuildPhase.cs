// <copyright file="PBXCopyFilesBuildPhase.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed class PBXCopyFilesBuildPhase : BuildPhase, IWriteableNode
    {
        public PBXCopyFilesBuildPhase(string name, string moduleName)
            : base(name, moduleName)
        {
        }

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
            writer.WriteLine("\t\t{0} /* {1} */ = {{", this.UUID, this.Name);
            writer.WriteLine("\t\t\tisa = PBXCopyFilesBuildPhase;");
            writer.WriteLine("\t\t\tbuildActionMask = 2147483647;");
            writer.WriteLine("\t\t\tdstPath = /usr/share/man/man1/;");
            writer.WriteLine("\t\t\tdstSubfolderSpec = 0;");
            writer.WriteLine("\t\t\tfiles = (");
            foreach (var file in this.Files)
            {
                var buildFile = file as PBXBuildFile;
                writer.WriteLine("\t\t\t\t{0} /* {1} in {2} */,", file.UUID, System.IO.Path.GetFileName(buildFile.FileReference.ShortPath), buildFile.BuildPhase.Name);
            }
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\trunOnlyForDeploymentPostprocessing = 1;");
            writer.WriteLine("\t\t};");
        }

#endregion
    }
}
