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
            this.Options = new System.Collections.Generic.Dictionary<string, Opus.Core.StringArray>();
            this.Options["PRODUCT_NAME"] = new Opus.Core.StringArray("\"$(TARGET_NAME)\"");
        }

        public string ModuleName
        {
            get;
            private set;
        }

        public System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> Options
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
                if (option.Value.Count == 1)
                {
                    writer.WriteLine("\t\t\t\t{0} = {1};", option.Key, option.Value);
                }
                else
                {
                    writer.WriteLine("\t\t\t\t{0} = (", option.Key);
                    foreach (var value in option.Value)
                    {
                        writer.WriteLine("\t\t\t\t\t{0},", value);
                    }
                    writer.WriteLine("\t\t\t\t);");
                }
            }
            writer.WriteLine("\t\t\t};");
            writer.WriteLine("\t\t\tname = {0};", this.Name);
            writer.WriteLine("\t\t};");
        }

#endregion
    }
 }
