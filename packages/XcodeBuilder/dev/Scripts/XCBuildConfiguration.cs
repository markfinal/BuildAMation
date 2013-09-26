// <copyright file="XCBuildConfiguration.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class XCBuildConfiguration : XCodeNodeData, IWriteableNode
    {
        public XCBuildConfiguration(string name, string moduleName)
            : base(name)
        {
            this.ModuleName = moduleName;
            this.Options = new OptionsDictionary();
            this.Options["PRODUCT_NAME"].AddUnique("\"$(TARGET_NAME)\"");
            this.SourceFiles = new Opus.Core.Array<PBXBuildFile>();
        }

        public string ModuleName
        {
            get;
            private set;
        }

        public OptionsDictionary Options
        {
            get;
            private set;
        }

        public Opus.Core.Array<PBXBuildFile> SourceFiles
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
            foreach (var option in this.Options)
            {
                if (option.Value.Count == 1)
                {
                    if (option.Value.Contains("="))
                    {
                        writer.WriteLine("\t\t\t\t{0} = \"{1}\";", option.Key, option.Value);
                    }
                    else
                    {
                        writer.WriteLine("\t\t\t\t{0} = {1};", option.Key, option.Value);
                    }
                }
                else
                {
                    writer.WriteLine("\t\t\t\t{0} = (", option.Key);
                    foreach (var value in option.Value)
                    {
                        if (value.Contains("="))
                        {
                            writer.WriteLine("\t\t\t\t\t\"{0}\",", value);
                        }
                        else
                        {
                            writer.WriteLine("\t\t\t\t\t{0},", value);
                        }
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
