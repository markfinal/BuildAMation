// <copyright file="DefaultScheduler.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public class DefaultScheduler :
        IBuildScheduler
    {
        private DependencyGraph graph;
        private int scheduledNodeCount = 0;
        private int percentComplete = 0;
        private int highestRankToBuild;

        public event BuildSchedulerProgressUpdatedDelegate ProgressUpdated;

        public
        DefaultScheduler(
            DependencyGraph graph)
        {
            this.graph = graph;
            this.TotalNodeCount = graph.TotalNodeCount;
            this.ScheduledNodeCount = 0;
            this.highestRankToBuild = this.graph.RankCount - 1;

            for (int rank = this.highestRankToBuild; rank >= 0; --rank)
            {
                var rankCollection = this.graph[rank];
                if (0 == rankCollection.Count)
                {
                    throw new Exception("Dependency node collection for rank {0} is empty", rank);
                }

                foreach (var node in rankCollection)
                {
                    if (!node.AreDependenciesProcessed)
                    {
                        throw new Exception("Node '{0}' has not been processed in rank {1}", node.UniqueModuleName, rank);
                    }
                }
            }
        }

        public int TotalNodeCount
        {
            get;
            private set;
        }

        private int ScheduledNodeCount
        {
            get
            {
                return this.scheduledNodeCount;
            }

            set
            {
                this.scheduledNodeCount = value;
                int percentComplete = (this.TotalNodeCount > 0) ? (100 * this.scheduledNodeCount / this.TotalNodeCount) : 100;
                if (percentComplete != this.percentComplete)
                {
                    this.percentComplete = percentComplete;
                    if (null != this.ProgressUpdated)
                    {
                        this.ProgressUpdated(percentComplete);
                    }
                }
            }
        }

        public bool AreNodesAvailable
        {
            get
            {
                return (this.ScheduledNodeCount != this.TotalNodeCount);
            }
        }

        static private bool
        RankedNodeCollectionComplete(
            DependencyNodeCollection rankCollection)
        {
            var rankCollectionComplete = System.Threading.WaitHandle.WaitAll(rankCollection.AllNodesCompletedEvent, 0);
            return rankCollectionComplete;
        }

        public DependencyNode
        GetNextNodeToBuild()
        {
            for (int rank = this.highestRankToBuild; rank >= 0; --rank)
            {
                var rankCollection = this.graph[rank];

                if (RankedNodeCollectionComplete(rankCollection))
                {
                    if (rankCollection.Rank == this.highestRankToBuild)
                    {
                        --this.highestRankToBuild;
                    }

                    continue;
                }

                foreach (var node in rankCollection)
                {
                    if (node.IsReadyToBuild())
                    {
                        ++this.ScheduledNodeCount;
                        // is the build function empty?
                        // if so, just mark as succeeded
                        if (null == node.BuildFunction)
                        {
                            node.BuildState = EBuildState.Succeeded;
                        }
                        else
                        {
                            this.graph.ExecutedNodes.Add(node);
                            return node;
                        }
                    }
                }
            }

            return null;
        }
    }
}
