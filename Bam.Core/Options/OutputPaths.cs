// <copyright file="OutputPaths.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class OutputPaths :
        System.Collections.IEnumerable
    {
        private System.Collections.Generic.SortedDictionary<System.Enum, string> fileDictionary;

        public
        OutputPaths()
        {
            this.fileDictionary = new System.Collections.Generic.SortedDictionary<System.Enum, string>();
        }

        public
        OutputPaths(
            OutputPaths source)
        {
            this.fileDictionary = new System.Collections.Generic.SortedDictionary<System.Enum, string>(source.fileDictionary);
        }

        public string this[System.Enum key]
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

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.fileDictionary.GetEnumerator();
        }

        public bool
        Has(
            System.Enum key)
        {
            bool containsKey = this.fileDictionary.ContainsKey(key);
            return containsKey;
        }

        public void
        Remove(
            System.Enum key)
        {
            this.fileDictionary.Remove(key);
        }

        public Array<System.Enum> Types
        {
            get
            {
                return new Array<System.Enum>(this.fileDictionary.Keys);
            }
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
