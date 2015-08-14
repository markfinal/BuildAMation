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
