// <copyright file="MakeFileTargetDictionary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MakeFileBuilder package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed class MakeFileTargetDictionary : System.Collections.Generic.Dictionary<System.Enum, Opus.Core.StringArray>
    {
        public void Append(MakeFileTargetDictionary dictionary)
        {
            foreach (System.Collections.Generic.KeyValuePair<System.Enum, Opus.Core.StringArray> itemPair in dictionary)
            {
                this.Add(itemPair.Key, itemPair.Value);
            }
        }

        public new void Add(System.Enum key, Opus.Core.StringArray value)
        {
            if (this.ContainsKey(key))
            {
                this[key].AddRange(value);
            }
            else
            {
                base.Add(key, value);
            }
        }

        public void Add(System.Enum key, string value)
        {
            if (this.ContainsKey(key))
            {
                this[key].Add(value);
            }
            else
            {
                base.Add(key, new Opus.Core.StringArray(value));
            }
        }
    }
}
