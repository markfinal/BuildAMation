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
            get;
            set;
        }

        public int PercentageScheduled
        {
            get
            {
                int percentage = this.ScheduledNodeCount * 100 / this.TotalNodeCount;
                return percentage;
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