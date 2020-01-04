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
            this.editableDict = new System.Collections.Generic.Dictionary<string, TokenizedString>();
            this.Owner = owner;
        }

        private string Owner { get; set; }

        private static string
        FormattedKey(
            string key)
        {
            return $"{TokenizedString.TokenPrefix}{key}{TokenizedString.TokenSuffix}";
        }

        /// <summary>
        /// Get the macro defined by the given formatted key.
        /// An exception is thrown if the key is not found in the macrolist.
        /// </summary>
        /// <param name="key">Formatted key, beginning with $( and ending with )</param>
        /// <returns>TokenizedString associated with the given key.</returns>
        public TokenizedString
        GetFormatted(
            string key)
        {
            if (!this.editableDict.ContainsKey(key))
            {
                var message = new System.Text.StringBuilder();
                var owningModule = this.Owner ?? "unknown";
                message.AppendLine(
                    $"Module '{owningModule}', does not include a macro with the key '{key}'."
                );
                message.AppendLine("Parsed macros available:");
                foreach (var (macroName, macroString) in this.editableDict.Where(item => item.Value.IsParsed))
                {
                    message.AppendLine($"\t{macroName} -> '{macroString.ToString()}'");
                }
                message.AppendLine("Unparsed macros available:");
                foreach (var (macroName, _) in this.editableDict.Where(item => !item.Value.IsParsed))
                {
                    message.AppendLine($"\t{macroName}");
                }
                throw new Exception(message.ToString());
            }
            return this.editableDict[key];
        }

        /// <summary>
        /// Get the macro defined by the given unformatted key.
        /// An additional string allocation is made during this call.
        /// An exception is thrown if the key is not found in the macrolist.
        /// </summary>
        /// <param name="key">Unformatted key, not beginning with $( nor ending with )</param>
        /// <returns>TokenizedString associated with the given key.</returns>
        public TokenizedString
        GetUnformatted(
            string key)
        {
            return this.GetFormatted(FormattedKey(key));
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
            if (key.StartsWith(TokenizedString.TokenPrefix, System.StringComparison.Ordinal) ||
                key.EndsWith(TokenizedString.TokenSuffix, System.StringComparison.Ordinal))
            {
                throw new Exception($"Invalid macro key: {key}");
            }
            if (null == value)
            {
                throw new Exception($"Cannot assign null to macro '{key}'");
            }
            var tokenizedMacro = FormattedKey(key);
            if (value.RefersToMacro(tokenizedMacro))
            {
                throw new Exception(
                    $"Circular reference; cannnot assign macro '{key}' when it is referred to in TokenizedString or one of it's positional strings"
                );
            }
            this.editableDict[FormattedKey(key)] = value;
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
            this.editableDict.Remove(FormattedKey(key));
        }

        private System.Collections.Generic.Dictionary<string, TokenizedString> editableDict { get; set; }

        /// <summary>
        /// Query if the dictionary contains the given key-token.
        /// This is additionally expensive since a new string must be allocated.
        /// </summary>
        /// <param name="token">Token (not starting with $( nor ending with )).</param>
        public bool
        ContainsUnformatted(
            string token)
        {
            return this.editableDict.ContainsKey(FormattedKey(token));
        }

        /// <summary>
        /// Query if the dictionary contains the given key-token.
        /// </summary>
        /// <param name="token">Token (starting with $( and ending with )).</param>
        public bool
        ContainsFormatted(
            string token)
        {
            return this.editableDict.ContainsKey(token);
        }

        /// <summary>
        /// Get the enumerator of string-TokenizedString pairs for this macro list.
        /// </summary>
        /// <returns>The macrolist enumerator</returns>
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, TokenizedString>> GetEnumerator()
        {
            return this.editableDict.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.editableDict.GetEnumerator();
        }
    }
}
