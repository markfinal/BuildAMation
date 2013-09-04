// <copyright file="PBXSourcesBuildPhase.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed class PBXSourcesBuildPhase : XCodeNodeData, IWriteableNode
    {
        public PBXSourcesBuildPhase(string name, string moduleName)
            : base(name)
        {
            this.ModuleName = moduleName;
            this.Files = new System.Collections.Generic.List<PBXBuildFile>();
        }

        public string ModuleName
        {
            get;
            set;
        }

        public System.Collections.Generic.List<PBXBuildFile> Files
        {
            get;
            private set;
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
                writer.WriteLine("\t\t\t\t{0} /* {1} */,", file.UUID, System.IO.Path.GetFileName((file as PBXBuildFile).FileReference.Path));
            }
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\trunOnlyForDeploymentPostprocessing = 0;");
            writer.WriteLine("\t\t};");
        }

#endregion
    }
}
