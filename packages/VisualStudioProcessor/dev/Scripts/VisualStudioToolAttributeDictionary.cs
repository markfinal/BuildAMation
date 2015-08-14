#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
