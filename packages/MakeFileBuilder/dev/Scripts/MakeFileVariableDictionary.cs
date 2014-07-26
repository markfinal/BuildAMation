// <copyright file="MakeFileVariableDictionary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MakeFileBuilder package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed class MakeFileVariableDictionary :
        System.Collections.Generic.Dictionary<Opus.Core.LocationKey, Opus.Core.StringArray>
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
            Opus.Core.LocationKey key,
            Opus.Core.StringArray value)
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

        public Opus.Core.StringArray Variables
        {
            get
            {
                var variables = new Opus.Core.StringArray();
                foreach (var item in this.Values)
                {
                    variables.AddRange(item);
                }

                return variables;
            }
        }

        public MakeFileVariableDictionary
        Filter(
            Opus.Core.Array<Opus.Core.LocationKey> filterKeys)
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
