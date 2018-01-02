#region License
// Copyright (c) 2010-2017, Mark Final
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
    /// Array of TokenizedStrings.
    /// </summary>
    public sealed class TokenizedStringArray :
        Array<TokenizedString>
    {
        /// <summary>
        /// Create an array of TokenizedStrings.
        /// </summary>
        public TokenizedStringArray()
        { }

        /// <summary>
        /// Create an array of TokenizedStrings from a single TokenizedString.
        /// </summary>
        /// <param name="input">Input.</param>
        public TokenizedStringArray(
            TokenizedString input)
            :
            base(new [] {input})
        { }

        /// <summary>
        /// Create an array of TokenizedStrings from an enumerable of TokenizedStrings.
        /// </summary>
        /// <param name="input">Input.</param>
        public TokenizedStringArray(
            System.Collections.Generic.IEnumerable<TokenizedString> input)
            :
            base(input)
        { }

        /// <summary>
        /// Create an array of TokenizedStrings from an array of TokenizedStrings.
        /// </summary>
        /// <param name="input">Input.</param>
        public
        TokenizedStringArray(
            params TokenizedString[] input)
            :
            base(input)
        { }

        /// <summary>
        /// Add a verbatim string to the array.
        /// </summary>
        /// <param name="item">Item.</param>
        public void
        Add(
            string item)
        {
            this.Add(Bam.Core.TokenizedString.CreateVerbatim(item));
        }

        /// <summary>
        /// Add a unique verbatim string to the array.
        /// </summary>
        /// <param name="item">Object to be added uniquely.</param>
        public void
        AddUnique(
            string item)
        {
           this.AddUnique(Bam.Core.TokenizedString.CreateVerbatim(item));
        }

        /// <summary>
        /// Find all TokenizedStrings that exist in two lists.
        /// </summary>
        /// <param name="other">The second list to intersect with.</param>
        /// <returns>The TokenizedStringArray containing just those elements in both TokenizedStringArrays.</returns>
        public TokenizedStringArray
        Intersect(
            TokenizedStringArray other)
        {
            return new TokenizedStringArray(base.Intersect(other));
        }

        /// <summary>
        /// Find all TokenizedStrings in this but not in <paramref name="other"/>
        /// </summary>
        /// <param name="other">The other TokenizedStringArray</param>
        /// <returns>The TokenizedStringArray containing the complement of the two TokenizedStringArrays.</returns>
        public TokenizedStringArray
        Complement(
            TokenizedStringArray other)
        {
            return new TokenizedStringArray(base.Complement(other));
        }

        /// <summary>
        /// If strings are parsed, then compare parsed strings, otherwise fall back on the default TokenizedString comparison.
        /// </summary>
        /// <param name="item">TokenizedString to determine if it exists in the list.</param>
        /// <returns>true if the string exists in the list; false otherwise.</returns>
        public override bool
        Contains(
            TokenizedString item)
        {
            if (item.IsParsed)
            {
                var itemString = item.ToString();
                foreach (var element in this.list)
                {
                    if (element.IsParsed)
                    {
                        if (itemString == element.ToString())
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (item.Equals(element))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            return base.Contains(item);
        }

        class ParsedTokenizedStringComparer :
            System.Collections.Generic.IEqualityComparer<TokenizedString>
        {
            bool
            System.Collections.Generic.IEqualityComparer<TokenizedString>.Equals(
                TokenizedString x,
                TokenizedString y)
            {
                return x.ToString() == y.ToString();
            }

            int
            System.Collections.Generic.IEqualityComparer<TokenizedString>.GetHashCode(
                TokenizedString obj)
            {
                return obj.ToString().GetHashCode();
            }
        }

        /// <summary>
        /// Returns an enumerable that has no duplicates, when comparing the parsed strings.
        /// Duplicates may occur when strings are finally parsed, as the TokenizedString hashes may be
        /// different due to being sourced with different modules in the same package.
        /// See https://github.com/markfinal/BuildAMation/issues/401
        /// </summary>
        /// <returns>Enumerable of the array without any duplicates.</returns>
        public System.Collections.Generic.IEnumerable<TokenizedString>
        ToEnumerableWithoutDuplicates()
        {
            return this.list.Distinct<TokenizedString>(new ParsedTokenizedStringComparer());
        }
    }
}
