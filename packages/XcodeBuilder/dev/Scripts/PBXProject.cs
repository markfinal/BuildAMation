// <copyright file="PBXProject.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class PBXProject : XCodeNodeData, IWriteableNode
    {
        public PBXProject(Opus.Core.DependencyNode node)
            : base(node.ModuleName)
        {
            var package = node.Package;
            var projectFilename = "project.pbxproj";
            var rootDirectory = System.IO.Path.Combine(Opus.Core.State.BuildRoot, package.FullName);
            rootDirectory = System.IO.Path.Combine(rootDirectory, node.ModuleName) + ".xcodeproj";
            this.RootUri = new System.Uri(rootDirectory, System.UriKind.Absolute);
            this.Path = System.IO.Path.Combine(rootDirectory, projectFilename);

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

            this.InitializeGroups();
        }

        private void InitializeGroups()
        {
            // create a main group
            var mainGroup = this.Groups.Get(string.Empty);
            mainGroup.SourceTree = "<group>";
            this.MainGroup = mainGroup;

            // create a products group
            var productsGroup = this.Groups.Get("Products");
            productsGroup.SourceTree = "<group>";
            this.ProductsGroup = productsGroup;

            mainGroup.Children.Add(productsGroup);

            // create common build configurations for all targets
            // these settings are overriden by per-target build configurations
            var projectConfigurationList = this.ConfigurationLists.Get(this);
            this.BuildConfigurationList = projectConfigurationList;
            foreach (var config in Opus.Core.State.BuildConfigurations)
            {
                var genericBuildConfiguration = this.BuildConfigurations.Get(config.ToString(), "Generic");
                genericBuildConfiguration.Options["SYMROOT"].AddUnique(Opus.Core.State.BuildRoot);
                if (config == Opus.Core.EConfiguration.Debug)
                {
                    // Xcode 5 wants this setting for build performance in debug
                    genericBuildConfiguration.Options["ONLY_ACTIVE_ARCH"].AddUnique("YES");
                }
                projectConfigurationList.AddUnique(genericBuildConfiguration);
            }
        }

        public System.Uri RootUri
        {
            get;
            private set;
        }

        public string Path
        {
            get;
            private set;
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

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
            if (null == this.BuildConfigurationList)
            {
                throw new Opus.Core.Exception("Project build configuration list not assigned");
            }

            writer.WriteLine("// !$*UTF8*$!");
            writer.WriteLine("{");
            writer.WriteLine("\tarchiveVersion = 1;");
            writer.WriteLine("\tclasses = {");
            writer.WriteLine("\t};");
            writer.WriteLine("\tobjectVersion = 46;");
            writer.WriteLine("\tobjects = {");

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

            writer.WriteLine("\t};");
            writer.WriteLine("\trootObject = {0} /* Project object */;", this.UUID);
            writer.WriteLine("}");
        }
#endregion
    }
}
