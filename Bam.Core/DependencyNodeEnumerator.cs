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
    public class DependencyNodeEnumerator :
        System.Collections.IEnumerator
    {
        private DependencyGraph graph;
        private int currentRank;
        private DependencyNodeCollection currentRankCollection;
        private int currentNodeIndex;

        public
        DependencyNodeEnumerator(
            DependencyGraph graph)
        {
            this.graph = graph;
            this.Reset();
        }

        public object Current
        {
            get
            {
                return this.currentRankCollection[this.currentNodeIndex];
            }
        }

        public bool
        MoveNext()
        {
            if (null == this.currentRankCollection)
            {
                return false;
            }

            this.currentNodeIndex++;
            if (this.currentNodeIndex >= this.currentRankCollection.Count)
            {
                for (;;)
                {
                    this.currentRank++;
                    if (this.currentRank >= this.graph.RankCount)
                    {
                        return false;
                    }

                    this.currentRankCollection = this.graph[this.currentRank];
                    if (0 != this.currentRankCollection.Count)
                    {
                        break;
                    }

                    Log.DebugMessage("Rank {0} collection is empty", this.currentRank);
                }
                this.currentNodeIndex = 0;
            }

            return true;
        }

        public void
        Reset()
        {
            this.currentRank = 0;
            if (this.graph.RankCount > this.currentRank)
            {
                this.currentRankCollection = this.graph[this.currentRank];
            }
            else
            {
                this.currentRankCollection = null;
            }
            this.currentNodeIndex = -1;
        }
    }
}
