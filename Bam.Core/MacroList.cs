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
        public MacroList()
        {
            this.DictInternal = new System.Collections.Generic.Dictionary<string, TokenizedString>();
        }

        private static string FormattedKey(string key)
        {
            return System.String.Format("{0}{1}{2}", TokenizedString.TokenPrefix, key, TokenizedString.TokenSuffix);
        }

        public TokenizedString this[string key]
        {
            get
            {
                return this.Dict[FormattedKey(key)];
            }
            set
            {
                this.DictInternal[FormattedKey(key)] = value;
            }
        }

        public void Add(string key, TokenizedString value)
        {
            if (key.StartsWith(TokenizedString.TokenPrefix) || key.EndsWith(TokenizedString.TokenSuffix))
            {
                throw new System.Exception(System.String.Format("Invalid macro key: {0}", key));
            }
            if (null == value)
            {
                throw new System.Exception("Macro value cannot be null");
            }
            this.DictInternal[FormattedKey(key)] = value;
        }

        public void Add(string key, string value)
        {
            this.Add(key, TokenizedString.Create(value, null));
        }

        private System.Collections.Generic.Dictionary<string, TokenizedString> DictInternal
        {
            get;
            set;
        }

        public System.Collections.ObjectModel.ReadOnlyDictionary<string, TokenizedString> Dict
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyDictionary<string, TokenizedString>(this.DictInternal);
            }
        }

        public bool Contains(string token)
        {
            return this.Dict.ContainsKey(token);
        }
    }
}
