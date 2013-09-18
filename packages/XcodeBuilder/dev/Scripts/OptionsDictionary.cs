// <copyright file="OptionsDictionary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class OptionsDictionary : System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Opus.Core.StringArray>>
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
 }
