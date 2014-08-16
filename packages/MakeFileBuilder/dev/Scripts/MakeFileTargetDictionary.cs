// <copyright file="MakeFileTargetDictionary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MakeFileBuilder package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed class MakeFileTargetDictionary :
        System.Collections.Generic.Dictionary<Bam.Core.LocationKey, Bam.Core.StringArray>
    {
        public void
        Append(
            MakeFileTargetDictionary dictionary)
        {
            foreach (var itemPair in dictionary)
            {
                this.Add(itemPair.Key, itemPair.Value);
            }
        }

        public new void
        Add(
            Bam.Core.LocationKey key,
            Bam.Core.StringArray value)
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

        public void
        Add(
            Bam.Core.LocationKey key,
            string value)
        {
            if (this.ContainsKey(key))
            {
                this[key].Add(value);
            }
            else
            {
                base.Add(key, new Bam.Core.StringArray(value));
            }
        }
    }
}
