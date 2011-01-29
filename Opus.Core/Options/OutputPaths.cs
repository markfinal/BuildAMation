// <copyright file="OutputPaths.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class OutputPaths : System.Collections.IEnumerable
    {
        private System.Collections.Generic.Dictionary<FlagsBase, string> fileDictionary = new System.Collections.Generic.Dictionary<FlagsBase, string>();

        public string this[FlagsBase key]
        {
            get
            {
                if (this.Has(key))
                {
                    return this.fileDictionary[key];
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value != null)
                {
                    this.fileDictionary[key] = value;
                }
                else if (this.Has(key))
                {
                    this.fileDictionary.Remove(key);
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.fileDictionary.GetEnumerator();
        }

        public bool Has(FlagsBase key)
        {
            bool containsKey = this.fileDictionary.ContainsKey(key);
            return containsKey;
        }

        public StringArray Paths
        {
            get
            {
                return new StringArray(this.fileDictionary.Values);
            }
        }
    }
}