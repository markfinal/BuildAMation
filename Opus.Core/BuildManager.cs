// <copyright file="BuildManager.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public class BuildManager
    {
        private DependencyGraph graph;
        private BuildScheduler scheduler;
        private bool active;
        private bool cancellationPending;
        private System.Collections.ArrayList agentsDone;
        private System.Collections.ArrayList agentWorkingList;
        private System.Collections.ArrayList agentFreeList;

        private class OutputQueueData
        {
            public DependencyNode node;
            public System.Text.StringBuilder output;
            public System.Text.StringBuilder error;
        }

        private System.Threading.ManualResetEvent allOutputComplete = new System.Threading.ManualResetEvent(false);
        private System.Collections.Generic.Queue<OutputQueueData> outputQueue = new System.Collections.Generic.Queue<OutputQueueData>();

        public BuildManager(DependencyGraph graph)
        {
            this.cancellationPending = false;
            this.graph = graph;
            this.scheduler = new BuildScheduler(graph);
            this.Builder = Core.State.BuilderInstance;

            int jobCount = Core.State.JobCount;
            if (0 == jobCount)
            {
                throw new Exception("Zero job count is invalid");
            }

            this.agentsDone = new System.Collections.ArrayList(jobCount);
            this.agentFreeList = new System.Collections.ArrayList(jobCount);
            this.agentWorkingList = new System.Collections.ArrayList(jobCount);
            for (int job = 0; job < jobCount; ++job)
            {
                BuildAgent agent = new BuildAgent(System.String.Format("Agent {0}", job), this.Builder);
                this.agentFreeList.Insert(job, agent);
            }
        }

        private IBuilder Builder
        {
            get;
            set;
        }

        private int WorkingAgentsCount
        {
            get
            {
                return this.agentWorkingList.Count;
            }
        }
        
        private BuildAgent AvailableAgent()
        {
            // if agents at working, check if any have finished
            if (this.agentWorkingList.Count > 0)
            {
                System.Threading.ManualResetEvent[] agentsDone = this.agentsDone.ToArray(typeof(System.Threading.ManualResetEvent)) as System.Threading.ManualResetEvent[];
                int finishedAgentIndex = System.Threading.WaitHandle.WaitAny(agentsDone, 0);
                if (System.Threading.WaitHandle.WaitTimeout != finishedAgentIndex)
                {
                    BuildAgent finishedAgent = this.agentWorkingList[finishedAgentIndex] as BuildAgent;
                    Log.DebugMessage("Agent '{0}' is now free", finishedAgent.Name);
                    if (!finishedAgent.Success)
                    {
                        Log.DebugMessage("Agent '{0}' reported failure", finishedAgent.Name);
                        this.cancellationPending = true;
                        return null;
                    }

                    this.agentWorkingList.RemoveAt(finishedAgentIndex);
                    this.agentsDone.RemoveAt(finishedAgentIndex);
                    this.agentFreeList.Add(finishedAgent);
                }
            }

            BuildAgent freeAgent = null;
            if (this.agentFreeList.Count > 0)
            {
                freeAgent = this.agentFreeList[0] as BuildAgent;
            }

            return freeAgent;
        }

        private void AddAgentToWorkingList(BuildAgent agent)
        {
            this.agentFreeList.Remove(agent);
            int index = this.agentWorkingList.Add(agent);
            this.agentsDone.Insert(index, agent.DoneEvent);
        }
        
        private void PreExecute()
        {
            System.Reflection.MethodInfo preExecuteMethod = this.Builder.GetType().GetMethod("PreExecute", new System.Type[] { });
            if (null != preExecuteMethod)
            {
                preExecuteMethod.Invoke(this.Builder, new object[] {});
            }
        }
        
        public bool Execute()
        {
            Log.Info("Build started");

            this.active = true;
            System.Threading.ThreadPool.QueueUserWorkItem(this.OutputErrorProcessingThread, this);

            try
            {
                this.PreExecute();
            }
            catch (System.Reflection.TargetInvocationException exception)
            {
                Core.Log.MessageAll("PreExecute function threw an exception: " + exception.Message);
                System.Exception innerException = exception;
                while (innerException.InnerException != null)
                {
                    innerException = innerException.InnerException;
                    Core.Log.MessageAll("Inner exception: {0}, {1}", innerException.GetType().ToString(), innerException.Message);
                }
                Core.Log.MessageAll(innerException.StackTrace);
                return false;
            }

            while (this.scheduler.AreNodesAvailable)
            {
                if (this.cancellationPending)
                {
                    Log.DebugMessage("Cancellation pending");
                    break;
                }

                BuildAgent agent = this.AvailableAgent();
                if (null != agent)
                {
                    DependencyNode nodeWork = this.scheduler.GetNextNodeToBuild();
                    if (null != nodeWork)
                    {
                        nodeWork.CompletedEvent += new DependencyNode.CompleteEventHandler(CompletedNode);
                        this.AddAgentToWorkingList(agent);
                        agent.Execute(nodeWork);
                    }
                    else
                    {
                        if ((0 == this.WorkingAgentsCount) &&
                            this.scheduler.AreNodesAvailable)
                        {
                            throw new Exception("No node to work on can be found, and no agents are working, so this is a bad state to be in");
                        }
                        else
                        {
                            Log.DebugMessage("Can't run anything yet");
                        }
                    }
                }
                else
                {
                    // yield time slice until agents have finished
                    System.Threading.Thread.Sleep(1);
                }
            }

            // wait for all agents to finish
            // TODO: should we be occasionally checking for failure? i.e. this.CancellationPending = true?
            if (this.agentsDone.Count > 0)
            {
                System.Threading.ManualResetEvent[] agentsDone = this.agentsDone.ToArray(typeof(System.Threading.ManualResetEvent)) as System.Threading.ManualResetEvent[];
                System.Threading.WaitHandle.WaitAll(agentsDone, -1);
            }

            foreach (BuildAgent agent in this.agentWorkingList)
            {
                if (!agent.Success)
                {
                    this.cancellationPending = true;
                    break;
                }
            }

            bool returnValue;
            if (this.cancellationPending)
            {
                returnValue = false;
            }
            else
            {
                try
                {
                    this.PostExecute();
                }
                catch (System.Reflection.TargetInvocationException exception)
                {
                    Core.Log.MessageAll("PostExecute function threw an exception: " + exception.Message);
                    System.Exception innerException = exception;
                    while (innerException.InnerException != null)
                    {
                        innerException = innerException.InnerException;
                        Core.Log.MessageAll("Inner exception: {0}, {1}", innerException.GetType().ToString(), innerException.Message);
                    }
                    Core.Log.MessageAll(innerException.StackTrace);
                    return false;
                }

                returnValue = true;
            }

            this.active = false;

            // wait on the thread that is outputting text from the nodes
            System.Threading.WaitHandle.WaitAll(new System.Threading.WaitHandle[] { this.allOutputComplete }, -1);

            Log.Info("Build finished; transformed {0} data entries", this.graph.TotalNodeCount);

            return returnValue;
        }

        private void CompletedNode(DependencyNode node)
        {
            node.CompletedEvent -= this.CompletedNode;

            if ((0 == node.OutputStringBuilder.Length) && (0 == node.ErrorStringBuilder.Length))
            {
                return;
            }

            OutputQueueData OutputQueueData = new OutputQueueData();
            OutputQueueData.node = node;
            OutputQueueData.output = node.OutputStringBuilder;
            OutputQueueData.error = node.ErrorStringBuilder;

            lock (this.outputQueue)
            {
                this.outputQueue.Enqueue(OutputQueueData);
            }
        }

        private void OutputErrorProcessingThread(object state)
        {
            BuildManager buildManager = state as BuildManager;
            System.Collections.Generic.Queue<OutputQueueData> outputQueue = buildManager.outputQueue;

            while (buildManager.active || outputQueue.Count > 0)
            {
                int queueSize = outputQueue.Count;
                if (outputQueue.Count > 0)
                {
                    OutputQueueData outputOutputQueueData = null;
                    lock (outputQueue)
                    {
                        outputOutputQueueData = outputQueue.Dequeue();
                    }
                    if (null == outputOutputQueueData)
                    {
                        throw new Exception(System.String.Format("Output Queue contained a null reference; there was {0} items before the last dequeue", queueSize), false);
                    }

                    bool preamble = false;
                    if (outputOutputQueueData.output.Length > 0)
                    {
                        string[] lines = outputOutputQueueData.output.ToString().Split(new char[] { '\n' });
                        foreach (string line in lines)
                        {
                            if (line.Length > 0)
                            {
                                if (!preamble)
                                {
                                    Log.DebugMessage("**** Output from Node {0}", outputOutputQueueData.node.UniqueModuleName);
                                    preamble = true;
                                }
                                Log.Info("\tMessage '{0}'", line);
                            }
                        }
                    }
                    if (outputOutputQueueData.error.Length > 0)
                    {
                        string[] lines = outputOutputQueueData.error.ToString().Split(new char[] { '\n' });
                        foreach (string line in lines)
                        {
                            if (line.Length > 0)
                            {
                                if (!preamble)
                                {
                                    Log.DebugMessage("**** Output from Node {0}", outputOutputQueueData.node.UniqueModuleName);
                                    preamble = true;
                                }
                                Log.Info("\tError '{0}'", line);
                            }
                        }
                    }
                }

                // yield
                System.Threading.Thread.Sleep(1);
            }

            // signal complete
            allOutputComplete.Set();
        }
        
        private void PostExecute()
        {
            System.Reflection.MethodInfo postExecuteMethod = this.Builder.GetType().GetMethod("PostExecute", new System.Type[] { typeof(DependencyNodeCollection) });

            if ((this.graph.ExecutedNodes.Count > 0) && (null != postExecuteMethod))
            {
                postExecuteMethod.Invoke(this.Builder, new object[] { this.graph.ExecutedNodes });
            }
        }
    }
}