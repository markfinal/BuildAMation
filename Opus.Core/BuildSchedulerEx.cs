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
                this.rankCollections.Enqueue(graph[rank].Clone() as DependencyNodeCollection);
            }
            //Log.MessageAll("** There are {0} ranks", this.rankCount);

            this.rankInProgress = this.rankCollections.Dequeue();
            //Log.MessageAll("** Current collection = {0}", this.rankInProgress.ToString());

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
                //Log.MessageAll("** Current rank is null");
                return null;
            }

            if (0 == this.rankInProgress.Count)
            {
                //Log.MessageAll("** Current rank is complete");
                for (; ; )
                {
                    if (0 == this.rankCollections.Count)
                    {
                        //Log.MessageAll("** No ranks remaining");
                        this.rankInProgress = null;
                        return null;
                    }

                    this.rankInProgress = this.rankCollections.Dequeue();
                    //Log.MessageAll("** New rank collection is {0}", this.rankInProgress);
                    if (this.rankInProgress.Count > 0)
                    {
                        //Log.MessageAll("\tand is not complete yet");
                        break;
                    }
                }
            }

            // get the next node
            while (this.rankInProgress.Count > 0)
            {
                DependencyNode node = this.rankInProgress[0];
                if (node.BuildState != EBuildState.NotStarted)
                {
                    throw new Exception("Next node to schedule is not in the correct state. This should never happen", false);
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

            //Log.MessageAll("** Nodes in current rank are all started");

            DependencyNodeCollection nextRank = this.rankCollections.Peek();
            while (nextRank.Count  >0)
            {
                DependencyNode node = nextRank[0];

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

            // no Node in the curent rank was found to work on
            // and no Node in the next rank was found to work on that didn't have all of it's
            // dependencies satisifed
            // so we just have to give up and wait for in-flight Nodes to finish
            //Log.MessageAll("*** Pass-thru - waiting for in-flight nodes to finish ***");
            return null;
        }
    }
}
