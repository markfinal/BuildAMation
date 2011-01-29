// <copyright file="OutputPaths.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class OutputPaths : System.Collections.IEnumerable
    {
        private System.Collections.Generic.Dictionary<object, string> fileDictionary = new System.Collections.Generic.Dictionary<object, string>();

        private void CheckKeyType(object key)
        {
            if (!key.GetType().IsEnum)
            {
                throw new Exception(System.String.Format("OutputFile key '{0}' is not of enum type", key.ToString()), false);
            }
        }

        public void Add(object key, string pathName)
        {
            this.CheckKeyType(key);
            this.fileDictionary[key] = pathName;
        }

        public string this[object key]
        {
            get
            {
                this.CheckKeyType(key);
                return this.fileDictionary[key];
            }

            set
            {
                this.CheckKeyType(key);
                this.fileDictionary[key] = value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.fileDictionary.GetEnumerator();
        }

        public bool Has(object key)
        {
            this.CheckKeyType(key);
            bool containsKey = this.fileDictionary.ContainsKey(key);
            return containsKey;
        }

        public void Remove(object key)
        {
            this.CheckKeyType(key);
            this.fileDictionary.Remove(key);
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