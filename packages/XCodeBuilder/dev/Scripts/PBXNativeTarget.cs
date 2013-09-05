// <copyright file="PBXNativeTarget.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed class PBXNativeTarget : XCodeNodeData, IWriteableNode
    {
        public PBXNativeTarget(string name)
            : base (name)
        {
            this.BuildPhases = new System.Collections.Generic.List<PBXSourcesBuildPhase>();
        }

        public PBXFileReference ProductReference
        {
            get;
            set;
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

        public System.Collections.Generic.List<PBXSourcesBuildPhase> BuildPhases
        {
            get;
            private set;
        }

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
            writer.WriteLine("\t\t{0} /* {1} */ = {{", this.UUID, this.Name);
            writer.WriteLine("\t\t\tisa = PBXNativeTarget;");
            writer.WriteLine("\t\t\tbuildConfigurationList = {0} /* Build configuration list for PBXNativeTarget \"{1}\" */;", this.BuildConfigurationList.UUID, this.Name);
            writer.WriteLine("\t\t\tbuildPhases = (");
            foreach (var buildPhase in this.BuildPhases)
            {
                writer.WriteLine("\t\t\t\t{0} /* {1} */,", buildPhase.UUID, buildPhase.Name);
            }
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\tbuildRules = (");
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\tdependencies = (");
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\tname = {0};", this.Name);
            writer.WriteLine("\t\t\tproductName = {0};", this.Name);
            writer.WriteLine("\t\t\tproductReference = {0};", this.ProductReference.UUID);
            writer.WriteLine("\t\t\tproductType = \"com.apple.product-type.tool\";");
            writer.WriteLine("\t\t};");
        }
#endregion
    }
}
