// <copyright file="OptionsDictionary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class OptionsDictionary : System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Opus.Core.StringArray>>
    {
        private System.Collections.Generic.SortedDictionary<string, Opus.Core.StringArray> dictionary = new System.Collections.Generic.SortedDictionary<string, Opus.Core.StringArray>();

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

        public int Count
        {
            get
            {
                return this.dictionary.Count;
            }
        }

        public override string ToString()
        {
            var builder = new System.Text.StringBuilder();
            builder.Append("{");
            foreach (var item in this.dictionary)
            {
                builder.AppendFormat("{0} = {1}; ", item.Key, SafeOptionValue(item.Value.ToString()));
            }
            builder.Append("};");
            return builder.ToString();
        }

        public static string SafeOptionValue(string value)
        {
            if (value.Contains("=") || value.Contains("$") || value.Contains(",") || value.Contains("+") || value.Contains("-"))
            {
                return "\"" + value + "\"";
            }
            return value;
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
 }
