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

        public event BuildSchedulerProgressUpdatedDelegate ProgressUpdated;
        
        public BuildSchedulerEx(DependencyGraph graph)
        {
            this.graph = graph;
            this.rankCount = graph.RankCount;
            this.rankCollections = new System.Collections.Generic.Queue<DependencyNodeCollection>(rankCount);

            // TODO: would like to do a reverse foreach
            for (int rank = this.rankCount - 1; rank >= 0; --rank)
            {
                if (0 == graph[rank].Count)
                {
                    throw new Exception(System.String.Format("Dependency node collection for rank {0} is empty", rank), false);
                }

                this.rankCollections.Enqueue(graph[rank].Clone() as DependencyNodeCollection);
            }
            //Log.MessageAll("** There are {0} ranks", this.rankCount);

            this.rankInProgress = this.rankCollections.Dequeue();
            //Log.MessageAll("** Current collection = {0}", this.rankInProgress.ToString());
            if (0 == this.rankInProgress.Count)
            {
                throw new Exception(System.String.Format("Dependency node collection for rank {0} is empty", this.rankInProgress.Rank), false);
            }

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
                int percentComplete = (this.TotalNodeCount > 0) ? 100 * this.scheduledNodeCount / this.TotalNodeCount : 100;
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
        
        public DependencyNode GetNextNodeToBuild()
        {
            if (null == this.rankInProgress)
            {
                //Log.MessageAll("** Current rank is null");
                return null;
            }

            if (0 == this.rankInProgress.Count)
            {
                //Log.MessageAll("** Current rank is complete");
                if (0 == this.rankCollections.Count)
                {
                    //Log.MessageAll("** No ranks remaining");
                    this.rankInProgress = null;
                    return null;
                }

                this.rankInProgress = this.rankCollections.Dequeue();
            }

            // get the next node
            while (this.rankInProgress.Count > 0)
            {
                for (int i = 0; i < this.rankInProgress.Count; ++i)
                {
                    DependencyNode node = this.rankInProgress[i];
                    if (!node.IsReadyToBuild())
                    {
                        continue;
                    }

                    this.rankInProgress.Remove(node);

                    //Log.MessageAll("** Found unstarted node {0}", node.ToString());
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

            //Log.MessageAll("** Nodes in current rank are all started");

            DependencyNodeCollection nextRank = this.rankCollections.Peek();
            while (nextRank.Count > 0)
            {
                DependencyNode node = nextRank[0];
                if (node.IsReadyToBuild())
                {
                    nextRank.Remove(node);

                    //Log.MessageAll("** Found unstarted node {0} in next rank", node.ToString());
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

            // no Node in the current rank was found to work on
            // and no Node in the next rank was found to work on that didn't have all of it's
            // dependencies satisifed
            // so we just have to give up and wait for in-flight Nodes to finish
            //Log.MessageAll("*** Pass-thru - waiting for in-flight nodes to finish ***");
            return null;
        }
    }
}
