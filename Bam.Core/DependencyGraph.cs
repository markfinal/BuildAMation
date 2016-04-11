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
namespace Bam.Core
{
    /// <summary>
    /// A representation of the dependencies between modules in a graph-like-manner.
    /// Each module resides uniquely within a collection of modules. This is termed a rank.
    /// Each rank has an index. The leaf modules have the largest index. The last module to build
    /// has an index of index.
    /// A module exists in a higher rank if some other module depends on it.
    /// To build, the modules with the highest rank are built first, then moves to the previous rank,
    /// and so on, until rank zero.
    /// </summary>
    public sealed class DependencyGraph :
        System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int, ModuleCollection>>
    {
        private System.Collections.Generic.Dictionary<int, ModuleCollection> graph = new System.Collections.Generic.Dictionary<int, ModuleCollection>();

        /// <summary>
        /// Access the rank of modules at the specified index, or returns an empty collection.
        /// </summary>
        /// <param name="rankIndex">Index of the rank to obtain.</param>
        /// <returns>The module collection at the specified index, or an empty collection.</returns>
        public ModuleCollection this[int rankIndex]
        {
            get
            {
                if (!this.graph.ContainsKey(rankIndex))
                {
                    this.graph.Add(rankIndex, new ModuleCollection());
                }
                return this.graph[rankIndex];
            }
        }

        /// <summary>
        /// Determine the rank index of the provided collection of modules.
        /// </summary>
        /// <param name="collection">Module collection whose rank is to be determined.</param>
        /// <returns>The index of the specified module collection, or -1 if it does not exist.</returns>
        public int this[ModuleCollection collection]
        {
            get
            {
                var pairs = this.graph.Where(item => item.Value == collection);
                if (pairs.Count() > 0)
                {
                    // TODO: I think it's highly unlikely that count > 1, so does this have to be catered for?
                    return pairs.ElementAt(0).Key;
                }
                return -1;
            }
        }

        /// <summary>
        /// Return each rank index and module collection in turn.
        /// </summary>
        /// <returns>An IEnumerator for the index and module collection.</returns>
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int, ModuleCollection>>
        GetEnumerator()
        {
            int rankIndex = 0;
            while (rankIndex < this.graph.Count)
            {
                yield return new System.Collections.Generic.KeyValuePair<int, ModuleCollection>(rankIndex, this.graph[rankIndex]);
                ++rankIndex;
            }
        }

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
