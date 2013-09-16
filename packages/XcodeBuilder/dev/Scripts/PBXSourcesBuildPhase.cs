// <copyright file="PBXSourcesBuildPhase.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class PBXSourcesBuildPhase : BuildPhase, IWriteableNode
    {
        public PBXSourcesBuildPhase(string name, string moduleName)
            : base(name, moduleName)
        {
        }

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
            if (0 == this.Files.Count)
            {
                return;
            }

            writer.WriteLine("\t\t{0} /* {1} */ = {{", this.UUID, this.Name);
            writer.WriteLine("\t\t\tisa = PBXSourcesBuildPhase;");
            writer.WriteLine("\t\t\tbuildActionMask = 2147483647;");
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
