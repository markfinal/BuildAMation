#region License
// Copyright 2010-2014 Mark Final
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
#endregion
namespace Bam.Core
{
    public sealed class BuildAgent
    {
        public
        BuildAgent(
            string name,
            object builder,
            System.Threading.ManualResetEvent reportFailure)
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

        private static void
        Run(
            object obj)
        {
            var agent = obj as BuildAgent;
            var node = agent.Node;
            var builder = agent.Builder as IBuilder;

            Log.DebugMessage("Agent '{0}' is running node '{1}' from rank {2}", agent.Name, node.UniqueModuleName, node.NodeCollection.Rank);
            var owningNode = node.Module.OwningNode;

            if (owningNode != node)
            {
                throw new Exception("Node '{0}' has a module with different node ownership '{1}'. That should not be possible", node.UniqueModuleName, owningNode.UniqueModuleName);
            }

            var buildFunction = node.BuildFunction;
            var arguments = new object[] { node.Module, false };
            try
            {
                node.Data = buildFunction.Invoke(builder, arguments);
            }
            catch (Core.Exception exception)
            {
                Exception.DisplayException(exception, "Build function '{0}' threw an exception", buildFunction.ToString());
                arguments[1] = false;
            }
            catch (System.Reflection.TargetInvocationException exception)
            {
                Exception.DisplayException(exception, "Build function '{0}' error", buildFunction.ToString());
                arguments[1] = false;
            }
            var success = (bool)arguments[1];
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

        public void
        Execute(
            DependencyNode node)
        {
            node.BuildState = EBuildState.Pending;

            this.IsAvailable.Reset();
            this.Node = node;

            // queue up work for the thread pool
            System.Threading.ThreadPool.QueueUserWorkItem(Run, this);
        }
    }
}
