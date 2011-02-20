// <copyright file="DependencyGraph.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public class DependencyGraph : System.Collections.IEnumerable
    {
        public System.Collections.IEnumerator GetEnumerator()
        {
            return new DependencyNodeEnumerator(this);
        }

        private System.Collections.Generic.List<DependencyNodeCollection> rankList = new System.Collections.Generic.List<DependencyNodeCollection>();

        private System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, DependencyNode>> uniqueNameToNodeDictionary = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, DependencyNode>>();

        public DependencyGraph()
        {
            if (null != State.Get("System", "Graph"))
            {
                throw new Exception("There can only be one Graph created");
            }

            this.ExecutedNodes = new DependencyNodeCollection();
        }

        public int RankCount
        {
            get
            {
                if (null == this.rankList)
                {
                    throw new Exception("DependencyGraph has been ill-constructed");
                }
                return this.rankList.Count;
            }
        }
        
        public DependencyNodeCollection this[int rank]
        {
            get
            {
                if (rank < 0)
                {
                    throw new Exception("Invalid rank");
                }
                if (rank >= this.RankCount)
                {
                    throw new Exception("Invalid rank");
                }
                return this.rankList[rank];
            }
        }

        public DependencyNodeCollection ExecutedNodes
        {
            get;
            private set;
        }
        
        public void AddTopLevelModule(System.Type moduleType, Target target)
        {
            AddModule(moduleType, 0, null, target);
        }
        
        public int TotalNodeCount
        {
            get
            {
                int totalNodeCount = 0;
                foreach (DependencyNodeCollection collection in this.rankList)
                {
                    totalNodeCount += collection.Count;
                }
                return totalNodeCount;
            }
        }
        
        private DependencyNode AddModule(System.Type moduleType, int rank, DependencyNode parent, Target target)
        {
            string toolchainImplementation = ModuleUtilities.GetToolchainImplementation(moduleType);

            Target targetUsed = target;
            bool isComplete = targetUsed.IsFullyFormed;
            bool consistentToolChain = targetUsed.Toolchain == toolchainImplementation; // TODO: not sure if this one is necessary at this point as it should've been checked before this call
            if (!isComplete || !consistentToolChain)
            {
                targetUsed = new Target(target, toolchainImplementation);
            }

            ModuleTargetsAttribute[] moduleTargetFilters = moduleType.GetCustomAttributes(typeof(ModuleTargetsAttribute), false) as ModuleTargetsAttribute[];
            if (moduleTargetFilters.Length > 0)
            {
                if (!targetUsed.MatchFilters(moduleTargetFilters[0]))
                {
                    Log.DebugMessage("Module '{0}' with filters '{1}' does not match target '{2}'", moduleType.ToString(), moduleTargetFilters[0].ToString(), targetUsed.ToString());
                    return null;
                }
            }

            DependencyNode moduleNode = new DependencyNode(moduleType, parent, targetUsed, -1, false);
            AddDependencyNodeToCollection(moduleNode, rank);

            return moduleNode;
        }
        
        private void AddDependencyNodeToCollection(DependencyNode moduleNode, int rank)
        {
            while (this.rankList.Count <= rank)
            {
                DependencyNodeCollection newRank = new DependencyNodeCollection(this.rankList.Count);
                this.rankList.Insert(this.rankList.Count, newRank);
            }

            this.rankList[rank].Add(moduleNode);
            moduleNode.Rank = rank;

            if (!this.uniqueNameToNodeDictionary.ContainsKey(moduleNode.UniqueModuleName))
            {
                this.uniqueNameToNodeDictionary[moduleNode.UniqueModuleName] = new System.Collections.Generic.Dictionary<string, DependencyNode>();
            }
            this.uniqueNameToNodeDictionary[moduleNode.UniqueModuleName][moduleNode.Target.Key] = moduleNode;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public void Dump()
        {
            foreach (DependencyNodeCollection nodeCollection in this.rankList)
            {
                Core.Log.DebugMessage("Rank {0}", nodeCollection.Rank);
                foreach (DependencyNode node in nodeCollection)
                {
                    Core.Log.DebugMessage("\t{0}", node.ToString());
                }
            }
        }

        private void CreateOptionCollections()
        {
            foreach (DependencyNodeCollection nodeCollection in this.rankList)
            {
                foreach (DependencyNode node in nodeCollection)
                {
                    node.CreateOptionCollection();
                }
            }
        }

        public void PopulateGraph()
        {
            if (0 == this.RankCount)
            {
                return;
            }

            this.AddChildAndExternalDependents();
            Core.State.ReadOnly = true;
            this.CreateOptionCollections();
            this.AddInjectedDependents();
        }

        private DependencyNode FindNodeForTargettedModule(string moduleName, Target target)
        {
            bool moduleNameExists = this.uniqueNameToNodeDictionary.ContainsKey(moduleName);
            if (!moduleNameExists)
            {
                return null;
            }

            bool moduleNameForTargetExists = this.uniqueNameToNodeDictionary[moduleName].ContainsKey(target.Key);
            if (!moduleNameForTargetExists)
            {
                return null;
            }

            return this.uniqueNameToNodeDictionary[moduleName][target.Key];
        }

        private DependencyNode FindOrCreateUnparentedNode(System.Type moduleType, string moduleName, Target target, int currentRank, System.Collections.Generic.Dictionary<DependencyNode, int> nodesToMove)
        {
            string toolchainImplementation = ModuleUtilities.GetToolchainImplementation(moduleType);
            Target targetUsed = target;
            if (targetUsed.Toolchain != toolchainImplementation)
            {
                targetUsed = new Target(target, toolchainImplementation);
            }

            DependencyNode node = this.FindNodeForTargettedModule(moduleName, targetUsed);
            if (null != node)
            {
                if (node.Rank < currentRank + 1)
                {
                    nodesToMove[node] = currentRank + 1 - node.Rank;
                }
            }
            else
            {
                DependencyNode parentNode = null;
                node = this.AddModule(moduleType, currentRank + 1, parentNode, targetUsed);
            }

            return node;
        }

        private void AddChildAndExternalDependents()
        {
            int currentRank = 0;
            while (currentRank < this.RankCount)
            {
                System.Collections.Generic.Dictionary<DependencyNode, int> nodesToMove = new System.Collections.Generic.Dictionary<DependencyNode,int>();

                DependencyNodeCollection rankNodes = this[currentRank];
                foreach (DependencyNode node in rankNodes)
                {
                    TypeArray externalDependentModuleTypes = ModuleUtilities.GetExternalDependents(node.Module, node.Target);
                    TypeArray additionalExternalDependentModuleTypes = null;
                    IIdentifyExternalDependencies identifyExternalDependencies = node.Module as IIdentifyExternalDependencies;
                    if (null != identifyExternalDependencies)
                    {
                        additionalExternalDependentModuleTypes = identifyExternalDependencies.IdentifyExternalDependencies(node.Target);
                        if (null != externalDependentModuleTypes)
                        {
                            externalDependentModuleTypes.AddRange(additionalExternalDependentModuleTypes);
                        }
                        else
                        {
                            externalDependentModuleTypes = additionalExternalDependentModuleTypes;
                        }
                    }
                    if (externalDependentModuleTypes != null)
                    {
                        foreach (System.Type dependentModuleType in externalDependentModuleTypes)
                        {
                            DependencyNode newNode = this.FindOrCreateUnparentedNode(dependentModuleType, dependentModuleType.Name, node.Target, currentRank, nodesToMove);

                            if (newNode.Module is IInjectModules)
                            {
                                nodesToMove[newNode] = currentRank + 2 - newNode.Rank;
                            }

                            node.AddExternalDependent(newNode);
                        }
                    }

                    TypeArray externalRequiredModuleTypes = ModuleUtilities.GetRequiredDependents(node.Module, node.Target);
                    if (externalRequiredModuleTypes != null)
                    {
                        foreach (System.Type requiredModuleType in externalRequiredModuleTypes)
                        {
                            DependencyNode newNode = this.FindOrCreateUnparentedNode(requiredModuleType, requiredModuleType.Name, node.Target, currentRank, nodesToMove);

                            node.AddRequiredDependent(newNode);
                        }
                    }

                    if (node.Module is INestedDependents)
                    {
                        INestedDependents nestedDependentsInterface = node.Module as INestedDependents;
                        ModuleCollection nestedDependentModules = nestedDependentsInterface.GetNestedDependents(node.Target);
                        if (null == nestedDependentModules)
                        {
                            throw new Exception(System.String.Format("Module '{0}' implements Opus.Core.INestedDependents but returns null"));
                        }

                        int childIndex = 0;
                        foreach (IModule nestedModule in nestedDependentModules)
                        {
                            System.Type nestedModuleType = nestedModule.GetType();
                            // TODO: the child index here might be a problem
                            string nestedModuleUniqueName = node.GetChildModuleName(nestedModuleType, childIndex);
                            DependencyNode newNode = this.FindNodeForTargettedModule(nestedModuleUniqueName, node.Target);
                            if (null != newNode)
                            {
                                if (newNode.Rank < currentRank + 1)
                                {
                                    nodesToMove[newNode] = currentRank + 1 - newNode.Rank;
                                }

                                if (newNode.Parent != node)
                                {
                                    throw new Exception(System.String.Format("Node '{0}' that already existed in the graph currently has parent '{1}' but should be '{2}'", newNode.UniqueModuleName, newNode.Parent.UniqueModuleName, node.UniqueModuleName));
                                }
                            }
                            else
                            {
                                newNode = new DependencyNode(nestedModule, node, node.Target, childIndex, true);
                                ++childIndex;

                                this.AddDependencyNodeToCollection(newNode, currentRank + 1);
                            }
                        }
                    }
                }

                if (nodesToMove.Count > 0)
                {
                    foreach (System.Collections.Generic.KeyValuePair<DependencyNode, int> value in nodesToMove)
                    {
                        this.IncrementNodeRank(value.Key, value.Value);
                    }
                }

                ++currentRank;
            }
        }

        private void IncrementNodeRank(DependencyNode node, int ranksToMove)
        {
            if (node.Children != null)
            {
                foreach (DependencyNode childNode in node.Children)
                {
                    this.IncrementNodeRank(childNode, ranksToMove);
                }
            }
            if (node.ExternalDependents != null)
            {
                foreach (DependencyNode dependentNode in node.ExternalDependents)
                {
                    this.IncrementNodeRank(dependentNode, ranksToMove);
                }
            }
            if (node.RequiredDependents != null)
            {
                foreach (DependencyNode requiredNode in node.RequiredDependents)
                {
                    this.IncrementNodeRank(requiredNode, ranksToMove);
                }
            }

            this[node.Rank].Remove(node);
            this.AddDependencyNodeToCollection(node, node.Rank + ranksToMove);
        }

        private void AddInjectedDependents()
        {
            int currentRank = 0;
            do
            {
                DependencyNodeCollection nodeCollection = this.rankList[currentRank];
                foreach (DependencyNode node in nodeCollection)
                {
                    IInjectModules injectModules = node.Module as IInjectModules;
                    if (null != injectModules)
                    {
                        ModuleCollection injectedModules = injectModules.GetInjectedModules(node.Target);
                        foreach (IModule module in injectedModules)
                        {
                            DependencyNodeCollection externalDependencyForCollection = node.ExternalDependentFor;
                            if (null == externalDependencyForCollection)
                            {
                                // check the parent, in case this was part of a collection
                                if (null != node.Parent)
                                {
                                    externalDependencyForCollection = node.Parent.ExternalDependentFor;
                                }
                            }
                            if (null == externalDependencyForCollection)
                            {
                                if (null != node.Parent)
                                {
                                    throw new Exception(System.String.Format("Node '{0}' nor its parent '{1}' are dependees", node.UniqueModuleName, node.Parent.UniqueModuleName), false);
                                }
                                else
                                {
                                    throw new Exception(System.String.Format("Node '{0}' is not a dependee", node.UniqueModuleName), false);
                                }
                            }

                            DependencyNode sourceOfDependency = null;
                            foreach (DependencyNode ext in externalDependencyForCollection)
                            {
                                foreach (DependencyNode extDep in ext.ExternalDependents)
                                {
                                    if (extDep == node || extDep == node.Parent)
                                    {
                                        sourceOfDependency = ext;
                                        break;
                                    }
                                }

                                if (null != sourceOfDependency)
                                {
                                    break;
                                }
                            }
                            if (null == sourceOfDependency)
                            {
                                throw new Exception(System.String.Format("Unable to locate the dependency of '{0}'", module.GetType().ToString()), false);
                            }

                            int childIndex;
                            if (sourceOfDependency.Children != null)
                            {
                                childIndex = sourceOfDependency.Children.Count;
                            }
                            else
                            {
                                childIndex = 0;
                            }

                            // TODO: would like to override the name in a better way
                            DependencyNode newNode = new DependencyNode(module, sourceOfDependency, sourceOfDependency.Target, childIndex, true);
                            newNode.AddExternalDependent(node);
                            this.AddDependencyNodeToCollection(newNode, node.Rank - 1);

                            // module inherits the options from the source of the dependency
                            newNode.CreateOptionCollection();
                        }
                    }
                }
                ++currentRank;
            }
            while (currentRank < this.RankCount);
        }
    }
}