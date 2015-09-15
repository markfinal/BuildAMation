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
using System.Linq;
namespace Bam.Core
{
    /// <summary>
    /// Representation of a dependency graph of modules
    /// </summary>
    public sealed class DependencyGraph :
        System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int, ModuleCollection>>
    {
        private System.Collections.Generic.Dictionary<int, ModuleCollection> graph = new System.Collections.Generic.Dictionary<int, ModuleCollection>();

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

        public int this[ModuleCollection collection]
        {
            get
            {
                var pair = this.graph.Where(item => item.Value == collection);
                if (pair.Count() > 0)
                {
                    return pair.ElementAt(0).Key;
                }
                return -1;
            }
        }

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
