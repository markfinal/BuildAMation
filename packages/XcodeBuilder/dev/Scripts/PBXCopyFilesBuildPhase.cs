// <copyright file="PBXCopyFilesBuildPhase.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class PBXCopyFilesBuildPhase : BuildPhase, IWriteableNode
    {
        public enum ESubFolder
        {
            AbsolutePath = 0,
            Wrapper = 1,
            Executables = 6,
            Resources = 7,
            Frameworks = 10,
            SharedFrameworks = 11,
            SharedSupport = 12,
            Plugins = 13,
            JavaResources = 15,
            ProductsDirectory = 16
        }

        public PBXCopyFilesBuildPhase(string name, string moduleName)
            : base(name, moduleName)
        {
            this.SubFolder = ESubFolder.Executables;
            this.CopyOnlyOnInstall = false;
            this.DestinationPath = string.Empty;
        }

        public ESubFolder SubFolder
        {
            get;
            set;
        }

        public bool CopyOnlyOnInstall
        {
            get;
            set;
        }

        public string DestinationPath
        {
            get;
            set;
        }

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
            writer.WriteLine("\t\t{0} /* {1} */ = {{", this.UUID, this.Name);
            writer.WriteLine("\t\t\tisa = PBXCopyFilesBuildPhase;");
            writer.WriteLine("\t\t\tbuildActionMask = {0};", 2147483647); // ((2<<31)-1)
            writer.WriteLine("\t\t\tdstPath = \"{0}\";", this.DestinationPath);
            writer.WriteLine("\t\t\tdstSubfolderSpec = {0};", (int)this.SubFolder);
            writer.WriteLine("\t\t\tfiles = (");
            foreach (var file in this.Files)
            {
                var buildFile = file as PBXBuildFile;
                writer.WriteLine("\t\t\t\t{0} /* {1} in {2} */,", file.UUID, System.IO.Path.GetFileName(buildFile.FileReference.ShortPath), buildFile.BuildPhase.Name);
            }
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\trunOnlyForDeploymentPostprocessing = {0};", this.CopyOnlyOnInstall ? 1 : 0);
            writer.WriteLine("\t\t};");
        }

#endregion
    }
}
