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
        
        public void AddTopLevelModule(System.Type moduleType, BaseTarget baseTarget)
        {
            AddModule(moduleType, 0, null, baseTarget);
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
        
        private DependencyNode AddModule(System.Type moduleType, int rank, DependencyNode parent, BaseTarget baseTarget)
        {
            IToolset toolset = ModuleUtilities.GetToolsetForModule(moduleType);
            Target targetUsed = Target.GetInstance(baseTarget, toolset);

            ModuleTargetsAttribute[] moduleTargetFilters = moduleType.GetCustomAttributes(typeof(ModuleTargetsAttribute), false) as ModuleTargetsAttribute[];
            if (moduleTargetFilters.Length > 0)
            {
                if (!TargetUtilities.MatchFilters(targetUsed, moduleTargetFilters[0]))
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
            Log.DebugMessage("-------------GRAPH--------------");
            foreach (DependencyNodeCollection nodeCollection in this.rankList)
            {
                Log.DebugMessage("Rank {0}", nodeCollection.Rank);
                foreach (DependencyNode node in nodeCollection)
                {
                    Log.DebugMessage("\t{0}", node.ToString());
                }
            }
            Log.DebugMessage("------------/GRAPH--------------");
        }

        private void CreateOptionCollections()
        {
            Log.DebugMessage("--------------------------------");
            Log.DebugMessage("Create option collections: START");
            Log.DebugMessage("--------------------------------");

            // in increasing rank, generate option collections, so that child inherit their parent options
            // if so desired
            foreach (DependencyNodeCollection nodeCollection in this.rankList)
            {
                foreach (DependencyNode node in nodeCollection)
                {
                    // empty build functions do not equate to no option collection
                    node.CreateOptionCollection();
                }
            }

            Log.DebugMessage("-----------------------------");
            Log.DebugMessage("Finalizing option collections");
            Log.DebugMessage("-----------------------------");

            // in decreasing rank, finalize option collections, so that dependent options can get their data
            for (int rank = this.RankCount - 1; rank >= 0; --rank)
            {
                DependencyNodeCollection nodeCollection = this.rankList[rank];
                foreach (DependencyNode node in nodeCollection)
                {
                    if (null != node.Module.Options)
                    {
                        node.Module.Options.FinalizeOptions(node);
                    }
                }
            }

            Log.DebugMessage("------------------------------");
            Log.DebugMessage("Create option collections: END");
            Log.DebugMessage("------------------------------");
        }

        public void PopulateGraph()
        {
            if (0 == this.RankCount)
            {
                return;
            }

            {
                TimeProfile profile = new TimeProfile(ETimingProfiles.PopulateGraph);
                profile.StartProfile();

                this.AddChildAndExternalDependents();

                profile.StopProfile();
                State.TimingProfiles[(int)ETimingProfiles.PopulateGraph] = profile;
            }

            {
                TimeProfile profile = new TimeProfile(ETimingProfiles.CreateOptionCollections);
                profile.StartProfile();

                this.CreateOptionCollections();

                profile.StopProfile();
                State.TimingProfiles[(int)ETimingProfiles.CreateOptionCollections] = profile;
            }

            {
                TimeProfile profile = new TimeProfile(ETimingProfiles.HandleInjectionDependents);
                profile.StartProfile();

                this.AddInjectedDependents();

                profile.StopProfile();
                State.TimingProfiles[(int)ETimingProfiles.HandleInjectionDependents] = profile;
            }
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
            IToolset toolset = ModuleUtilities.GetToolsetForModule(moduleType);
            Target targetUsed = Target.GetInstance((BaseTarget)target, toolset);

            DependencyNode node = this.FindNodeForTargettedModule(moduleName, targetUsed);
            if (null != node)
            {
                if (node.Rank < currentRank + 1)
                {
                    nodesToMove[node] = currentRank + 1;
                }
            }
            else
            {
                DependencyNode parentNode = null;
                node = this.AddModule(moduleType, currentRank + 1, parentNode, (BaseTarget)targetUsed);
            }

            return node;
        }

        private void AddChildAndExternalDependents()
        {
            DependencyNodeCollection nodesWithForwardedDependencies = new DependencyNodeCollection();

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
                        bool hasForwardedDeps = (node.Module is IForwardDependenciesOn);
                        foreach (System.Type dependentModuleType in externalDependentModuleTypes)
                        {
                            DependencyNode newNode = this.FindOrCreateUnparentedNode(dependentModuleType, dependentModuleType.FullName, node.Target, currentRank, nodesToMove);
                            if (null == newNode)
                            {
                                continue;
                            }

                            if (newNode.Module is IInjectModules)
                            {
                                nodesToMove[newNode] = currentRank + 2; // +2 to jump over the collection of objects
                            }
                            else if (newNode.Rank < currentRank)
                            {
                                nodesToMove[newNode] = currentRank + 1;
                            }

                            node.AddExternalDependent(newNode);

                            //cache these for the end of the populate operation
                            if (hasForwardedDeps)
                            {
                                if (!nodesWithForwardedDependencies.Contains(node))
                                {
                                    nodesWithForwardedDependencies.Add(node);
                                    newNode.ConsiderForBuild = false;
                                }
                            }
                        }
                    }

                    TypeArray externalRequiredModuleTypes = ModuleUtilities.GetRequiredDependents(node.Module, node.Target);
                    if (externalRequiredModuleTypes != null)
                    {
                        foreach (System.Type requiredModuleType in externalRequiredModuleTypes)
                        {
                            DependencyNode newNode = this.FindOrCreateUnparentedNode(requiredModuleType, requiredModuleType.FullName, node.Target, currentRank, nodesToMove);
                            if (null == newNode)
                            {
                                continue;
                            }

                            node.AddRequiredDependent(newNode);
                        }
                    }

                    if (node.Module is INestedDependents)
                    {
                        INestedDependents nestedDependentsInterface = node.Module as INestedDependents;
                        ModuleCollection nestedDependentModules = nestedDependentsInterface.GetNestedDependents(node.Target);
                        if (null == nestedDependentModules)
                        {
                            throw new Exception("Module '{0}' implements Opus.Core.INestedDependents but returns null");
                        }

                        int childIndex = 0;
                        foreach (BaseModule nestedModule in nestedDependentModules)
                        {
                            System.Type nestedModuleType = nestedModule.GetType();
                            // TODO: the child index here might be a problem
                            string nestedModuleUniqueName = node.GetChildModuleName(nestedModuleType, childIndex);

                            IToolset nestedToolset = ModuleUtilities.GetToolsetForModule(nestedModuleType);
                            Target nestedTargetUsed = Target.GetInstance((BaseTarget)node.Target, nestedToolset);

                            DependencyNode newNode = this.FindNodeForTargettedModule
(nestedModuleUniqueName, nestedTargetUsed);
                            if (null != newNode)
                            {
                                if (newNode.Rank < currentRank + 1)
                                {
                                    nodesToMove[newNode] = currentRank + 1;
                                }

                                if (newNode.Parent != node)
                                {
                                    throw new Exception("Node '{0}' (target {1}) that already existed in the graph currently has parent '{2}' (target {3}) but should be '{4}' (target {5})", newNode.UniqueModuleName, newNode.Target.ToString(), newNode.Parent.UniqueModuleName, newNode.Parent.Target.ToString(), node.UniqueModuleName, node.Target.ToString());
                                }
                            }
                            else
                            {
                                newNode = new DependencyNode(nestedModule, node, nestedTargetUsed, childIndex, true);
                                this.AddDependencyNodeToCollection(newNode, currentRank + 1);
                            }

                            ++childIndex;
                        }
                    }
                }

                if (nodesToMove.Count > 0)
                {
                    // flatten the hierarchy of nodes, so there is one move per dependency, to the maximum rank needed
                    System.Collections.Generic.Dictionary<DependencyNode, int> flattenedList = new System.Collections.Generic.Dictionary<DependencyNode, int>();
                    foreach (System.Collections.Generic.KeyValuePair<DependencyNode, int> value in nodesToMove)
                    {
                        DependencyNode node = value.Key;
                        int targetRank = value.Value;

                        this.FlattenHierarchy(node, targetRank, flattenedList, 0);
                    }

                    // now move
                    foreach (System.Collections.Generic.KeyValuePair<DependencyNode, int> value in flattenedList)
                    {
                        DependencyNode node = value.Key;
                        int targetRank = value.Value;

                        this[node.Rank].Remove(node);
                        this.AddDependencyNodeToCollection(node, targetRank);
                    }
                }

                ++currentRank;
            }

            // now, at the very end, link up forwarded dependencies
            // performed at the end because the existing dependencies are already in place
            // and they will automatically satisfy the new links
            foreach (DependencyNode nodeWith in nodesWithForwardedDependencies)
            {
                if (nodeWith.ExternalDependentFor != null)
                {
                    foreach (DependencyNode forNode in nodeWith.ExternalDependentFor)
                    {
                        foreach (DependencyNode forwardedDependency in nodeWith.ExternalDependents)
                        {
                            forNode.AddExternalDependent(forwardedDependency);
                            if (!forwardedDependency.ConsiderForBuild)
                            {
                                Log.DebugMessage("Node '{0}' was being ignored, but is needed by a forwarded dependent", forwardedDependency.UniqueModuleName);
                                forwardedDependency.ConsiderForBuild = true;
                            }
                        }
                    }
                }
            }

            // ensure nodes not under consideration for build do not build their children either
            currentRank = 0;
            while (currentRank < this.RankCount)
            {
                DependencyNodeCollection rankNodes = this[currentRank];
                foreach (DependencyNode node in rankNodes)
                {
                    if (!node.ConsiderForBuild && (null != node.Children))
                    {
                        Log.DebugMessage("Node '{0}' is not under consideration", node.UniqueModuleName);
                        foreach (DependencyNode child in node.Children)
                        {
                            Log.DebugMessage("\tMarking '{0}' as also not under consideration", child.UniqueModuleName);
                            child.ConsiderForBuild = false;
                        }
                    }
                }

                ++currentRank;
            }
        }

        private void FlattenHierarchy(DependencyNode node, int targetRank, System.Collections.Generic.Dictionary<DependencyNode, int> flattenedList, int depth)
        {
            if (!flattenedList.ContainsKey(node))
            {
                flattenedList.Add(node, targetRank);
            }
            else if (targetRank > flattenedList[node])
            {
                flattenedList[node] = targetRank;
            }

            int rankDelta = targetRank - node.Rank;

            if (node.Children != null)
            {
                foreach (DependencyNode childNode in node.Children)
                {
                    int childTargetRank = childNode.Rank + rankDelta;
                    this.FlattenHierarchy(childNode, childTargetRank, flattenedList, depth + 1);
                }
            }
            if (node.ExternalDependents != null)
            {
                foreach (DependencyNode dependentNode in node.ExternalDependents)
                {
                    int dependentTargetRank = dependentNode.Rank + rankDelta;
                    this.FlattenHierarchy(dependentNode, dependentTargetRank, flattenedList, depth + 1);
                }
            }
            if (node.RequiredDependents != null)
            {
                foreach (DependencyNode requiredNode in node.RequiredDependents)
                {
                    int requiredTargetRank = requiredNode.Rank + rankDelta;
                    this.FlattenHierarchy(requiredNode, requiredTargetRank, flattenedList, depth + 1);
                }
            }
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
                        foreach (BaseModule module in injectedModules)
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
                                    throw new Exception("Node '{0}' nor its parent '{1}' are dependees", node.UniqueModuleName, node.Parent.UniqueModuleName);
                                }
                                else
                                {
                                    throw new Exception("Node '{0}' is not a dependee", node.UniqueModuleName);
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
                                throw new Exception("Unable to locate the dependency of '{0}'", module.GetType().ToString());
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
                            newNode.Module.Options.FinalizeOptions(newNode);
                        }
                    }
                }
                ++currentRank;
            }
            while (currentRank < this.RankCount);
        }
    }
}