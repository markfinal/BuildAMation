#region License
// Copyright (c) 2010-2016, Mark Final
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
using System.Linq;
namespace C
{
    /// <summary>
    /// Representation of preprocessor definitions, of key-value string pairs, and the value
    /// is optional.
    /// </summary>
    public sealed class PreprocessorDefinitions :
        System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>
    {
        private System.Collections.Generic.Dictionary<string, string> Defines = new System.Collections.Generic.Dictionary<string, string>();

        public PreprocessorDefinitions()
        {}

        public PreprocessorDefinitions(
            System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>> items)
        {
            foreach (var item in items)
            {
                this.Defines.Add(item.Key, item.Value);
            }
        }

        public void
        Add(
            string name,
            string value)
        {
            if (this.Defines.ContainsKey(name))
            {
                if (this.Defines[name] != value)
                {
                    throw new Bam.Core.Exception("Preprocessor define {0} already exists with value {1}. Cannot change it to {2}", name, this.Defines[name], value);
                }
                return;
            }
            this.Defines.Add(name, value);
        }

        public void
        Add(
            string name,
            Bam.Core.TokenizedString value)
        {
            var parsedValue = value.Parse();
            if (this.Defines.ContainsKey(name))
            {
                if (this.Defines[name] != parsedValue)
                {
                    throw new Bam.Core.Exception("Preprocessor define {0} already exists with value {1}. Cannot change it to {2}", name, this.Defines[name], parsedValue);
                }
                return;
            }
            this.Defines.Add(name, parsedValue);
        }

        public void
        Add(
            string name)
        {
            this.Add(name, string.Empty);
        }

        public void
        Remove(
            string name)
        {
            if (this.Defines.ContainsKey(name))
            {
                this.Defines.Remove(name);
            }
        }

        public int Count
        {
            get
            {
                return this.Defines.Count;
            }
        }

        public bool
        Contains(
            string key)
        {
            return this.Defines.ContainsKey(key);
        }

        public string
        this[string key]
        {
            get
            {
                return this.Defines[key];
            }
        }

        public System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, string>>
        GetEnumerator()
        {
            foreach (var item in this.Defines)
            {
                yield return item;
            }
        }

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override string
        ToString()
        {
            var content = new System.Text.StringBuilder();
            foreach (var item in this.Defines)
            {
                if (System.String.IsNullOrEmpty(item.Value))
                {
                    content.AppendFormat("{0};", item.Key);
                }
                else
                {
                    content.AppendFormat("{0}={1};", item.Key, item.Value);
                }
            }
            return content.ToString();
        }

        public PreprocessorDefinitions
        Intersect(
            PreprocessorDefinitions other)
        {
            return new PreprocessorDefinitions(System.Linq.Enumerable.Intersect(this, other));
        }

        public PreprocessorDefinitions
        Complement(
            PreprocessorDefinitions other)
        {
            return new PreprocessorDefinitions(System.Linq.Enumerable.Except(this, other));
        }
    }
}
