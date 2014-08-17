#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion
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
