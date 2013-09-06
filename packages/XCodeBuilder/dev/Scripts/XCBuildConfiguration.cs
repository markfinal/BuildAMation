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
            this.Options = new System.Collections.Generic.Dictionary<string, string>();
            this.Options["PRODUCT_NAME"] = "\"$(TARGET_NAME)\"";
        }

        public string ModuleName
        {
            get;
            private set;
        }

        // TODO: the value can be a list of values
        public System.Collections.Generic.Dictionary<string, string> Options
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
            // TODO: ideally the options dictionary should be sorted alphabetically
            foreach (var option in this.Options)
            {
                writer.WriteLine("\t\t\t\t{0} = {1};", option.Key, option.Value);
            }
            writer.WriteLine("\t\t\t};");
            writer.WriteLine("\t\t\tname = {0};", this.Name);
            writer.WriteLine("\t\t};");
        }

#endregion
    }
 }
