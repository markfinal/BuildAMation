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
namespace V2
{
    using System.Linq;

    /// <summary>
    /// Management of the graph execution
    /// </summary>
    public sealed class Executor
    {
        // Provides a task scheduler that ensures a maximum concurrency level while
        // running on top of the thread pool.
        // https://msdn.microsoft.com/en-us/library/ee789351.aspx
        public class LimitedConcurrencyLevelTaskScheduler : System.Threading.Tasks.TaskScheduler
        {
            // Indicates whether the current thread is processing work items.
            [System.ThreadStatic]
            private static bool _currentThreadIsProcessingItems;

            // The list of tasks to be executed
            private readonly System.Collections.Generic.LinkedList<System.Threading.Tasks.Task> _tasks = new System.Collections.Generic.LinkedList<System.Threading.Tasks.Task>(); // protected by lock(_tasks)

            // The maximum concurrency level allowed by this scheduler.
            private readonly int _maxDegreeOfParallelism;

            // Indicates whether the scheduler is currently processing work items.
            private int _delegatesQueuedOrRunning = 0;

            // Creates a new instance with the specified degree of parallelism.
            public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
            {
                if (maxDegreeOfParallelism < 1) throw new System.ArgumentOutOfRangeException("maxDegreeOfParallelism");
                _maxDegreeOfParallelism = maxDegreeOfParallelism;
            }

            // Queues a task to the scheduler.
            protected sealed override void QueueTask(System.Threading.Tasks.Task task)
            {
                // Add the task to the list of tasks to be processed.  If there aren't enough
                // delegates currently queued or running to process tasks, schedule another.
                lock (_tasks)
                {
                    _tasks.AddLast(task);
                    if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                    {
                        ++_delegatesQueuedOrRunning;
                        NotifyThreadPoolOfPendingWork();
                    }
                }
            }

            // Inform the ThreadPool that there's work to be executed for this scheduler.
            private void NotifyThreadPoolOfPendingWork()
            {
                System.Threading.ThreadPool.UnsafeQueueUserWorkItem(_ =>
                {
                    // Note that the current thread is now processing work items.
                    // This is necessary to enable inlining of tasks into this thread.
                    _currentThreadIsProcessingItems = true;
                    try
                    {
                        // Process all available items in the queue.
                        while (true)
                        {
                            System.Threading.Tasks.Task item;
                            lock (_tasks)
                            {
                                // When there are no more items to be processed,
                                // note that we're done processing, and get out.
                                if (_tasks.Count == 0)
                                {
                                    --_delegatesQueuedOrRunning;
                                    break;
                                }

                                // Get the next item from the queue
                                item = _tasks.First.Value;
                                _tasks.RemoveFirst();
                            }

                            // Execute the task we pulled out of the queue
                            base.TryExecuteTask(item);
                        }
                    }
                    // We're done processing items on the current thread
                    finally { _currentThreadIsProcessingItems = false; }
                }, null);
            }

            // Attempts to execute the specified task on the current thread.
            protected sealed override bool TryExecuteTaskInline(System.Threading.Tasks.Task task, bool taskWasPreviouslyQueued)
            {
                // If this thread isn't already processing a task, we don't support inlining
                if (!_currentThreadIsProcessingItems) return false;

                // If the task was previously queued, remove it from the queue
                if (taskWasPreviouslyQueued)
                    // Try to run the task.
                if (TryDequeue(task))
                    return base.TryExecuteTask(task);
                else
                    return false;
                else
                    return base.TryExecuteTask(task);
            }

            // Attempt to remove a previously scheduled task from the scheduler.
            protected sealed override bool TryDequeue(System.Threading.Tasks.Task task)
            {
                lock (_tasks) return _tasks.Remove(task);
            }

            // Gets the maximum concurrency level supported by this scheduler.
            public sealed override int MaximumConcurrencyLevel { get { return _maxDegreeOfParallelism; } }

            // Gets an enumerable of the tasks currently scheduled on this scheduler.
            protected sealed override System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task> GetScheduledTasks()
            {
                bool lockTaken = false;
                try
                {
                    System.Threading.Monitor.TryEnter(_tasks, ref lockTaken);
                    if (lockTaken) return _tasks;
                    else throw new System.NotSupportedException();
                }
                finally
                {
                    if (lockTaken) System.Threading.Monitor.Exit(_tasks);
                }
            }
        }

        public void Run()
        {
            // TODO: should the rank collections be sorted, so that modules with fewest dependencies are first?

            var allUpToDate = true;
            var graph = Graph.Instance;
            foreach (var rank in graph.Reverse())
            {
                foreach (Module module in rank)
                {
                    module.Evaluate();
                    allUpToDate &= module.IsUpToDate;
                }
            }
            if (allUpToDate)
            {
                Log.DebugMessage("Everything up to date");
                return;
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

            var threadCount = CommandLineProcessor.Evaluate(new MultiThreaded());
            if (0 == threadCount)
            {
                threadCount = System.Environment.ProcessorCount;
            }

            System.Exception abortException = null;
            if (threadCount > 1)
            {
                var cancellationSource = new System.Threading.CancellationTokenSource();
                var cancellationToken = cancellationSource.Token;

                // LongRunning is absolutely necessary in order to achieve paralleism
                var creationOpts = System.Threading.Tasks.TaskCreationOptions.LongRunning;
                var continuationOpts = System.Threading.Tasks.TaskContinuationOptions.LongRunning;

                var scheduler = new LimitedConcurrencyLevelTaskScheduler(threadCount);

                var factory = new System.Threading.Tasks.TaskFactory(
                        cancellationToken,
                        creationOpts,
                        continuationOpts,
                        scheduler);

                var tasks = new Array<System.Threading.Tasks.Task>();
                foreach (var rank in graph.Reverse())
                {
                    foreach (var module in rank)
                    {
                        if (module.IsUpToDate)
                        {
                            Log.DebugMessage("Module {0} is up-to-date", module.ToString());
                            continue;
                        }

                        Log.DebugMessage("Module {0} requires building", module.ToString());
                        var context = new ExecutionContext();

                        var task = factory.StartNew(() =>
                            {
                                var depTasks = new Array<System.Threading.Tasks.Task>();
                                foreach (var dep in module.Dependents)
                                {
                                    if (null == dep.ExecutionTask)
                                    {
                                        continue;
                                    }
                                    depTasks.Add(dep.ExecutionTask);
                                }
                                foreach (var dep in module.Requirements)
                                {
                                    if (null == dep.ExecutionTask)
                                    {
                                        continue;
                                    }
                                    depTasks.Add(dep.ExecutionTask);
                                }
                                System.Threading.Tasks.Task.WaitAll(depTasks.ToArray());

                                (module as IModuleExecution).Execute(context);

                                Log.Full(context.OutputStringBuilder.ToString());
                                if (context.ErrorStringBuilder.Length > 0)
                                {
                                    Log.ErrorMessage(context.ErrorStringBuilder.ToString());
                                }
                            });
                        tasks.Add(task);
                        module.ExecutionTask = task;
                    }
                }
                System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
            }
            else
            {
                foreach (var rank in graph.Reverse())
                {
                    if (null != abortException)
                    {
                        break;
                    }
                    foreach (IModuleExecution module in rank)
                    {
                        if (module.IsUpToDate)
                        {
                            Log.DebugMessage("Module {0} is up-to-date", module.ToString());
                            continue;
                        }
                        Log.DebugMessage("Module {0} requires building", module.ToString());

                        var context = new ExecutionContext();
                        try
                        {
                            module.Execute(context);
                        }
                        catch (Bam.Core.Exception ex)
                        {
                            abortException = ex;
                            break;
                        }
                        finally
                        {
                            Log.Full(context.OutputStringBuilder.ToString());
                            if (context.ErrorStringBuilder.Length > 0)
                            {
                                Log.ErrorMessage(context.ErrorStringBuilder.ToString());
                            }
                        }
                    }
                }
            }

            if (null != abortException)
            {
                throw abortException;
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
