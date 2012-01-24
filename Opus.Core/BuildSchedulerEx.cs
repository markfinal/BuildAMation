// <copyright file="BuildSchedulerEx.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public class BuildSchedulerEx : IBuildScheduler
    {
        private int rankCount;
        private int scheduledNodeCount = 0;
        private int percentComplete = 0;
        private System.Collections.Generic.Queue<DependencyNodeCollection> rankCollections;
        private DependencyNodeCollection rankInProgress;
        private DependencyGraph graph; // TODO: to remove

        public delegate void ProgressUpdatedDelegate(int percentageComplete);
        public static event ProgressUpdatedDelegate ProgressUpdated;
        
        public BuildSchedulerEx(DependencyGraph graph)
        {
            this.graph = graph;
            this.rankCount = graph.RankCount;
            this.rankCollections = new System.Collections.Generic.Queue<DependencyNodeCollection>(rankCount);

            // TODO: would like to do a reverse foreach
            for (int rank = this.rankCount - 1; rank >= 0; --rank)
            {
                this.rankCollections.Enqueue(graph[rank].Clone() as DependencyNodeCollection);
            }

            this.rankInProgress = this.rankCollections.Dequeue();

            this.TotalNodeCount = graph.TotalNodeCount;
            this.ScheduledNodeCount = 0;
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
                int percentComplete = 100 * this.scheduledNodeCount / this.TotalNodeCount;
                if (percentComplete != this.percentComplete)
                {
                    this.percentComplete = percentComplete;
                    ProgressUpdated(percentComplete);
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
        
        public DependencyNode GetNextNodeToBuild()
        {
            if (null == this.rankInProgress)
            {
                return null;
            }

            if (this.rankInProgress.Complete)
            {
                for (; ; )
                {
                    if (0 == this.rankCollections.Count)
                    {
                        this.rankInProgress = null;
                        return null;
                    }

                    this.rankInProgress = this.rankCollections.Dequeue();
                    if (!this.rankInProgress.Complete)
                    {
                        break;
                    }
                }
            }

            // look for an available node in the current rank
            foreach (DependencyNode node in this.rankInProgress)
            {
                if (EBuildState.NotStarted == node.BuildState)
                {
                    return node;
                }
            }

            DependencyNodeCollection nextRank = this.rankCollections.Peek();
            foreach (DependencyNode node in nextRank)
            {
                DependencyNodeCollection dependents = new DependencyNodeCollection();
                if (null != node.Children)
                {
                    dependents.AddRange(node.Children);
                }
                if (null != node.ExternalDependents)
                {
                    dependents.AddRange(node.ExternalDependents);
                }
                if (null != node.RequiredDependents)
                {
                    dependents.AddRange(node.RequiredDependents);
                }
                bool dependentsComplete = true;
                if (0 != dependents.Count)
                {
                    dependentsComplete = dependents.Complete;
                }
                if (dependentsComplete && (EBuildState.NotStarted == node.BuildState))
                {
                    ++this.ScheduledNodeCount;
                    // is the build function empty? if so, just mark as succeeded
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

            // TODO: I suppose this could happen if the nextRankCollection were tiny
            // and didn't need to do much work
            throw new Exception("Unable to locate a node to build");
        }
    }
}
