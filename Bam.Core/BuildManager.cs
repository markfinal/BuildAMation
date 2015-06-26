#region License
// Copyright 2010-2015 Mark Final
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
#endregion // License
namespace Bam.Core
{
namespace V2
{
    using System.Linq;

    /// <summary>
    /// Management of the graph execution
    /// </summary>
    public sealed class Executor
    {
        public Executor()
        {
        }

        public void Run()
        {
            var graph = Graph.Instance;
            foreach (var rank in graph.Reverse())
            {
                foreach (Module module in rank)
                {
                    module.Evaluate();
                }
            }

            var metaName = System.String.Format("{0}Builder.V2.{0}Meta", Core.State.BuilderName);
            var metaData = Core.State.ScriptAssembly.GetType(metaName);
            if (null != metaData)
            {
                var method = metaData.GetMethod("PreExecution");
                if (null != method)
                {
                    method.Invoke(null, null);
                }
            }

            if (!System.IO.Directory.Exists(Core.State.BuildRoot))
            {
                System.IO.Directory.CreateDirectory(Core.State.BuildRoot);
            }

            var contextQueue = new System.Collections.Generic.Queue<ExecutionContext>();
            foreach (var rank in graph.Reverse())
            {
                foreach (IModuleExecution module in rank)
                {
                    if (module.IsUpToDate)
                    {
                        Log.DebugMessage("Module {0} is up-to-date", module.ToString());
                        continue;
                    }

                    var context = new ExecutionContext();
                    contextQueue.Enqueue(context);

                    Log.DebugMessage("Module {0} requires building", module.ToString());
                    module.Execute(context);

                    // deal with the latest context
                    var dealWithContext = contextQueue.Dequeue();
                    Log.Full(dealWithContext.OutputStringBuilder.ToString());
                    if (dealWithContext.ErrorStringBuilder.Length > 0)
                    {
                        Log.ErrorMessage(dealWithContext.ErrorStringBuilder.ToString());
                    }
                }
            }

            if (null != metaData)
            {
                var method = metaData.GetMethod("PostExecution");
                if (null != method)
                {
                    method.Invoke(null, null);
                }
            }
        }
    }
}

    public class BuildManager
    {
        private DependencyGraph graph;
        private IBuildScheduler scheduler;
        private bool active;
        private bool cancelled = false;

        private System.Collections.Generic.List<BuildAgent> agents;
        private System.Collections.Generic.List<System.Threading.ManualResetEvent> agentsAvailable;
        private System.Threading.ManualResetEvent AgentReportsFailure = new System.Threading.ManualResetEvent(false);

        private class OutputQueueData
        {
            public DependencyNode node;
            public System.Text.StringBuilder output;
            public System.Text.StringBuilder error;
        }

        private System.Threading.ManualResetEvent allOutputComplete = new System.Threading.ManualResetEvent(false);
        private System.Threading.ManualResetEvent ioAvailable = new System.Threading.ManualResetEvent(false);
        private System.Collections.Generic.Queue<OutputQueueData> outputQueue = new System.Collections.Generic.Queue<OutputQueueData>();
        private System.Collections.Generic.Dictionary<DependencyNode, System.Threading.ManualResetEvent> nodesProcessing = new System.Collections.Generic.Dictionary<DependencyNode, System.Threading.ManualResetEvent>();

        public System.Collections.Generic.List<System.Threading.ManualResetEvent> AdditionalThreadCompletionEvents
        {
            get;
            private set;
        }

        public
        BuildManager(
            DependencyGraph graph)
        {
            this.graph = graph;

            var schedulerType = System.Type.GetType(State.SchedulerType);
            if (null == schedulerType)
            {
                schedulerType = State.ScriptAssembly.GetType(State.SchedulerType);
                if (null == schedulerType)
                {
                    throw new Exception("Scheduler type '{0}' not found in the Bam.Core assembly, nor the script assembly", State.SchedulerType);
                }
            }
            TypeUtilities.CheckTypeImplementsInterface(schedulerType, typeof(Core.IBuildScheduler));

            this.scheduler = System.Activator.CreateInstance(schedulerType, new object[] { graph }) as IBuildScheduler;
            // attach updates
            foreach (var function in State.SchedulerProgressUpdates)
            {
                this.scheduler.ProgressUpdated += function;
            }

            this.Builder = Core.State.BuilderInstance;

            int jobCount = Core.State.JobCount;
            if (0 == jobCount)
            {
                throw new Exception("Zero job count is invalid");
            }

            this.agents = new System.Collections.Generic.List<BuildAgent>(jobCount);
            this.agentsAvailable = new System.Collections.Generic.List<System.Threading.ManualResetEvent>(jobCount);
            for (int job = 0; job < jobCount; ++job)
            {
                var agent = new BuildAgent(System.String.Format("Agent {0}", job), this.Builder, this.AgentReportsFailure);
                this.agents.Insert(job, agent);
                this.agentsAvailable.Insert(job, agent.IsAvailable);
            }

            this.Finished = new System.Threading.ManualResetEvent(false);
            this.AdditionalThreadCompletionEvents = new System.Collections.Generic.List<System.Threading.ManualResetEvent>();
        }

        public void
        Cancel()
        {
            this.cancelled = true;
        }

        private IBuilder Builder
        {
            get;
            set;
        }

        private BuildAgent
        AvailableAgent()
        {
            // wait indefinitely for an available agent
            int availableAgentIndex = System.Threading.WaitHandle.WaitAny(this.agentsAvailable.ToArray(), -1);
            var availableAgent = this.agents[availableAgentIndex];
            return availableAgent;
        }

        private bool HasFailed
        {
            get
            {
                if (this.cancelled)
                {
                    return true;
                }

                if (System.Threading.WaitHandle.WaitAll(new System.Threading.WaitHandle[] { this.AgentReportsFailure }, 0))
                {
                    Log.DebugMessage("Agent reports failure");
                    return true;
                }

                return false;
            }
        }

        public bool
        Execute()
        {
            Log.Info("Build started");

            State.BuildStartedEvent.Set();
            this.active = true;
            System.Threading.ThreadPool.QueueUserWorkItem(this.OutputErrorProcessingThread, this);

            try
            {
                var preExecute = (this.Builder as IBuilderPreExecute);
                if (null != preExecute)
                {
                    preExecute.PreExecute();
                }
            }
            catch (System.Reflection.TargetInvocationException exception)
            {
                Exception.DisplayException(exception, "PreExecute function error");
                return false;
            }

            while (this.scheduler.AreNodesAvailable)
            {
                var agent = this.AvailableAgent();
                var nodeWork = this.scheduler.GetNextNodeToBuild();

                // check for failure to build a previous node
                if (this.HasFailed)
                {
                    break;
                }

                if (null != nodeWork)
                {
                    lock (this.nodesProcessing)
                    {
                        this.nodesProcessing.Add(nodeWork, new System.Threading.ManualResetEvent(false));
                    }

                    nodeWork.CompletedEvent += new DependencyNode.CompleteEventHandler(CompletedNode);
                    agent.Execute(nodeWork);
                }
                else
                {
                    Log.DebugMessage("**** No available Node found ready to build; waiting for running nodes to finish ****");
                    System.Threading.ManualResetEvent[] toWaitOn = null;
                    lock (this.nodesProcessing)
                    {
                        toWaitOn = new System.Threading.ManualResetEvent[this.nodesProcessing.Count];
                        this.nodesProcessing.Values.CopyTo(toWaitOn, 0);
                    }
                    if (toWaitOn.Length > 0)
                    {
                        System.Threading.WaitHandle.WaitAny(toWaitOn, -1);
                    }
                }
            }

            // wait for all running agents to finish
            // some may cancel, but we catch this shortly
            System.Threading.WaitHandle.WaitAll(this.agentsAvailable.ToArray(), -1);

            // check for failure
            var agentsFailed = this.HasFailed;

            bool returnValue;
            if (!agentsFailed)
            {
                try
                {
                    var postExecute = this.Builder as IBuilderPostExecute;
                    if (null != postExecute)
                    {
                        postExecute.PostExecute(this.graph.ExecutedNodes);
                    }
                }
                catch (System.Reflection.TargetInvocationException exception)
                {
                    Exception.DisplayException(exception, "PostExecute function error");
                    return false;
                }

                returnValue = true;
            }
            else
            {
                returnValue = false;
            }

            this.active = false;
            this.Finished.Set();

            // wait on any additional threads
            if (this.AdditionalThreadCompletionEvents.Count > 0)
            {
                System.Threading.WaitHandle.WaitAll(this.AdditionalThreadCompletionEvents.ToArray(), -1);
                this.AdditionalThreadCompletionEvents = null;
            }

            // first signal the event that there's output (even though there isn't)
            this.ioAvailable.Set();

            // wait on the thread that is outputting text from the nodes to finish and notice
            // that the build manager is shutting down (on this.active)
            System.Threading.WaitHandle.WaitAll(new System.Threading.WaitHandle[] { this.allOutputComplete }, -1);

            Log.Info("Build finished; graph contained {0} entries", this.graph.TotalNodeCount);

            return returnValue;
        }

        public System.Threading.ManualResetEvent Finished
        {
            get;
            set;
        }

        private void
        CompletedNode(
            DependencyNode node)
        {
            this.nodesProcessing[node].Set();

            lock (this.nodesProcessing)
            {
                this.nodesProcessing.Remove(node);
            }

            node.CompletedEvent -= this.CompletedNode;

            if ((0 == node.OutputStringBuilder.Length) && (0 == node.ErrorStringBuilder.Length))
            {
                return;
            }

            var OutputQueueData = new OutputQueueData();
            OutputQueueData.node = node;
            OutputQueueData.output = node.OutputStringBuilder;
            OutputQueueData.error = node.ErrorStringBuilder;

            lock (this.outputQueue)
            {
                this.outputQueue.Enqueue(OutputQueueData);
                this.ioAvailable.Set();
            }
        }

        private void
        OutputErrorProcessingThread(
            object state)
        {
            var buildManager = state as BuildManager;
            var outputQueue = buildManager.outputQueue;

            while (buildManager.active)
            {
                // this event is signalled at the end of the build as well as whenever a node
                // has some textual output from it's build
                System.Threading.WaitHandle.WaitAll(new System.Threading.WaitHandle[] { this.ioAvailable }, -1);

                while (outputQueue.Count > 0)
                {
                    OutputQueueData outputOutputQueueData = null;
                    lock (outputQueue)
                    {
                        outputOutputQueueData = outputQueue.Dequeue();
                    }
                    if (null == outputOutputQueueData)
                    {
                        throw new Exception("Output Queue contained a null reference; there was {0} items before the last dequeue", outputQueue.Count + 1);
                    }

                    var preamble = false;
                    if (outputOutputQueueData.output.Length > 0)
                    {
                        if (!preamble)
                        {
                            Log.DebugMessage("**** Output from Node {0}", outputOutputQueueData.node.UniqueModuleName);
                            preamble = true;
                        }

                        Log.Info("Messages:");
                        var lines = outputOutputQueueData.output.ToString().Split(new char[] { '\n' });
                        int count = 0;
                        foreach (var line in lines)
                        {
                            if (line.Length > 0)
                            {
                                Log.Info("{0}: {1}", count++, line);
                            }
                        }
                    }
                    if (outputOutputQueueData.error.Length > 0)
                    {
                        if (!preamble)
                        {
                            Log.DebugMessage("**** Output from Node {0}", outputOutputQueueData.node.UniqueModuleName);
                            preamble = true;
                        }

                        var lines = outputOutputQueueData.error.ToString().Split(new char[] { '\n' });
                        foreach (var line in lines)
                        {
                            if (line.Length > 0)
                            {
                                Log.ErrorMessage(line);
                            }
                        }
                    }
                }

                // no more work to be done at this time
                lock (ioAvailable)
                {
                    ioAvailable.Reset();
                }
            }

            // signal complete
            allOutputComplete.Set();
        }
    }
}
