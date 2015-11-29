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
    }
}
