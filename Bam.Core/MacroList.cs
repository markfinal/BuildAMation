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
namespace Bam.Core
{
    /// <summary>
    /// Collection of key-values pairs representing macro replacement in strings (usually paths)
    /// </summary>
    public sealed class MacroList :
        System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, TokenizedString>>
    {
        /// <summary>
        /// Construct an instance of a MacroList.
        /// Optionally define the name of the owner of this list.
        /// </summary>
        /// <param name="owner">Optional name of the object owning this list</param>
        public MacroList(
            string owner = null)
        {
            this.TokenToMacroDict = new System.Collections.Generic.Dictionary<string, TokenizedString>();
            this.Owner = owner;
        }

        private string Owner { get; set; }

        private static string
        NameToToken(
            string name)
        {
            return $"{TokenizedString.TokenPrefix}{name}{TokenizedString.TokenSuffix}";
        }

        /// <summary>
        /// Get the macro defined by the given token.
        /// An exception is thrown if the token is not found in the macrolist.
        /// </summary>
        /// <param name="token">Token, beginning with $( and ending with )</param>
        /// <returns>TokenizedString associated with the given token.</returns>
        public TokenizedString
        FromToken(
            string token)
        {
            if (!this.TokenToMacroDict.ContainsKey(token))
            {
                var message = new System.Text.StringBuilder();
                var owningModule = this.Owner ?? "unknown";
                message.AppendLine(
                    $"Module '{owningModule}', does not include a macro with the key '{token}'."
                );
                message.AppendLine("Parsed macros available:");
                foreach (var (macroName, macroString) in this.TokenToMacroDict.Where(item => item.Value.IsParsed))
                {
                    message.AppendLine($"\t{macroName} -> '{macroString.ToString()}'");
                }
                message.AppendLine("Unparsed macros available:");
                foreach (var (macroName, _) in this.TokenToMacroDict.Where(item => !item.Value.IsParsed))
                {
                    message.AppendLine($"\t{macroName}");
                }
                throw new Exception(message.ToString());
            }
            return this.TokenToMacroDict[token];
        }

        /// <summary>
        /// Get the macro defined by the given key name (no token markup).
        /// An additional string allocation is made during this call.
        /// An exception is thrown if the name is not found in the macrolist.
        /// </summary>
        /// <param name="name">Name of macro without token markup, i.e. not beginning with $( nor ending with )</param>
        /// <returns>TokenizedString associated with the given name.</returns>
        public TokenizedString
        FromName(
            string name)
        {
            return this.FromToken(NameToToken(name));
        }

        /// <summary>
        /// Add the TokenizedString against the key name provided.
        /// An additional string allocation is made.
        /// </summary>
        /// <param name="name">Key name. No token markup, must not start with $( nor end with ).</param>
        /// <param name="macroString">The TokenizedString macro to associate with the name.</param>
        public void
        Add(
            string name,
            TokenizedString macroString)
        {
            if (null == macroString)
            {
                throw new Exception($"Cannot assign null to macro key name '{name}'");
            }
            if (name.StartsWith(TokenizedString.TokenPrefix, System.StringComparison.Ordinal) ||
                name.EndsWith(TokenizedString.TokenSuffix, System.StringComparison.Ordinal))
            {
                throw new Exception($"Invalid macro key name: {name}");
            }
            var token = NameToToken(name);
            if (macroString.RefersToMacro(token))
            {
                throw new Exception(
                    $"Circular reference; cannnot assign macro '{name}' when it is referred to in TokenizedString or one of it's positional strings"
                );
            }
            this.TokenToMacroDict[NameToToken(name)] = macroString;
        }

        /// <summary>
        /// Add a verbatim macro.
        /// </summary>
        /// <param name="name">Key name. No token markup, must not start with $( nor end with ).</param>
        /// <param name="macroString">The TokenizedString macro to associate with the name.</param>
        /// <param name="cached">Optional whether to use TokenizedString caching. Default to true.</param>
        public void
        AddVerbatim(
            string name,
            string macroString,
            bool cached = true)
        {
            this.Add(name, cached ? TokenizedString.CreateVerbatim(macroString) : TokenizedString.CreateUncachedVerbatim(macroString));
        }

        /// <summary>
        /// Remove the macro associated with the provided key name.
        /// An additional string allocation is made.
        /// </summary>
        /// <param name="name">Key name. No token markup, must not start with $( nor end with ).</param>
        public void
        Remove(
            string name)
        {
            this.TokenToMacroDict.Remove(NameToToken(name));
        }

        private System.Collections.Generic.Dictionary<string, TokenizedString> TokenToMacroDict { get; set; }

        /// <summary>
        /// Query if the dictionary contains the given key name..
        /// This is additionally expensive since a new string must be allocated.
        /// </summary>
        /// <param name="name">Name of key to look up macro for (without token markup, i.e. not starting with $( nor ending with )).</param>
        public bool
        ContainsName(
            string name)
        {
            return this.TokenToMacroDict.ContainsKey(NameToToken(name));
        }

        /// <summary>
        /// Query if the dictionary contains the given key-token.
        /// </summary>
        /// <param name="token">Token (starting with $( and ending with )).</param>
        public bool
        ContainsToken(
            string token)
        {
            return this.TokenToMacroDict.ContainsKey(token);
        }

        /// <summary>
        /// Get the enumerator of token-TokenizedString pairs for this macro list.
        /// </summary>
        /// <returns>The macrolist enumerator</returns>
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, TokenizedString>> GetEnumerator()
        {
            return this.TokenToMacroDict.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.TokenToMacroDict.GetEnumerator();
        }
    }
}
