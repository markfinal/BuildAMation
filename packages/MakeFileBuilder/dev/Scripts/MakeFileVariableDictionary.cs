// <copyright file="MakeFileVariableDictionary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MakeFileBuilder package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed class MakeFileVariableDictionary : System.Collections.Generic.Dictionary<Opus.Core.LocationKey, Opus.Core.StringArray>
    {
        public void Append(MakeFileVariableDictionary dictionary)
        {
            foreach (var itemPair in dictionary)
            {
                this.Add(itemPair.Key, itemPair.Value);
            }
        }

        public new void Add(Opus.Core.LocationKey key, Opus.Core.StringArray value)
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

#if true
#else
        public MakeFileVariableDictionary Filter(System.Enum filterKeys)
        {
            int filterKeysAsInt = System.Convert.ToInt32(filterKeys);

            MakeFileVariableDictionary filtered = new MakeFileVariableDictionary();
            foreach (System.Collections.Generic.KeyValuePair<System.Enum, Opus.Core.StringArray> pair in this)
            {
                int pairKeyAsInt = System.Convert.ToInt32(pair.Key);

                if (pairKeyAsInt == (filterKeysAsInt & pairKeyAsInt))
                {
                    filtered.Add(pair.Key, pair.Value);
                }
            }

            if (0 == filtered.Count)
            {
                throw new Opus.Core.Exception("No matching variable types were found for '{0}'", filterKeys.ToString());
            }

            return filtered;
        }
#endif
    }
}
