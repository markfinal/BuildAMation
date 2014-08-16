// <copyright file="MakeFileVariableDictionary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MakeFileBuilder package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed class MakeFileVariableDictionary :
        System.Collections.Generic.Dictionary<Bam.Core.LocationKey, Bam.Core.StringArray>
    {
        public void
        Append(
            MakeFileVariableDictionary dictionary)
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

        public Bam.Core.StringArray Variables
        {
            get
            {
                var variables = new Bam.Core.StringArray();
                foreach (var item in this.Values)
                {
                    variables.AddRange(item);
                }

                return variables;
            }
        }

        public MakeFileVariableDictionary
        Filter(
            Bam.Core.Array<Bam.Core.LocationKey> filterKeys)
        {
            var filteredDictionary = new MakeFileVariableDictionary();
            foreach (var key in this.Keys)
            {
                if (filterKeys.Contains(key))
                {
                    filteredDictionary.Add(key, this[key]);
                }
            }
            return filteredDictionary;
        }
    }
}
