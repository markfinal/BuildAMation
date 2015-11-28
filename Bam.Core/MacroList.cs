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
namespace Bam.Core
{
    /// <summary>
    /// Container of key-values pairs representing macro replacement in strings (usually paths)
    /// </summary>
    public sealed class MacroList
    {
        /// <summary>
        /// Construct an instance of a MacroList.
        /// </summary>
        public MacroList()
        {
            this.DictInternal = new System.Collections.Generic.Dictionary<string, TokenizedString>();
        }

        private static string
        FormattedKey(
            string key)
        {
            return System.String.Format("{0}{1}{2}", TokenizedString.TokenPrefix, key, TokenizedString.TokenSuffix);
        }

        /// <summary>
        /// Get or set the macro defined by the given key.
        /// </summary>
        /// <param name="key">Key.</param>
        public TokenizedString this[string key]
        {
            get
            {
                var fKey = FormattedKey(key);
                if (!this.Dict.ContainsKey(fKey))
                {
                    throw new Exception("Unable to locate macro '{0}'", fKey);
                }
                return this.Dict[fKey];
            }
            set
            {
                this.DictInternal[FormattedKey(key)] = value;
            }
        }

        /// <summary>
        /// Add the TokenizedString against the key provided.
        /// </summary>
        /// <param name="key">Key. Must not start with $( nor end with ).</param>
        /// <param name="value">Value.</param>
        public void
        Add(
            string key,
            TokenizedString value)
        {
            if (key.StartsWith(TokenizedString.TokenPrefix) || key.EndsWith(TokenizedString.TokenSuffix))
            {
                throw new Exception("Invalid macro key: {0}", key);
            }
            if (null == value)
            {
                throw new Exception("Cannot assign null to macro '{0}'", key);
            }
            this.DictInternal[FormattedKey(key)] = value;
        }

        /// <summary>
        /// Add a non-verbatim macro.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public void
        Add(
            string key,
            string value)
        {
            this.Add(key, TokenizedString.Create(value, null));
        }

        /// <summary>
        /// Add a verbatim macro.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public void
        AddVerbatim(
            string key,
            string value)
        {
            this.Add(key, TokenizedString.CreateVerbatim(value));
        }

        /// <summary>
        /// Remove the macro associated with the provided key.
        /// </summary>
        /// <param name="key">Key.</param>
        public void
        Remove(
            string key)
        {
            this.DictInternal.Remove(FormattedKey(key));
        }

        private System.Collections.Generic.Dictionary<string, TokenizedString> DictInternal
        {
            get;
            set;
        }

        /// <summary>
        /// Obtain a read-only instance of the key-value pair dictionary.
        /// </summary>
        /// <value>The dict.</value>
        public System.Collections.ObjectModel.ReadOnlyDictionary<string, TokenizedString> Dict
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyDictionary<string, TokenizedString>(this.DictInternal);
            }
        }

        /// <summary>
        /// Query if the dictionary contains the given key-token.
        /// </summary>
        /// <param name="token">Token (not starting with $( nor ending with )).</param>
        public bool
        Contains(
            string token)
        {
            return this.Dict.ContainsKey(FormattedKey(token));
        }
    }
}
