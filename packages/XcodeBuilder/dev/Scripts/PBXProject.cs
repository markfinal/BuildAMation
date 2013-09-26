// <copyright file="PBXProject.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class PBXProject : XCodeNodeData, IWriteableNode
    {
        public PBXProject(string name)
            : base(name)
        {
            this.NativeTargets = new PBXNativeTargetSection();
            this.FileReferences = new PBXFileReferenceSection();
            this.BuildFiles = new PBXBuildFileSection();
            this.BuildConfigurations = new XCBuildConfigurationSection();
            this.ConfigurationLists = new XCConfigurationListSection();
            this.Groups = new PBXGroupSection();
            this.SourceBuildPhases = new PBXSourcesBuildPhaseSection();
            this.CopyFilesBuildPhases = new PBXCopyFilesBuildPhaseSection();
            this.FrameworksBuildPhases = new PBXFrameworksBuildPhaseSection();
            this.TargetDependencies = new PBXTargetDependencySection();
            this.ContainerItemProxies = new PBXContainerItemProxySection();
            this.SourceFilesToBuild = new Opus.Core.Array<PBXBuildFile>();
        }

        public PBXNativeTargetSection NativeTargets
        {
            get;
            private set;
        }

        public PBXFileReferenceSection FileReferences
        {
            get;
            private set;
        }

        public PBXBuildFileSection BuildFiles
        {
            get;
            private set;
        }

        public XCBuildConfigurationSection BuildConfigurations
        {
            get;
            private set;
        }

        public XCConfigurationListSection ConfigurationLists
        {
            get;
            private set;
        }

        public XCConfigurationList BuildConfigurationList
        {
            get;
            set;
        }

        public PBXGroupSection Groups
        {
            get;
            private set;
        }

        public PBXSourcesBuildPhaseSection SourceBuildPhases
        {
            get;
            private set;
        }

        public PBXCopyFilesBuildPhaseSection CopyFilesBuildPhases
        {
            get;
            private set;
        }

        public PBXFrameworksBuildPhaseSection FrameworksBuildPhases
        {
            get;
            private set;
        }

        public PBXTargetDependencySection TargetDependencies
        {
            get;
            private set;
        }

        public PBXContainerItemProxySection ContainerItemProxies
        {
            get;
            private set;
        }

        public PBXGroup MainGroup
        {
            get;
            set;
        }

        public PBXGroup ProductsGroup
        {
            get;
            set;
        }

        public Opus.Core.Array<PBXBuildFile> SourceFilesToBuild
        {
            get;
            private set;
        }

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
            if (null == this.BuildConfigurationList)
            {
                throw new Opus.Core.Exception("Project build configuration list not assigned");
            }

            (this.BuildFiles as IWriteableNode).Write(writer);
            (this.ContainerItemProxies as IWriteableNode).Write(writer);
            (this.CopyFilesBuildPhases as IWriteableNode).Write(writer);
            (this.FileReferences as IWriteableNode).Write(writer);
            (this.FrameworksBuildPhases as IWriteableNode).Write(writer);
            (this.Groups as IWriteableNode).Write(writer);
            (this.NativeTargets as IWriteableNode).Write(writer);

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXProject section */");
            writer.WriteLine("\t\t{0} /* Project object */ = {{", this.UUID);
            writer.WriteLine("\t\t\tisa = PBXProject;");
            writer.WriteLine("\t\t\tattributes = {");
            writer.WriteLine("\t\t\t\tLastUpgradeCheck = 0460;");
            writer.WriteLine("\t\t\t\tORGANIZATIONNAME = \"Mark Final\";");
            writer.WriteLine("\t\t\t};");
            writer.WriteLine("\t\t\tbuildConfigurationList = {0} /* Build configuration list for PBXProject \"{1}\" */;", this.BuildConfigurationList.UUID, this.Name);
            writer.WriteLine("\t\t\tcompatibilityVersion = \"Xcode 3.2\";");
            writer.WriteLine("\t\t\tdevelopmentRegion = English;");
            writer.WriteLine("\t\t\thasScannedForEncodings = 0;");
            writer.WriteLine("\t\t\tknownRegions = (");
            writer.WriteLine("\t\t\t\ten,");
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\tmainGroup = {0};", this.MainGroup.UUID);
            writer.WriteLine("\t\t\tproductRefGroup = {0} /* {1} */;", this.ProductsGroup.UUID, this.ProductsGroup.Name);
            writer.WriteLine("\t\t\tprojectDirPath = \"\";");
            writer.WriteLine("\t\t\tprojectRoot = \"\";");
            writer.WriteLine("\t\t\ttargets = (");
            foreach (PBXNativeTarget target in this.NativeTargets)
            {
                writer.WriteLine("\t\t\t\t{0} /* {1} */,", target.UUID, target.Name);
            }
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t};");
            writer.WriteLine("/* End PBXProject section */");

            (this.SourceBuildPhases as IWriteableNode).Write(writer);
            (this.TargetDependencies as IWriteableNode).Write(writer);
            (this.BuildConfigurations as IWriteableNode).Write(writer);
            (this.ConfigurationLists as IWriteableNode).Write(writer);
        }
#endregion
    }
}
