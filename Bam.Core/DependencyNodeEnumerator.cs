// <copyright file="DependencyNodeEnumerator.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
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
