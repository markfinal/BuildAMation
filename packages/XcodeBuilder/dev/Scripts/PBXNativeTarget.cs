// <copyright file="PBXNativeTarget.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class PBXNativeTarget : XCodeNodeData, IWriteableNode
    {
        public enum EType
        {
            ApplicationBundle,
            Executable,
            DynamicLibrary,
            StaticLibrary
        }

        public PBXNativeTarget(string name, EType type)
            : base (name)
        {
            this.Type = type;
            this.BuildPhases = new Opus.Core.Array<BuildPhase>();
            this.Dependencies = new Opus.Core.Array<PBXTargetDependency>();
            this.SourceFilesToBuild = new Opus.Core.Array<PBXBuildFile>();
        }

        public EType Type
        {
            get;
            private set;
        }

        public PBXFileReference ProductReference
        {
            get;
            set;
        }

        public XCConfigurationList BuildConfigurationList
        {
            get;
            set;
        }

        public Opus.Core.Array<BuildPhase> BuildPhases
        {
            get;
            private set;
        }

        public Opus.Core.Array<PBXTargetDependency> Dependencies
        {
            get;
            private set;
        }

        public Opus.Core.Array<PBXBuildFile> SourceFilesToBuild
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
            foreach (var dependency in this.Dependencies)
            {
                writer.WriteLine("\t\t\t\t{0} /* {1} */,", dependency.UUID, dependency.GetType().Name);
            }
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\tname = {0};", this.Name);
            writer.WriteLine("\t\t\tproductName = {0};", this.Name);
            writer.WriteLine("\t\t\tproductReference = {0} /* {1} */;", this.ProductReference.UUID, this.ProductReference.ShortPath);
            switch (this.Type)
            {
            case EType.ApplicationBundle:
                writer.WriteLine("\t\t\tproductType = \"com.apple.product-type.application\";");
                break;

            case EType.Executable:
                writer.WriteLine("\t\t\tproductType = \"com.apple.product-type.tool\";");
                break;

            case EType.DynamicLibrary:
                writer.WriteLine("\t\t\tproductType = \"com.apple.product-type.library.dynamic\";");
                break;

            case EType.StaticLibrary:
                writer.WriteLine("\t\t\tproductType = \"com.apple.product-type.library.static\";");
                break;

            default:
                throw new Opus.Core.Exception("Unknown product type");
            }
            writer.WriteLine("\t\t};");
        }
#endregion
    }
}
