// <copyright file="BuildAgent.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class BuildAgent
    {   
        public BuildAgent(string name, object builder)
        {
            this.Name = name;
            this.Builder = builder;
            this.DoneEvent = new System.Threading.ManualResetEvent(false);
            this.Success = true;

            System.Threading.ParameterizedThreadStart threadStart = new System.Threading.ParameterizedThreadStart(Run);
            this.Thread = new System.Threading.Thread(threadStart);
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
        
        public System.Threading.Thread Thread
        {
            get;
            private set;
        }

        private DependencyNode Node
        {
            get;
            set;
        }

        public System.Threading.ManualResetEvent DoneEvent
        {
            get;
            private set;
        }

        public bool Success
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
                Log.MessageAll("Build function threw an exception for '{0}' in target '{1}': '{2}'\n{3}", node.UniqueModuleName, node.Target.ToString(), exception.Message, exception.StackTrace);
                arguments[1] = false;
            }
            catch (System.Reflection.TargetInvocationException exception)
            {
                Core.Log.MessageAll("Build function threw an exception for '{0}' in target '{1}': '{2}'", node.UniqueModuleName, node.Target.ToString(), exception.Message);
                System.Exception innerException = exception;
                while (innerException.InnerException != null)
                {
                    innerException = innerException.InnerException;
                    Core.Log.MessageAll("Inner exception: {0}, {1}", innerException.GetType().ToString(), innerException.Message);
                }
                Core.Log.MessageAll(innerException.StackTrace);
                arguments[1] = false;
            }
            bool success = (bool)arguments[1];
            agent.Success = success;
            
            if (success)
            {
                node.BuildState = EBuildState.Succeeded;
            }
            else
            {
                node.BuildState = EBuildState.Failed;
                Log.MessageAll("Failed while building module '{0}' for target '{1}'", node.ModuleName, node.Target.ToString());
            }

            agent.Node = null;

            agent.DoneEvent.Set();
        }
        
        public void Execute(DependencyNode node)
        {
            node.BuildState = EBuildState.Pending;

            this.DoneEvent.Reset();
            this.Success = false;
            this.Node = node;

            // queue up work for the thread pool
            System.Threading.ThreadPool.QueueUserWorkItem(Run, this);
        }
    }
}