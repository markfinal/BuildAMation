// <copyright file="DependencyNodeEnumerator.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public class DependencyNodeEnumerator : System.Collections.IEnumerator
    {
        private DependencyGraph graph;
        private int currentRank;
        private DependencyNodeCollection currentRankCollection;
        private int currentNodeIndex;
        
        public DependencyNodeEnumerator(DependencyGraph graph)
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
        
        public bool MoveNext()
        {
            this.currentNodeIndex++;
            if (this.currentNodeIndex >= this.currentRankCollection.Count)
            {
#if true
                for (;;)
                {
                    this.currentRank++;
                    if (this.currentRank >= this.graph.RankCount)
                    {
                        return false;
                    }
                    
                    this.currentRankCollection = this.graph[this.currentRank];
                    if (0 == this.currentRankCollection.Count)
                    {
                        Log.DebugMessage("Rank {0} collection is empty", this.currentRank);
                    }
                    else
                    {
                        break;
                    }
                }
#else
                this.currentRank++;
                if (this.currentRank >= this.graph.RankCount)
                {
                    return false;
                }
                this.currentRankCollection = this.graph[this.currentRank];
#endif
                this.currentNodeIndex = 0;
            }
            
            return true;
        }
        
        public void Reset()
        {
            this.currentRank = 0;
            this.currentRankCollection = this.graph[this.currentRank];
            this.currentNodeIndex = -1;
        }
    }
}