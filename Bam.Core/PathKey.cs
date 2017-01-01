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
    /// String based representation of a unique key to use to identify particular paths within modules.
    /// </summary>
    public sealed class PathKey
    {
        private static System.Collections.Generic.List<PathKey> GeneratedKeys = new System.Collections.Generic.List<PathKey>();

        private PathKey(
            string key)
        {
            this.Id = key;
        }

        /// <summary>
        /// Generate, or return an existing, unique key given the name.
        /// </summary>
        /// <param name="key">Key.</param>
        public static PathKey
        Generate(
            string key)
        {
            var foundPathKey = GeneratedKeys.FirstOrDefault(item => (item.Id == key));
            if (null != foundPathKey)
            {
                return foundPathKey;
            }
            var newKey = new PathKey(key);
            GeneratedKeys.Add(newKey);
            return newKey;
        }

        /// <summary>
        /// Recall the Id for the key.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Required override for the Equals override.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int
        GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        /// <summary>
        /// Are two keys equal?
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Bam.Core.PathKey"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="Bam.Core.PathKey"/>; otherwise, <c>false</c>.</returns>
        public override bool
        Equals(
            object obj)
        {
            return this.Id.Equals((obj as PathKey).Id);
        }

        /// <summary>
        /// String representation of a key.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="Bam.Core.PathKey"/>.</returns>
        public override string
        ToString()
        {
            return this.Id;
        }
    }
}
