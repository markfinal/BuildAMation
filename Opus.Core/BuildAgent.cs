// <copyright file="BuildAgent.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class BuildAgent
    {
        public BuildAgent(string name, object builder, System.Threading.ManualResetEvent reportFailure)
        {
            this.Name = name;
            this.Builder = builder;
            this.IsAvailable = new System.Threading.ManualResetEvent(true);
            this.ReportFailure = reportFailure;
        }
        
        public string Name
        {
            get;
            private set;
        }
        
        private object Builder
        {
            get;
            set;
        }
        
        private DependencyNode Node
        {
            get;
            set;
        }

        public System.Threading.ManualResetEvent IsAvailable
        {
            get;
            private set;
        }

        private System.Threading.ManualResetEvent ReportFailure
        {
            get;
            set;
        }

        private static void Run(object obj)
        {
            BuildAgent agent = obj as BuildAgent;
            DependencyNode node = agent.Node;
            IBuilder builder = agent.Builder as IBuilder;

            Log.DebugMessage("Agent '{0}' is running node '{1}'", agent.Name, node.UniqueModuleName);

            if (node.Module.OwningNode != node)
            {
                throw new Exception(System.String.Format("Node '{0}' has a module with different node ownership '{1}'. That should not be possible",
                                    node.UniqueModuleName, node.Module.OwningNode.UniqueModuleName), false);
            }

            System.Reflection.MethodInfo buildFunction = node.BuildFunction;
            object[] arguments = new object[] { node.Module, false };
            try
            {
                node.Data = buildFunction.Invoke(builder, arguments);
            }
            catch (Core.Exception exception)
            {
                Exception.DisplayException(exception, System.String.Format("Build function '{0}' threw an exception", buildFunction.ToString()));
#if false
                Log.MessageAll("Build function threw an exception for '{0}' in target '{1}': '{2}'\n{3}", node.UniqueModuleName, node.Target.ToString(), exception.Message, exception.StackTrace);
#endif
                arguments[1] = false;
            }
            catch (System.Reflection.TargetInvocationException exception)
            {
                Exception.DisplayException(exception, System.String.Format("Build function '{0}' error", buildFunction.ToString()));
#if false
                Core.Log.MessageAll("Build function threw an exception for '{0}' in target '{1}': '{2}'", node.UniqueModuleName, node.Target.ToString(), exception.Message);
                System.Exception innerException = exception;
                while (innerException.InnerException != null)
                {
                    innerException = innerException.InnerException;
                    Core.Log.MessageAll("Inner exception: {0}, {1}", innerException.GetType().ToString(), innerException.Message);
                }
                Core.Log.MessageAll(innerException.StackTrace);
#endif
                arguments[1] = false;
            }
            bool success = (bool)arguments[1];
            if (success)
            {
                node.BuildState = EBuildState.Succeeded;
            }
            else
            {
                node.BuildState = EBuildState.Failed;
                Log.ErrorMessage("Failed while building module '{0}' for target '{1}'", node.ModuleName, node.Target.ToString());
                agent.ReportFailure.Set();
            }

            agent.Node = null;
            agent.IsAvailable.Set();
        }
        
        public void Execute(DependencyNode node)
        {
            node.BuildState = EBuildState.Pending;

            this.IsAvailable.Reset();
            this.Node = node;

            // queue up work for the thread pool
            System.Threading.ThreadPool.QueueUserWorkItem(Run, this);
        }
    }
}