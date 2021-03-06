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
namespace VSSolutionBuilder
{
    /// <summary>
    /// Make a GUID that is determinstic across builds, so that regenerating .vcxprojs will just
    /// update existing solutions and projects
    /// </summary>
    sealed class DeterministicGuid
    {
        /// <summary>
        /// Construct the GUID
        /// </summary>
        /// <param name="input">from this data</param>
        public
        DeterministicGuid(
            string input)
        {
            // ref: http://geekswithblogs.net/EltonStoneman/archive/2008/06/26/generating-deterministic-guids.aspx

            // use MD5 hash to get a 16-byte hash of the string
            var provider = new System.Security.Cryptography.MD5CryptoServiceProvider();
            var inputBytes = System.Text.Encoding.Default.GetBytes(input);
            var hashBytes = provider.ComputeHash(inputBytes);

            // generate a guid from the hash
            var hashGuid = new System.Guid(hashBytes);

            this.Guid = hashGuid;
        }

        /// <summary>
        /// Get the GUID
        /// </summary>
        public System.Guid Guid { get; private set; }
    }
}
