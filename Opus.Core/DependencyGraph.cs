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
            AddModule(moduleType, 0, null, baseTarget, null, -1);
        }

        public int TotalNodeCount
        {
            get
            {
                int totalNodeCount = 0;
                foreach (var collection in this.rankList)
                {
                    totalNodeCount += collection.Count;
                }
                return totalNodeCount;
            }
        }

        private DependencyNode
        AddModule(
            System.Type moduleType,
            int rank,
            DependencyNode parent,
            BaseTarget baseTarget,
            string uniqueNameSuffix,
            int uniqueIndex)
        {
            var toolset = ModuleUtilities.GetToolsetForModule(moduleType);
            var targetUsed = Target.GetInstance(baseTarget, toolset);

            var moduleTargetFilters = moduleType.GetCustomAttributes(typeof(ModuleTargetsAttribute), false) as ModuleTargetsAttribute[];
            if (moduleTargetFilters.Length > 0)
            {
                if (!TargetUtilities.MatchFilters(targetUsed, moduleTargetFilters[0]))
                {
                    Log.DebugMessage("Module '{0}' with filters '{1}' does not match target '{2}'", moduleType.ToString(), moduleTargetFilters[0].ToString(), targetUsed.ToString());
                    return null;
                }
            }

            // this is the only place a manual construction of a module is made
            var module = ModuleFactory.CreateModule(moduleType, targetUsed);

            var isNested = (-1 != uniqueIndex);
            var moduleNode = new DependencyNode(module, parent, targetUsed, uniqueIndex, isNested, uniqueNameSuffix);
            AddDependencyNodeToCollection(moduleNode, rank);

            return moduleNode;
        }

        private void AddDependencyNodeToCollection(DependencyNode moduleNode, int rank)
        {
            while (this.rankList.Count <= rank)
            {
                var newRank = new DependencyNodeCollection(this.rankList.Count);
                this.rankList.Insert(this.rankList.Count, newRank);
            }

            // TODO: why can these two statements not be combined? Is there another case where they must be separate?
            this.rankList[rank].Add(moduleNode);
            moduleNode.NodeCollection = this.rankList[rank];

            // a module can be added multiple times (for unique targets)
            if (!this.uniqueNameToNodeDictionary.ContainsKey(moduleNode.UniqueModuleName))
            {
                this.uniqueNameToNodeDictionary[moduleNode.UniqueModuleName] = new System.Collections.Generic.Dictionary<string, DependencyNode>();
            }
            if (!this.uniqueNameToNodeDictionary[moduleNode.UniqueModuleName].ContainsKey(moduleNode.Target.Key))
            {
                this.uniqueNameToNodeDictionary[moduleNode.UniqueModuleName][moduleNode.Target.Key] = moduleNode;
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public void Dump()
        {
            Log.DebugMessage("-------------GRAPH--------------");
            foreach (var nodeCollection in this.rankList)
            {
                var rank = nodeCollection.Rank;
                Log.DebugMessage("Rank {0}", rank);
                Log.DebugMessage("=======");
                int index = 0;
                foreach (var node in nodeCollection)
                {
                    Log.DebugMessage("({0}:r{1}) {2}", index++, rank, node.ToString());
                }
            }
            Log.DebugMessage("------------/GRAPH--------------");
        }

        private void CreateOptionCollections()
        {
            Log.DebugMessage("--------------------------------");
            Log.DebugMessage("Create option collections: START");
            Log.DebugMessage("--------------------------------");

            // in increasing rank, generate option collections, so that children inherit their parent options
            // if so desired
            foreach (var nodeCollection in this.rankList)
            {
                // TODO: this could be parallelized within a rank
                foreach (var node in nodeCollection)
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
                var nodeCollection = this.rankList[rank];
                foreach (var node in nodeCollection)
                {
                    node.PostCreateOptionCollection();
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

            //Log.DebugMessage("\nTop level modules only");
            //this.Dump();

            {
                var profile = new TimeProfile(ETimingProfiles.PopulateGraph);
                profile.StartProfile();

#if true
                // TODO: while deployment modules do not satisfy normal dependency checks
                // this will not place the top level modules into a mini-graph properly
                this.SortTopLevelModules();

                //this.Dump();

                var nodesWithForwardedDependencies = new DependencyNodeCollection();
                // TODO: this assumes that all dependencies are added up front
                // however, some nodes are only created and added as requirements of a child node later,
                // and these NEED to have their own dependencies to be linked up
                // TODO: almost need to detect when a new dependency is introduced in the child adding stage
                // and resolve it's dependencies
                this.AddDependents(nodesWithForwardedDependencies);
                this.PopulateChildNodes(nodesWithForwardedDependencies);
                this.ForwardOnDependencies(nodesWithForwardedDependencies);
#else
                this.AddChildAndExternalDependents();
#endif

                profile.StopProfile();
                State.TimingProfiles[(int)ETimingProfiles.PopulateGraph] = profile;
            }

            //Log.DebugMessage("\nPost normal dependencies");
            //this.Dump();

            {
                var profile = new TimeProfile(ETimingProfiles.CreateOptionCollections);
                profile.StartProfile();

                this.CreateOptionCollections();

                profile.StopProfile();
                State.TimingProfiles[(int)ETimingProfiles.CreateOptionCollections] = profile;
            }

#if false
            {
                var profile = new TimeProfile(ETimingProfiles.HandleInjectionDependents);
                profile.StartProfile();

                this.AddInjectedDependents();
                this.AddPostActions();

                profile.StopProfile();
                State.TimingProfiles[(int)ETimingProfiles.HandleInjectionDependents] = profile;
            }

            Log.DebugMessage("\nPost injected dependencies");
#endif
            this.Dump();
        }

        private DependencyNode FindNodeForTargettedModule(string moduleName, Target target)
        {
            var moduleNameExists = this.uniqueNameToNodeDictionary.ContainsKey(moduleName);
            if (!moduleNameExists)
            {
                return null;
            }

            var moduleNameForTargetExists = this.uniqueNameToNodeDictionary[moduleName].ContainsKey(target.Key);
            if (!moduleNameForTargetExists)
            {
                return null;
            }

            return this.uniqueNameToNodeDictionary[moduleName][target.Key];
        }

#if true
        private DependencyNode FindExistingNode(
            System.Type moduleType,
            string moduleName,
            Target target)
        {
            var toolset = ModuleUtilities.GetToolsetForModule(moduleType);
            var targetUsed = Target.GetInstance((BaseTarget)target, toolset);

            var node = this.FindNodeForTargettedModule(moduleName, targetUsed);
            return node;
        }

        private DependencyNodeCollection
        FindExistingOrCreateNewNodes(
            DependencyNode node,
            TypeArray dependentTypes,
            System.Collections.Generic.Dictionary<DependencyNode, int> intendedNodeRanks)
        {
            var dependentNodes = new DependencyNodeCollection();
            foreach (var depType in dependentTypes)
            {
                var depNode = this.FindExistingNode(depType, depType.FullName, node.Target);
                if (null == depNode)
                {
                    // note: this node is unparented
                    DependencyNode parent = null;
                    depNode = this.AddModule(depType, node.NodeCollection.Rank + 1, parent, (BaseTarget)node.Target, null, -1);
                }
                else
                {
                    // the node already exists, but does not satisfy all dependencies at it's current position
                    // so record where it should be placed (but don't move it yet)
                    // since each rank is considered in turn, the movement of existing nodes generally doesn't
                    // extend further than the next rank
                    var isInjectionModule = depNode.Module is IInjectModules;
                    var rankOffset = isInjectionModule ? 2 : 1;
                    var depNodeRank = depNode.NodeCollection.Rank;
                    if (intendedNodeRanks.ContainsKey(node))
                    {
                        if (depNodeRank <= intendedNodeRanks[node])
                        {
                            intendedNodeRanks[depNode] = intendedNodeRanks[node] + rankOffset;
                        }
                    }
                    else
                    {
                        var nodeRank = node.NodeCollection.Rank;
                        if (depNodeRank <= nodeRank)
                        {
                            intendedNodeRanks[depNode] = nodeRank + rankOffset;
                        }
                    }
                }
                dependentNodes.Add(depNode);
            }
            return dependentNodes;
        }

        private DependencyNode
        InjectNodeAbove(
            DependencyNode owningNode,
            DependencyNode nodePerformingInjection,
            BaseTarget baseTarget,
            int uniqueIndex,
            int insertionRank)
        {
            var injectInterface = nodePerformingInjection.Module as IInjectModules;
            var injectType = injectInterface.GetInjectedModuleType(baseTarget);
            var injectSuffix = injectInterface.GetInjectedModuleNameSuffix(baseTarget);
            var uniqueSuffix = System.String.Format("{0}.{1}", injectSuffix, uniqueIndex);
            var parentNode = injectInterface.GetInjectedParentNode(owningNode);

            var injectedNode = this.AddModule(injectType, insertionRank, parentNode, baseTarget, uniqueSuffix, uniqueIndex);
            injectedNode.AddExternalDependent(nodePerformingInjection);

            injectInterface.ModuleCreationFixup(injectedNode);

            return injectedNode;
        }

        private void
        ProcessNode(
            DependencyNode node,
            System.Collections.Generic.Dictionary<DependencyNode, int> intendedNodeRanks,
            DependencyNodeCollection nodesWithForwardedDependencies)
        {
            if (node.AreDependenciesProcessed)
            {
                return;
            }

            this.ConnectExternalDependencies(node, intendedNodeRanks, nodesWithForwardedDependencies);
            this.ConnectRequiredDependencies(node, intendedNodeRanks, nodesWithForwardedDependencies);
            node.AreDependenciesProcessed = true;
        }

        private void
        ConnectExternalDependencies(
            DependencyNode node,
            System.Collections.Generic.Dictionary<DependencyNode, int> intendedNodeRanks,
            DependencyNodeCollection nodesWithForwardedDependencies)
        {
            // find all dependencies that are in the attribute metadata
            var depTypes = ModuleUtilities.GetExternalDependents(node.Module, node.Target);

            // find any additional dependencies that are procedurally defined
            var externalDepsInterface = node.Module as IIdentifyExternalDependencies;
            if (null != externalDepsInterface)
            {
                var additionalDepTypes = externalDepsInterface.IdentifyExternalDependencies(node.Target);
                if (null != additionalDepTypes)
                {
                    if (depTypes != null)
                    {
                        depTypes.AddRangeUnique(additionalDepTypes);
                    }
                    else
                    {
                        depTypes = additionalDepTypes;
                    }
                }
            }
            if (null == depTypes)
            {
                return;
            }

            var externalDeps = this.FindExistingOrCreateNewNodes(node, depTypes, intendedNodeRanks);
            foreach (var dep in externalDeps)
            {
                node.AddExternalDependent(dep);
                if (!dep.AreDependenciesProcessed)
                {
                    this.ProcessNode(dep, intendedNodeRanks, nodesWithForwardedDependencies);
                }

                if (dep.Module is IInjectModules)
                {
                    var baseTarget = (BaseTarget)dep.Target;
                    var childIndex = 0;
                    var currentRank = intendedNodeRanks[dep];

                    // move the dependency one rank down, to make room for the injected node
                    ++intendedNodeRanks[dep];

                    var injectedNode = this.InjectNodeAbove(dep, dep, baseTarget, childIndex, currentRank);
                    this.ProcessNode(injectedNode, intendedNodeRanks, nodesWithForwardedDependencies);
                }
            }

            var hasForwardedDependencies = (node.Module is IForwardDependenciesOn);
            if (hasForwardedDependencies)
            {
                nodesWithForwardedDependencies.Add(node);
            }
        }

        private void
        ConnectRequiredDependencies(
            DependencyNode node,
            System.Collections.Generic.Dictionary<DependencyNode, int> intendedNodeRanks,
            DependencyNodeCollection nodesWithForwardedDependencies)
        {
            // find all dependencies that are in the attribute metadata
            var depTypes = ModuleUtilities.GetRequiredDependents(node.Module, node.Target);
            if (null == depTypes)
            {
                return;
            }

            var requiredNodes = this.FindExistingOrCreateNewNodes(node, depTypes, intendedNodeRanks);
            foreach (var required in requiredNodes)
            {
                node.AddRequiredDependent(required);
                if (!required.AreDependenciesProcessed)
                {
                    this.ProcessNode(required, intendedNodeRanks, nodesWithForwardedDependencies);
                }
            }
        }

        void SortTopLevelModules()
        {
            var topLevelNodesToMove = new DependencyNodeCollection();
            foreach (var node in this.rankList[0])
            {
                // find all dependencies that are in the attribute metadata
                var depTypes = ModuleUtilities.GetExternalDependents(node.Module, node.Target);

                // find any additional dependencies that are procedurally defined
                var externalDepsInterface = node.Module as IIdentifyExternalDependencies;
                if (null != externalDepsInterface)
                {
                    var additionalDepTypes = externalDepsInterface.IdentifyExternalDependencies(node.Target);
                    if (null != additionalDepTypes)
                    {
                        if (depTypes != null)
                        {
                            depTypes.AddRangeUnique(additionalDepTypes);
                        }
                        else
                        {
                            depTypes = additionalDepTypes;
                        }
                    }
                }
                // find all requirements that are in the attribute metadata
                var reqTypes = ModuleUtilities.GetRequiredDependents(node.Module, node.Target);
                if (reqTypes != null)
                {
                    if (depTypes != null)
                    {
                        depTypes.AddRangeUnique(reqTypes);
                    }
                    else
                    {
                        depTypes = reqTypes;
                    }
                }
                if (null == depTypes)
                {
                    continue;
                }

                foreach (var depType in depTypes)
                {
                    var existingDep = this.FindExistingNode(depType, depType.FullName, node.Target);
                    if (null != existingDep)
                    {
                        topLevelNodesToMove.Add(existingDep);
                    }
                }
            }

            foreach (var movedNode in topLevelNodesToMove)
            {
                this.rankList[0].Remove(movedNode);
                this.AddDependencyNodeToCollection(movedNode, 1);
            }
        }

        private void
        ForwardOnDependencies(
            DependencyNodeCollection nodesWithForwardedDependencies)
        {
            foreach (var node in nodesWithForwardedDependencies)
            {
                if (null == node.ExternalDependentFor)
                {
                    continue;
                }

                foreach (var dependee in node.ExternalDependentFor)
                {
                    foreach (var dependent in node.ExternalDependents)
                    {
                        // add forwarded dependencies to the end, as this should satisfy the link order of a single pass linker
                        dependee.AddExternalDependent(dependent);
                    }
                }
            }
        }

        private void
        AddDependents(DependencyNodeCollection nodesWithForwardedDependencies)
        {
            // DependencyNodes have a loose connection with Rank and their DependencyNodeCollection at this stage
            // There is some need for it, in order to determine whether a node satisfies all dependencies
            int currentRank = 0;
            while (currentRank < this.RankCount)
            {
                // TODO: at this stage, I kind of expect there might be empty ranks on purpose, as dependencies
                // move around... at the end of this initial stage, the empty ranks can then be squashed
                // this is another reason to not have ranks in the nodes themselves as it would require a double update
                // to squash collections
                //this.CheckForEmptyRanks();

                var intendedNodeRanks = new System.Collections.Generic.Dictionary<DependencyNode, int>();
                foreach (var node in this.rankList[currentRank])
                {
                    this.ProcessNode(node, intendedNodeRanks, nodesWithForwardedDependencies);
                }

                foreach (var node in intendedNodeRanks.Keys)
                {
                    int newRank = intendedNodeRanks[node];
                    node.NodeCollection.Remove(node);
                    this.AddDependencyNodeToCollection(node, newRank);
                }

                ++currentRank;
            }

            this.SquashEmptyNodeCollections();
        }

        private void SquashEmptyNodeCollections()
        {
            var emptyCollections = new Array<DependencyNodeCollection>();
            int currentRank = 0;
            while (currentRank < this.RankCount)
            {
                if (0 == this[currentRank].Count)
                {
                    emptyCollections.Add(this[currentRank]);
                }
                ++currentRank;
            }

            if (emptyCollections.Count > 0)
            {
                foreach (var empty in emptyCollections)
                {
                    this.rankList.Remove(empty);
                }

                // ensure rank indices are contiguous
                currentRank = 0;
                while (currentRank < this.RankCount)
                {
                    if (this.rankList[currentRank].Rank != currentRank)
                    {
                        this.rankList[currentRank].ReassignRank(currentRank);
                    }
                    ++currentRank;
                }
            }
        }

        private void DetermineIfNodeNeedsToMove(
            DependencyNode node,
            int parentIntendedRank,
            System.Collections.Generic.Dictionary<DependencyNode, int> intendedNodeRanks,
            System.Collections.Generic.Dictionary<DependencyNode, int> latestedIntendedNodeRanks)
        {
            int pastRank = node.NodeCollection.Rank;
            int intendedRank = (node.Module is IInjectModules) ? parentIntendedRank + 2 : parentIntendedRank + 1;
            if (intendedNodeRanks.ContainsKey(node))
            {
                if (intendedNodeRanks[node] > pastRank)
                {
                    pastRank = intendedNodeRanks[node];
                }
            }
            if (latestedIntendedNodeRanks.ContainsKey(node))
            {
                if (latestedIntendedNodeRanks[node] > pastRank)
                {
                    pastRank = latestedIntendedNodeRanks[node];
                }
            }
            if (pastRank < (parentIntendedRank + 1))
            {
                // dependency of that moved no longer satisfies rank order
                // always add it to the re-evaluation list, in case an earlier evaluation
                // needs to be performed again
                // this will only be a problem if there are circular dependencies
                latestedIntendedNodeRanks[node] = parentIntendedRank + 1;
            }
        }

        private System.Collections.Generic.Dictionary<DependencyNode, int> DetermineNodesToMove(
            System.Collections.Generic.Dictionary<DependencyNode, int> intendedNodeRanks)
        {
            var moreNodesToMove = new System.Collections.Generic.Dictionary<DependencyNode, int>();
            foreach (var node in intendedNodeRanks.Keys)
            {
                int currentRank = (node.NodeCollection != null) ? node.NodeCollection.Rank : -1;
                int intendedRank = intendedNodeRanks[node];

                if (node.ExternalDependents != null)
                {
                    foreach (var dependent in node.ExternalDependents)
                    {
                        this.DetermineIfNodeNeedsToMove(dependent, intendedRank, intendedNodeRanks, moreNodesToMove);
                    }
                }
                if (node.RequiredDependents != null)
                {
                    foreach (var dependent in node.RequiredDependents)
                    {
                        this.DetermineIfNodeNeedsToMove(dependent, intendedRank, intendedNodeRanks, moreNodesToMove);
                    }
                }
            }

            return (moreNodesToMove.Count > 0) ? moreNodesToMove : null;
        }

        private void
        PopulateChildNodes(
            DependencyNodeCollection nodesWithForwardedDependencies)
        {
            int currentRank = 0;
            while (currentRank < this.RankCount)
            {
                var intendedNodeRanks = new System.Collections.Generic.Dictionary<DependencyNode, int>();
                var rankCollection = this.rankList[currentRank];
                foreach (var node in rankCollection)
                {
                    if (node.AreChildrenProcessed)
                    {
                        continue;
                    }

                    var nestedDependentsInterface = node.Module as INestedDependents;
                    if (null == nestedDependentsInterface)
                    {
                        continue;
                    }

                    var nestedDependentModules = nestedDependentsInterface.GetNestedDependents(node.Target);
                    if (null == nestedDependentModules)
                    {
                        throw new Exception("Module '{0}' implements Opus.Core.INestedDependents but returns null", node.UniqueModuleName);
                    }

                    int childIndex = 0;
                    foreach (var nestedModule in nestedDependentModules)
                    {
                        var baseTarget = (BaseTarget)node.Target;
                        var nestedModuleType = nestedModule.GetType();

                        var nestedToolset = ModuleUtilities.GetToolsetForModule(nestedModuleType);
                        var nestedTargetUsed = Target.GetInstance(baseTarget, nestedToolset);

                        var isNestedNodeInjected = nestedModule is IInjectModules;
                        // for injected nodes, move the nested node down one rank, to squeeze in the injected node
                        var nestedNodeRank = isNestedNodeInjected ? currentRank + 2 : currentRank + 1;

                        var childNode = new DependencyNode(nestedModule as BaseModule, node, nestedTargetUsed, childIndex, true, null);
                        this.AddDependencyNodeToCollection(childNode, nestedNodeRank);
                        this.ProcessNode(childNode, intendedNodeRanks, nodesWithForwardedDependencies);

                        // all children inherit their collection's dependents
                        if (node.Module is IModuleCollection)
                        {
                            if (null != node.ExternalDependents)
                            {
                                foreach (var dep in node.ExternalDependents)
                                {
                                    childNode.AddExternalDependent(dep);
                                }
                            }
                        }

                        if (isNestedNodeInjected)
                        {
                            // add the injected node, in a rank above the nested node
                            var injectedNode = this.InjectNodeAbove(node, childNode, baseTarget, childIndex, nestedNodeRank - 1);
                            this.ProcessNode(injectedNode, intendedNodeRanks, nodesWithForwardedDependencies);
                        }

                        ++childIndex;
                    }

                    node.AreChildrenProcessed = true;
                }

                // if there are nodes that need moving to satisfy dependencies, recursively search
                // through their dependencies to ensure all sub-dependencies get moved too
                // this may involve dependencies being examined more than once, if they are dependents
                // of several moving nodes, and subsequently move multiple times
                // WARNING: if cyclic dependencies are introduced, this will result in an infinite loop
                if (intendedNodeRanks.Count > 0)
                {
                    var nodesToExamine = intendedNodeRanks;
                    for (;;)
                    {
                        var moreDependentsToMove = this.DetermineNodesToMove(nodesToExamine);
                        if (null == moreDependentsToMove)
                        {
                            break;
                        }

                        // merge the nodes that have to move
                        foreach (var toMove in moreDependentsToMove.Keys)
                        {
                            intendedNodeRanks[toMove] = moreDependentsToMove[toMove];
                        }

                        // now examine the latest set of nodes, so as not to repeat
                        nodesToExamine = moreDependentsToMove;
                    }
                }

                // finally move the desired nodes
                foreach (var node in intendedNodeRanks.Keys)
                {
                    int newRank = intendedNodeRanks[node];
                    node.NodeCollection.Remove(node);
                    this.AddDependencyNodeToCollection(node, newRank);
                }

                ++currentRank;
            }
        }
#else
        private DependencyNode FindOrCreateUnparentedNode(System.Type moduleType,
                                                          string moduleName,
                                                          Target target,
                                                          int currentRank,
                                                          System.Collections.Generic.Dictionary<DependencyNode, int> nodesToMove,
                                                          INestedDependents nestedDependentsInterface)
        {
            var toolset = ModuleUtilities.GetToolsetForModule(moduleType);
            var targetUsed = Target.GetInstance((BaseTarget)target, toolset);

            var node = this.FindNodeForTargettedModule(moduleName, targetUsed);
            if (null != node)
            {
                int rankToMoveTo = currentRank + 1;
                if (null != nestedDependentsInterface)
                {
                    var nestedDependentModules = nestedDependentsInterface.GetNestedDependents(target);
                    if (null == nestedDependentModules)
                    {
                        throw new Exception("Module implements Opus.Core.INestedDependents but returns null");
                    }

                    if (nestedDependentModules.Count > 0)
                    {
                        ++rankToMoveTo;
                    }
                }

                if (node.Rank < rankToMoveTo)
                {
                    nodesToMove[node] = rankToMoveTo;
                }
            }
            else
            {
                DependencyNode parentNode = null;
                node = this.AddModule(moduleType, currentRank + 1, parentNode, (BaseTarget)targetUsed);
            }

            return node;
        }

        private void CheckForEmptyRanks()
        {
            int currentRank = 0;
            while (currentRank < this.RankCount)
            {
                if (0 == this[currentRank].Count)
                {
                    throw new Exception("Internal error: Dependency node rank {0} is unexpectedly empty", currentRank);
                }
                ++currentRank;
            }
        }

        private void AddChildAndExternalDependents()
        {
            var nodesWithForwardedDependencies = new DependencyNodeCollection();
            var nodesWithPostActionsDealtWith = new DependencyNodeCollection();

            int currentRank = 0;
            while (currentRank < this.RankCount)
            {
                // Can no longer check for empty ranks now, because they can be intentionally introduced by post actions
                //this.CheckForEmptyRanks();
                var nodesToMove = new System.Collections.Generic.Dictionary<DependencyNode,int>();
                var rankNodes = this[currentRank];
                foreach (var node in rankNodes)
                {
                    // cannot add the post-action module into this rank, as you cannot modify the container being iterated
                    // but we can create 'space' to add it later by moving the current node to the next rank
                    var postActionInterface = node.Module as IPostActionModules;
                    if (postActionInterface != null &&
                        postActionInterface.GetPostActionModuleTypes((BaseTarget)node.Target) != null)
                    {
                        if (!nodesWithPostActionsDealtWith.Contains(node))
                        {
                            // remember to move the node, and don't process it on it's current rank
                            nodesToMove.Add(node, currentRank + 1);
                            nodesWithPostActionsDealtWith.Add(node);
                            continue;
                        }
                    }

                    var nestedDependentsInterface = node.Module as INestedDependents;
                    var externalDependentModuleTypes = ModuleUtilities.GetExternalDependents(node.Module, node.Target);
                    TypeArray additionalExternalDependentModuleTypes = null;
                    var identifyExternalDependencies = node.Module as IIdentifyExternalDependencies;
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
                        var hasForwardedDeps = (node.Module is IForwardDependenciesOn);
                        foreach (var dependentModuleType in externalDependentModuleTypes)
                        {
                            var newNode = this.FindOrCreateUnparentedNode(dependentModuleType, dependentModuleType.FullName, node.Target, currentRank, nodesToMove, nestedDependentsInterface);
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

                    var externalRequiredModuleTypes = ModuleUtilities.GetRequiredDependents(node.Module, node.Target);
                    if (externalRequiredModuleTypes != null)
                    {
                        foreach (var requiredModuleType in externalRequiredModuleTypes)
                        {
                            var newNode = this.FindOrCreateUnparentedNode(requiredModuleType, requiredModuleType.FullName, node.Target, currentRank, nodesToMove, nestedDependentsInterface);
                            if (null == newNode)
                            {
                                continue;
                            }

                            node.AddRequiredDependent(newNode);
                        }
                    }

                    if (null != nestedDependentsInterface)
                    {
                        var nestedDependentModules = nestedDependentsInterface.GetNestedDependents(node.Target);
                        if (null == nestedDependentModules)
                        {
                            throw new Exception("Module '{0}' implements Opus.Core.INestedDependents but returns null", node.UniqueModuleName);
                        }

                        int childIndex = 0;
                        foreach (var nestedModule in nestedDependentModules)
                        {
                            var nestedModuleType = nestedModule.GetType();
                            // TODO: the child index here might be a problem
                            var nestedModuleUniqueName = node.GetChildModuleName(nestedModuleType, childIndex);

                            var nestedToolset = ModuleUtilities.GetToolsetForModule(nestedModuleType);
                            var nestedTargetUsed = Target.GetInstance((BaseTarget)node.Target, nestedToolset);

                            var newNode = this.FindNodeForTargettedModule
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
                                newNode = new DependencyNode(nestedModule as BaseModule, node, nestedTargetUsed, childIndex, true);
                                this.AddDependencyNodeToCollection(newNode, currentRank + 1);
                            }

                            ++childIndex;
                        }
                    }
                }

                if (nodesToMove.Count > 0)
                {
                    // flatten the hierarchy of nodes, so there is one move per dependency, to the maximum rank needed
                    var flattenedList = new System.Collections.Generic.Dictionary<DependencyNode, int>();
                    foreach (var value in nodesToMove)
                    {
                        var node = value.Key;
                        int targetRank = value.Value;

                        this.FlattenHierarchy(node, targetRank, flattenedList, 0);
                    }

                    // now move
                    foreach (var value in flattenedList)
                    {
                        var node = value.Key;
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
            foreach (var nodeWith in nodesWithForwardedDependencies)
            {
                if (nodeWith.ExternalDependentFor != null)
                {
                    foreach (var forNode in nodeWith.ExternalDependentFor)
                    {
                        foreach (var forwardedDependency in nodeWith.ExternalDependents)
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
                var rankNodes = this[currentRank];
                foreach (var node in rankNodes)
                {
                    if (!node.ConsiderForBuild && (null != node.Children))
                    {
                        Log.DebugMessage("Node '{0}' is not under consideration", node.UniqueModuleName);
                        foreach (var child in node.Children)
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
                foreach (var childNode in node.Children)
                {
                    int childTargetRank = childNode.Rank + rankDelta;
                    this.FlattenHierarchy(childNode, childTargetRank, flattenedList, depth + 1);
                }
            }
            if (node.ExternalDependents != null)
            {
                foreach (var dependentNode in node.ExternalDependents)
                {
                    // if the dependent is already far enough ahead, don't move it
                    if (dependentNode.Rank >= (targetRank + rankDelta))
                    {
                        continue;
                    }

                    // cap the delta if it goes too far ahead
                    // TODO: this may not be right, as the delta might be to satisfy other dependees that have also moved?
                    if (dependentNode.Module is IInjectModules)
                    {
                        if (rankDelta > 2)
                        {
                            rankDelta = 2;
                        }
                    }
                    else if (rankDelta > 1)
                    {
                        rankDelta = 1;
                    }

                    int dependentTargetRank = dependentNode.Rank + rankDelta;
                    this.FlattenHierarchy(dependentNode, dependentTargetRank, flattenedList, depth + 1);
                }
            }
            if (node.RequiredDependents != null)
            {
                foreach (var requiredNode in node.RequiredDependents)
                {
                    // if the requirement is already far enough ahead, don't move it
                    if (requiredNode.Rank >= (targetRank + rankDelta))
                    {
                        continue;
                    }

                    int requiredTargetRank = requiredNode.Rank + rankDelta;
                    this.FlattenHierarchy(requiredNode, requiredTargetRank, flattenedList, depth + 1);
                }
            }
        }

        private void AddInjectedDependents()
        {
            int currentRank = 0;
            Array<DependencyNode> injectedNodes = new Array<DependencyNode>();
            do
            {
                var nodeCollection = this.rankList[currentRank];
                foreach (var node in nodeCollection)
                {
                    var injectModules = node.Module as IInjectModules;
                    if (null != injectModules)
                    {
                        var injectedModules = injectModules.GetInjectedModules(node.Target);
                        foreach (var module in injectedModules)
                        {
                            var externalDependencyForCollection = node.ExternalDependentFor;
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
                            foreach (var ext in externalDependencyForCollection)
                            {
                                foreach (var extDep in ext.ExternalDependents)
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
                            var newNode = new DependencyNode(module as BaseModule, sourceOfDependency, sourceOfDependency.Target, childIndex, true);
                            newNode.AddExternalDependent(node);
                            this.AddDependencyNodeToCollection(newNode, node.Rank - 1);
                            injectedNodes.Add(newNode);

                            // module inherits the options from the source of the dependency
                            newNode.CreateOptionCollection();
                        }
                    }
                }
                ++currentRank;
            }
            while (currentRank < this.RankCount);

            // now finalize the options on the nodes in reverse order to their addition
            if (injectedNodes.Count > 0)
            {
                for (int i = injectedNodes.Count - 1; i >= 0; --i)
                {
                    var node = injectedNodes[i];
                    node.PostCreateOptionCollection();
                }
            }
        }

        private void AddPostActions()
        {
            int currentRank = 0;
            var addedNodes = new Array<DependencyNode>();
            do
            {
                var nodeCollection = this.rankList[currentRank];
                foreach (var node in nodeCollection)
                {
                    var postActionModuleTypes = node.Module as IPostActionModules;
                    if (null == postActionModuleTypes)
                    {
                        continue;
                    }

                    var postActionTypes = postActionModuleTypes.GetPostActionModuleTypes((BaseTarget)node.Target);
                    if (null == postActionTypes)
                    {
                        continue;
                    }

                    var postActionRank = node.Rank - 1;
                    if (postActionRank < 0)
                    {
                        throw new Exception("Cannot have a negative rank");
                    }

                    int count = 0;
                    foreach (var moduleType in postActionTypes)
                    {
                        // TODO: would like to have a unique name for this node, to identify it as a post action
                        // but the DependencyNode unique name does not have an override
                        var newNode = this.AddModule(moduleType, postActionRank, null, (BaseTarget)node.Target);
                        newNode.AddExternalDependent(node);
                        this.AddDependencyNodeToCollection(newNode, postActionRank);
                        addedNodes.Add(newNode);

                        // module inherits the options from the source of the dependency
                        newNode.CreateOptionCollection();

                        ++count;
                    }
                }
                ++currentRank;
            }
            while (currentRank < this.RankCount);

            // now finalize the options on the nodes in reverse order to their addition
            if (addedNodes.Count > 0)
            {
                for (int i = addedNodes.Count - 1; i >= 0; --i)
                {
                    var node = addedNodes[i];
                    node.PostCreateOptionCollection();
                }
            }
        }
#endif
    }
}