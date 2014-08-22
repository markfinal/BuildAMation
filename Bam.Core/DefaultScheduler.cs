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
    public class DefaultScheduler :
        IBuildScheduler
    {
        private DependencyGraph graph;
        private int scheduledNodeCount = 0;
        private int percentComplete = 0;
        private int rankHighWaterMark;

        public event BuildSchedulerProgressUpdatedDelegate ProgressUpdated;

        public
        DefaultScheduler(
            DependencyGraph graph)
        {
            this.graph = graph;
            this.TotalNodeCount = graph.TotalNodeCount;
            this.ScheduledNodeCount = 0;
            this.rankHighWaterMark = this.graph.RankCount - 1;

            for (int rank = this.rankHighWaterMark; rank >= 0; --rank)
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
            for (int rank = this.rankHighWaterMark; rank >= 0; --rank)
            {
                var rankCollection = this.graph[rank];

                if (RankedNodeCollectionComplete(rankCollection))
                {
                    if (rankCollection.Rank == this.rankHighWaterMark)
                    {
                        --this.rankHighWaterMark;
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
