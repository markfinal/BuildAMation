#region License
// Copyright (c) 2010-2019, Mark Final
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
        System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Bam.Core.TokenizedString>>
    {
        private readonly System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedString> Defines = new System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedString>();

        /// <summary>
        /// Default constructor.
        /// This might look unusually empty, but a default constructor is needed to initialise Settings classes
        /// </summary>
        public PreprocessorDefinitions()
        {}

        /// <summary>
        /// Construct an instance
        /// </summary>
        /// <param name="items">Source key-value pairs to initialise from</param>
        public PreprocessorDefinitions(
            System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Bam.Core.TokenizedString>> items)
        {
            foreach (var item in items)
            {
                this.Defines.Add(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Add a key-value pair
        /// </summary>
        /// <param name="name">Preprocessor define name</param>
        /// <param name="value">Associated value</param>
        public void
        Add(
            string name,
            string value)
        {
            var valueTS = Bam.Core.TokenizedString.CreateVerbatim(value);
            if (this.Defines.ContainsKey(name))
            {
                // compares hashes
                if (this.Defines[name] != valueTS)
                {
                    throw new Bam.Core.Exception(
                        $"Preprocessor define {name} already exists with a different value to {valueTS.ToString()}"
                    );
                }
                return;
            }
            this.Defines.Add(name, valueTS);
        }

        /// <summary>
        /// Add a key-value pair
        /// </summary>
        /// <param name="name">Preprocessor define name</param>
        /// <param name="value">Associated value</param>
        public void
        Add(
            string name,
            Bam.Core.TokenizedString value)
        {
            if (this.Defines.ContainsKey(name))
            {
                // compares hashes
                if (this.Defines[name] != value)
                {
                    throw new Bam.Core.Exception(
                        $"Preprocessor define {name} already exists with a different value"
                    );
                }
                return;
            }
            this.Defines.Add(name, value);
        }

        /// <summary>
        /// Add a key
        /// </summary>
        /// <param name="name">Preprocessor define name</param>
        public void
        Add(
            string name) => this.Add(name, null as Bam.Core.TokenizedString);

        /// <summary>
        /// Remove a key
        /// </summary>
        /// <param name="name">Preprocessor define name</param>
        public void
        Remove(
            string name)
        {
            if (this.Defines.ContainsKey(name))
            {
                this.Defines.Remove(name);
            }
        }

        /// <summary>
        /// Number of definitions
        /// </summary>
        public int Count => this.Defines.Count;

        /// <summary>
        /// Determine if this key is contained
        /// </summary>
        /// <param name="key">Preprocessor define name</param>
        /// <returns>true if the key is contained</returns>
        public bool
        Contains(
            string key) => this.Defines.ContainsKey(key);

        /// <summary>
        /// Indexer to get definition value for the specified key.
        /// </summary>
        /// <param name="key">Preprocessor define name</param>
        /// <returns>Associated value</returns>
        public Bam.Core.TokenizedString this[string key] => this.Defines[key];

        /// <summary>
        /// Get the enumerator for this instance
        /// </summary>
        /// <returns>The enumerator</returns>
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, Bam.Core.TokenizedString>>
        GetEnumerator()
        {
            foreach (var item in this.Defines)
            {
                yield return item;
            }
        }

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Convert the preprocessor define (and optional value) into a string.
        /// Note that for defines with a value, the value is assumed to be Parsed() already, or
        /// an error will occur.
        /// </summary>
        /// <returns>Strings of the form 'key' or 'key=value'.</returns>
        public override string
        ToString()
        {
            var content = new System.Text.StringBuilder();
            foreach (var item in this.Defines)
            {
                if (null == item.Value)
                {
                    content.Append($"{item.Key};");
                }
                else
                {
                    content.Append($"{item.Key}={item.Value.ToString()};");
                }
            }
            return content.ToString();
        }

        /// <summary>
        /// Get the intersection of preprocessor definitions
        /// </summary>
        /// <param name="other">Another set of preprocessor definitions</param>
        /// <returns>Intersection of defines</returns>
        public PreprocessorDefinitions
        Intersect(
            PreprocessorDefinitions other) => new PreprocessorDefinitions(System.Linq.Enumerable.Intersect(this, other));

        /// <summary>
        /// Get the complement of preprocessor definitions
        /// </summary>
        /// <param name="other">Another set of preprocessor definitions</param>
        /// <returns>Complement of defines</returns>
        public PreprocessorDefinitions
        Complement(
            PreprocessorDefinitions other) => new PreprocessorDefinitions(System.Linq.Enumerable.Except(this, other));
    }
}
