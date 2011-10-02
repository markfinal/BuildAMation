// <copyright file="BuildScheduler.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public class BuildScheduler
    {
        private DependencyGraph graph;
        private int scheduledNodeCount = 0;
        private int percentComplete = 0;

        public delegate void ProgressUpdatedDelegate(int percentageComplete);
        public static event ProgressUpdatedDelegate ProgressUpdated;
        
        public BuildScheduler(DependencyGraph graph)
        {
            this.graph = graph;
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
            int rankCount = this.graph.RankCount;
            for (int rank = rankCount - 1; rank >= 0; --rank)
            {
                DependencyNodeCollection rankCollection = this.graph[rank];
                if (!rankCollection.Complete)
                {
                    foreach (DependencyNode node in rankCollection)
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
                }
            }
            return null;
        }
    }
}