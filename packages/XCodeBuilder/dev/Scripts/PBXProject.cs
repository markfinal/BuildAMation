// <copyright file="PBXProject.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
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

        private XCConfigurationList _BuildConfigurationList;
        public XCConfigurationList BuildConfigurationList
        {
            get
            {
                return this._BuildConfigurationList;
            }

            set
            {
                this._BuildConfigurationList = value;
                value.Owner = this;
            }
        }

        public PBXGroupSection Groups
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
            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXProject section */");
            writer.WriteLine("\t\t{0} /* Project object */ = {{", this.UUID);
            writer.WriteLine("\t\t\tisa = PBXProject;");
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

            (this.NativeTargets as IWriteableNode).Write(writer);
            (this.FileReferences as IWriteableNode).Write(writer);
            (this.BuildFiles as IWriteableNode).Write(writer);
            (this.BuildConfigurations as IWriteableNode).Write(writer);
            (this.ConfigurationLists as IWriteableNode).Write(writer);
            (this.Groups as IWriteableNode).Write(writer);
        }
#endregion
    }
}
