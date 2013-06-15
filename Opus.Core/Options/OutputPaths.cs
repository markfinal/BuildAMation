// <copyright file="OutputPaths.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class OutputPaths : System.Collections.IEnumerable
    {
        private System.Collections.Generic.SortedDictionary<System.Enum, StringArray> fileArrayDictionary;

        public OutputPaths()
        {
            this.fileArrayDictionary = new System.Collections.Generic.SortedDictionary<System.Enum, StringArray>();
        }

        public OutputPaths(OutputPaths source)
        {
            this.fileArrayDictionary = new System.Collections.Generic.SortedDictionary<System.Enum, StringArray>(source.fileArrayDictionary);
        }

        public StringArray this[System.Enum key]
        {
            get
            {
                if (this.Has(key))
                {
                    return this.fileArrayDictionary[key];
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
                    this.fileArrayDictionary[key] = value;
                }
                else if (this.Has(key))
                {
                    this.fileArrayDictionary.Remove(key);
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.fileArrayDictionary.GetEnumerator();
        }

        public bool Has(System.Enum key)
        {
            bool containsKey = this.fileArrayDictionary.ContainsKey(key);
            return containsKey;
        }

        public void Remove(System.Enum key)
        {
            this.fileArrayDictionary.Remove(key);
        }

        public Array<System.Enum> Types
        {
            get
            {
                return new Array<System.Enum>(this.fileArrayDictionary.Keys);
            }
        }

        public StringArray Paths
        {
            get
            {
                var flattenedPaths = new StringArray();
                foreach (var paths in this.fileArrayDictionary.Values)
                {
                    flattenedPaths.AddRange(paths);
                }
                return flattenedPaths;
            }
        }
    }
}