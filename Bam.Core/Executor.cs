#region License
// Copyright (c) 2010-2016, Mark Final
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
    /// Management of the graph execution.
    /// </summary>
    public sealed class Executor
    {
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
            var graph = Graph.Instance;
            var modulesNeedEvaluating = new Array<Module>();

            // not all build modes need to determine if modules are up-to-date
            var evaluationRequiredAttr =
                metaType.GetCustomAttributes(typeof(EvaluationRequiredAttribute), false) as EvaluationRequiredAttribute[];
            if (0 == evaluationRequiredAttr.Length)
            {
                // query if any individual modules override this
                foreach (var rank in graph.Reverse())
                {
                    foreach (Module module in rank)
                    {
                        var moduleEvaluationRequiredAttr = module.GetType().GetCustomAttributes(typeof(EvaluationRequiredAttribute), true) as EvaluationRequiredAttribute[];
                        if (moduleEvaluationRequiredAttr.Length > 0 && moduleEvaluationRequiredAttr[0].Enabled)
                        {
                            modulesNeedEvaluating.Add(module);
                        }
                    }
                }

                if (0 == modulesNeedEvaluating.Count)
                {
                    Log.DebugMessage("No Bam.Core.EvaluationRequired attribute on build mode metadata, assume rebuilds necessary");
                    return false;
                }
            }

            if ((evaluationRequiredAttr.Length > 0) && !evaluationRequiredAttr[0].Enabled && 0 == modulesNeedEvaluating.Count)
            {
                Log.DebugMessage("Module evaluation disabled");
                return false;
            }

            using (var cancellationSource = new System.Threading.CancellationTokenSource())
            {
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

                graph.MetaData = factory;

                if (0 == modulesNeedEvaluating.Count)
                {
                    Log.DebugMessage("Module evaluation enabled for build mode {0}", graph.Mode);
                    foreach (var rank in graph.Reverse())
                    {
                        foreach (Module module in rank)
                        {
                            module.Evaluate();
                        }
                    }
                }
                else
                {
                    Log.DebugMessage("Module evaluation disabled for build mode {0}, but enabled for individual modules:", graph.Mode);
                    foreach (var module in modulesNeedEvaluating)
                    {
                        Log.DebugMessage("\tEvaluation for module {0}", module.GetType().ToString());
                        module.Evaluate();
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Run the executor on the dependency graph.
        /// Graph execution can be single threaded or many threaded, using .NET framework Tasks.
        /// Graph execution can have a pre- and post- build step, defined by the build mode meta data.
        /// Modules are executed in rank, honouring dependencies.
        /// </summary>
        public void
        Run()
        {
            Log.Detail("Running build");

            // TODO: should the rank collections be sorted, so that modules with fewest dependencies are first?

            var graph = Graph.Instance;
            var metaDataType = graph.BuildModeMetaData.GetType();
            var useEvaluation = CheckIfModulesNeedRebuilding(metaDataType);
            var explainRebuild = CommandLineProcessor.Evaluate(new Options.ExplainBuildReason());
            var immediateOutput = CommandLineProcessor.Evaluate(new Options.ImmediateOutput());

            ExecutePreBuild(metaDataType);

            var threadCount = CommandLineProcessor.Evaluate(new Options.MultiThreaded());
            if (0 == threadCount)
            {
                threadCount = System.Environment.ProcessorCount;
            }

            System.Exception abortException = null;
            if (threadCount > 1)
            {
                using (var cancellationSource = new System.Threading.CancellationTokenSource())
                {
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
                            var context = new ExecutionContext(useEvaluation, explainRebuild, immediateOutput);
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
                                        if (context.OutputStringBuilder != null && context.OutputStringBuilder.Length > 0)
                                        {
                                            Log.Info(context.OutputStringBuilder.ToString());
                                        }
                                        if (context.ErrorStringBuilder != null && context.ErrorStringBuilder.Length > 0)
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
                        var context = new ExecutionContext(useEvaluation, explainRebuild, immediateOutput);
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
                            if (context.OutputStringBuilder != null && context.OutputStringBuilder.Length > 0)
                            {
                                Log.Info(context.OutputStringBuilder.ToString());
                            }
                            if (context.ErrorStringBuilder != null && context.ErrorStringBuilder.Length > 0)
                            {
                                Log.Info(context.ErrorStringBuilder.ToString());
                            }
                        }
                    }
                }
            }

            if (null != abortException)
            {
                throw new Exception(abortException, "Error during {0}threaded build", (threadCount > 1) ? string.Empty : "non-");
            }

            ExecutePostBuild(metaDataType);
        }
    }
}
