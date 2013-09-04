// <copyright file="XCBuildConfiguration.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed class XCBuildConfiguration : XCodeNodeData, IWriteableNode
    {
        public XCBuildConfiguration(string name, string moduleName)
            : base(name)
        {
            this.ModuleName = moduleName;
        }

        public string ModuleName
        {
            get;
            private set;
        }

#region IWriteableNode implementation

        void IWriteableNode.Write (System.IO.TextWriter writer)
        {
            writer.WriteLine("\t\t{0} /* {1} */ = {{", this.UUID, this.Name);
            writer.WriteLine("\t\t\tisa = XCBuildConfiguration;");
            writer.WriteLine("\t\t\tbuildSettings = {");
            writer.WriteLine("\t\t\t\tPRODUCT_NAME = \"$(TARGET_NAME)\";");
            writer.WriteLine("\t\t\t};");
            writer.WriteLine("\t\t\tname = {0};", this.Name);
            writer.WriteLine("\t\t};");
        }

#endregion
    }
 }
