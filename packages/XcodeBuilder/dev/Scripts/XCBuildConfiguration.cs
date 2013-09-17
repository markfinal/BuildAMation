// <copyright file="XCBuildConfiguration.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class XCBuildConfiguration : XCodeNodeData, IWriteableNode
    {
        public class OptionsDictionary : System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Opus.Core.StringArray>>
        {
            private System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> dictionary = new System.Collections.Generic.Dictionary<string, Opus.Core.StringArray>();

            public Opus.Core.StringArray this[string key]
            {
                get
                {
                    if (!this.dictionary.ContainsKey(key))
                    {
                        this.dictionary[key] = new Opus.Core.StringArray();
                    }

                    return this.dictionary[key];
                }
            }

            #region IEnumerable implementation

            System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, Opus.Core.StringArray>> System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Opus.Core.StringArray>>.GetEnumerator ()
            {
                return this.dictionary.GetEnumerator();
            }

            #endregion

            #region IEnumerable implementation

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return (this as System.Collections.IEnumerable).GetEnumerator();
            }

            #endregion
        }

        public XCBuildConfiguration(string name, string moduleName)
            : base(name)
        {
            this.ModuleName = moduleName;
            this.Options = new OptionsDictionary();
            this.Options["PRODUCT_NAME"].AddUnique("\"$(TARGET_NAME)\"");
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
