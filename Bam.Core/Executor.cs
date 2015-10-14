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
using System.Linq;
namespace Bam.Core
{
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

        private static void
        ExecutePreBuild(
            System.Type metaType)
        {
            var method = metaType.GetMethod("PreExecution");
            if (null == method)
            {
                return;
            }
            try
            {
                method.Invoke(null, null);
            }
            catch (System.Reflection.TargetInvocationException exception)
            {
                var inner = exception.InnerException;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }
                throw new Exception(inner, "Pre-build error:");
            }
        }

        private static void
        ExecutePostBuild(
            System.Type metaType)
        {
            var method = metaType.GetMethod("PostExecution");
            if (null == method)
            {
                return;
            }
            try
            {
                method.Invoke(null, null);
            }
            catch (System.Reflection.TargetInvocationException exception)
            {
                var inner = exception.InnerException;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }
                throw new Exception(inner, "Post-build error:");
            }
        }

        private static bool
        CheckIfModulesNeedRebuilding(
            System.Type metaType)
        {
            // not all build modes need to determine if modules are up-to-date
            var evaluationRequiredAttr =
                metaType.GetCustomAttributes(typeof(EvaluationRequiredAttribute), false) as EvaluationRequiredAttribute[];
            if (0 == evaluationRequiredAttr.Length)
            {
                Log.DebugMessage("No Bam.Core.EvaluationRequired attribute on build mode metadata, assume rebuilds necessary");
                return false;
            }

            if (!evaluationRequiredAttr[0].Enabled)
            {
                Log.DebugMessage("Module evaluation disabled");
                return false;
            }

            Log.DebugMessage("Module evaluation enabled");

            var cancellationSource = new System.Threading.CancellationTokenSource();
            var cancellationToken = cancellationSource.Token;

            // LongRunning is absolutely necessary in order to achieve paralleism
            var creationOpts = System.Threading.Tasks.TaskCreationOptions.LongRunning;
            var continuationOpts = System.Threading.Tasks.TaskContinuationOptions.LongRunning;

            var threadCount = 1;
            var scheduler = new LimitedConcurrencyLevelTaskScheduler(threadCount);

            var factory = new System.Threading.Tasks.TaskFactory(
                    cancellationToken,
                    creationOpts,
                    continuationOpts,
                    scheduler);

            var graph = Graph.Instance;
            graph.MetaData = factory;

            foreach (var rank in graph.Reverse())
            {
                foreach (Module module in rank)
                {
                    module.Evaluate();
                }
            }

            return true;
        }

        public void
        Run()
        {
            Log.Detail("Executing graph");
            var cleanFirst = CommandLineProcessor.Evaluate(new CleanFirst());
            if (cleanFirst && System.IO.Directory.Exists(State.BuildRoot))
            {
                Log.Info("Deleting build root '{0}'", State.BuildRoot);
                try
                {
                    System.IO.Directory.Delete(State.BuildRoot, true);
                }
                catch (System.IO.IOException ex)
                {
                    Log.Info("Failed to delete build root, because {0}. Continuing", ex.Message);
                }
            }

            // TODO: should the rank collections be sorted, so that modules with fewest dependencies are first?

            var graph = Graph.Instance;
            var metaDataType = graph.BuildModeMetaData.GetType();
            var useEvaluation = CheckIfModulesNeedRebuilding(metaDataType);
            var explainRebuild = CommandLineProcessor.Evaluate(new ExplainBuildReason());

            ExecutePreBuild(metaDataType);

            if (!System.IO.Directory.Exists(State.BuildRoot))
            {
                System.IO.Directory.CreateDirectory(State.BuildRoot);
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
                        var context = new ExecutionContext(useEvaluation, explainRebuild);
                        var task = factory.StartNew(() =>
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    return;
                                }
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
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    return;
                                }

                                try
                                {
                                    (module as IModuleExecution).Execute(context);
                                }
                                catch (Exception ex)
                                {
                                    abortException = ex;
                                    cancellationSource.Cancel();
                                }
                                finally
                                {
                                    if (context.OutputStringBuilder.Length > 0)
                                    {
                                        Log.Info(context.OutputStringBuilder.ToString());
                                    }
                                    if (context.ErrorStringBuilder.Length > 0)
                                    {
                                        Log.Info(context.ErrorStringBuilder.ToString());
                                    }
                                }
                            });
                        tasks.Add(task);
                        module.ExecutionTask = task;
                    }
                }
                try
                {
                    System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
                }
                catch (System.AggregateException exception)
                {
                    if (!(exception.InnerException is System.Threading.Tasks.TaskCanceledException))
                    {
                        throw new Exception(exception, "Error during threaded build");
                    }
                }
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
                        var context = new ExecutionContext(useEvaluation, explainRebuild);
                        try
                        {
                            module.Execute(context);
                        }
                        catch (Exception ex)
                        {
                            abortException = ex;
                            break;
                        }
                        finally
                        {
                            if (context.OutputStringBuilder.Length > 0)
                            {
                                Log.Info(context.OutputStringBuilder.ToString());
                            }
                            if (context.ErrorStringBuilder.Length > 0)
                            {
                                Log.Info(context.ErrorStringBuilder.ToString());
                            }
                        }
                    }
                }
            }

            if (null != abortException)
            {
                throw abortException;
            }

            ExecutePostBuild(metaDataType);
        }
    }
}
