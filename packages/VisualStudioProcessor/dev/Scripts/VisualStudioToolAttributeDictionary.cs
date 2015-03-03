#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
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

        public void
        EnableCanInherit(
            string key)
        {
            this.canInherit[key] = true;
        }

        public bool
        CanInherit(
            string key)
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
                        throw new Bam.Core.Exception("Splitter character is a letter or digit");
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

        System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, string>>
        System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>.GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
