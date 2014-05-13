// <copyright file="VisualStudioToolAttributeDictionary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualStudioProcessor package</summary>
// <author>Mark Final</author>
namespace VisualStudioProcessor
{
    public sealed class ToolAttributeDictionary : System.Collections.Generic.Dictionary<string, string>
    {
        public void Merge(ToolAttributeDictionary dictionary)
        {
            // TODO: can use a var? even on mono?
            foreach (System.Collections.Generic.KeyValuePair<string, string> option in dictionary)
            {
                var attributeName = option.Key;
                var attributeValue = option.Value;
                if (this.ContainsKey(attributeName))
                {
                    var updatedAttributeValue = new System.Text.StringBuilder(this[attributeName]);
                    var splitter = attributeValue.ToString()[attributeValue.Length - 1];
                    if (System.Char.IsLetterOrDigit(splitter))
                    {
                        throw new Opus.Core.Exception("Splitter character is a letter or digit");
                    }
                    var splitNew = attributeValue.ToString().Split(splitter);
                    foreach (var split in splitNew)
                    {
                        if (!System.String.IsNullOrEmpty(split) && !this[attributeName].Contains(split))
                        {
                            updatedAttributeValue.AppendFormat("{0}{1}", split, splitter);
                        }
                    }
                    this[attributeName] = updatedAttributeValue.ToString();
                }
                else
                {
                    this[attributeName] = attributeValue.ToString();
                }
            }
        }
    }
}
