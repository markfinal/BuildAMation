// <copyright file="VisualStudioToolAttributeDictionary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualStudioProcessor package</summary>
// <author>Mark Final</author>
namespace VisualStudioProcessor
{
    public sealed class ToolAttributeDictionary :
        System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string,string>>
    {
        private System.Collections.Generic.Dictionary<string, string> dictionary = new System.Collections.Generic.Dictionary<string, string>();
        private System.Collections.Generic.Dictionary<string, bool> canInherit = new System.Collections.Generic.Dictionary<string, bool>();

        public void
        Add(
            string key,
            string value)
        {
            this.dictionary.Add(key, value);
            this.canInherit.Add(key, false);
        }

        public void EnableCanInherit(string key)
        {
            this.canInherit[key] = true;
        }

        public bool CanInherit(string key)
        {
            return this.canInherit[key];
        }

        public void
        Merge(
            ToolAttributeDictionary dictionary)
        {
            // TODO: can use a var? even on mono?
            foreach (System.Collections.Generic.KeyValuePair<string, string> option in dictionary)
            {
                var attributeName = option.Key;
                var attributeValue = option.Value;
                if (this.dictionary.ContainsKey(attributeName))
                {
                    var updatedAttributeValue = new System.Text.StringBuilder(this.dictionary[attributeName]);
                    var splitter = attributeValue.ToString()[attributeValue.Length - 1];
                    if (System.Char.IsLetterOrDigit(splitter))
                    {
                        throw new Opus.Core.Exception("Splitter character is a letter or digit");
                    }
                    var splitNew = attributeValue.ToString().Split(splitter);
                    foreach (var split in splitNew)
                    {
                        if (!System.String.IsNullOrEmpty(split) && !this.dictionary[attributeName].Contains(split))
                        {
                            updatedAttributeValue.AppendFormat("{0}{1}", split, splitter);
                        }
                    }
                    this.dictionary[attributeName] = updatedAttributeValue.ToString();
                }
                else
                {
                    this.dictionary[attributeName] = attributeValue.ToString();
                }
                this.canInherit[attributeName] = dictionary.canInherit[attributeName];
            }
        }

        #region IEnumerable<KeyValuePair<string,string>> Members

        System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, string>> System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>.GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
