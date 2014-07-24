// <copyright file="DependencyGraph.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public class DependencyGraph :
        System.Collections.IEnumerable
    {
        public System.Collections.IEnumerator
        GetEnumerator()
        {
            return new DependencyNodeEnumerator(this);
        }

        private System.Collections.Generic.List<DependencyNodeCollection> rankList = new System.Collections.Generic.List<DependencyNodeCollection>();

        private System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, DependencyNode>> uniqueNameToNodeDictionary = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, DependencyNode>>();

        public
        DependencyGraph()
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

        public void
        AddTopLevelModule(
            System.Type moduleType,
            BaseTarget baseTarget)
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

        private void
        AddDependencyNodeToCollection(
            DependencyNode moduleNode,
            int rank)
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
        public void
        Dump()
        {
            Log.DebugMessage("-------------GRAPH--------------");
            foreach (var nodeCollection in this.rankList)
            {
                var rank = nodeCollection.Rank;
                Log.DebugMessage(new string('=', 80));
                Log.DebugMessage("= Rank {0}", rank);
                Log.DebugMessage(new string('=', 80));
                int index = 0;
                foreach (var node in nodeCollection)
                {
                    Log.DebugMessage("({0}:r{1}) {2}", index++, rank, node.ToString());
                }
            }
            Log.DebugMessage("------------/GRAPH--------------");
        }

        private void
        CreateOptionCollections()
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

        public void
        PopulateGraph()
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

                // TODO: while deployment modules do not satisfy normal dependency checks
                // this will not place the top level modules into a mini-graph properly
                this.SortTopLevelModules();

                var nodesWithForwardedDependencies = new DependencyNodeCollection();
                // TODO: this assumes that all dependencies are added up front
                // however, some nodes are only created and added as requirements of a child node later,
                // and these NEED to have their own dependencies to be linked up
                // TODO: almost need to detect when a new dependency is introduced in the child adding stage
                // and resolve it's dependencies
                this.AddDependents(nodesWithForwardedDependencies);
                this.PopulateChildNodes(nodesWithForwardedDependencies);
                this.ForwardOnDependencies(nodesWithForwardedDependencies);
                this.SquashEmptyNodeCollections();

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

            this.Dump();
        }

        private DependencyNode
        FindNodeForTargettedModule(
            string moduleName,
            Target target)
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

        private DependencyNode
        FindExistingNode(
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
            System.Collections.Generic.Dictionary<DependencyNode, int> nodeRankOffsets)
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
                    if (null == depNode)
                    {
                        // this can happen, say, if the dependent is filtered
                        continue;
                    }
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
                    // if the dependency is currently ranked less than the parent node, the parent
                    // node needs to move to a higher rank
                    if (nodeRankOffsets.ContainsKey(node))
                    {
                        if (depNodeRank <= node.NodeCollection.Rank + nodeRankOffsets[node])
                        {
                            nodeRankOffsets[depNode] = nodeRankOffsets[node] + rankOffset;
                        }
                    }
                    else
                    {
                        var nodeRank = node.NodeCollection.Rank;
                        if (depNodeRank <= nodeRank)
                        {
                            nodeRankOffsets[depNode] = rankOffset;
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
        ShiftUpRanksFrom(
            int minimumRank)
        {
            // move all subsequent ranks up by one
            for (var movingRankIndex = this.RankCount - 1; movingRankIndex >= minimumRank; --movingRankIndex)
            {
                var rankCollection = this.rankList[movingRankIndex];
                var newRank = movingRankIndex + 1;
                rankCollection.ReassignRank(newRank);
            }
            this.rankList.Insert(minimumRank, new DependencyNodeCollection(minimumRank));
        }

        private void
        ProcessNode(
            DependencyNode node,
            System.Collections.Generic.Dictionary<DependencyNode, int> nodeRankOffsets,
            DependencyNodeCollection nodesWithForwardedDependencies)
        {
            if (node.AreDependenciesProcessed)
            {
                return;
            }

            this.ConnectExternalDependencies(node, nodeRankOffsets, nodesWithForwardedDependencies);
            this.ConnectRequiredDependencies(node, nodeRankOffsets, nodesWithForwardedDependencies);

            var postActionInterface = node.Module as IPostActionModules;
            if (null != postActionInterface)
            {
                var postActionModuleTypes = postActionInterface.GetPostActionModuleTypes((BaseTarget)node.Target);
                if (null != postActionModuleTypes)
                {
                    var nodeRank = node.NodeCollection.Rank;
                    if (nodeRankOffsets.ContainsKey(node))
                    {
                        nodeRank += nodeRankOffsets[node];
                    }

                    this.ShiftUpRanksFrom(nodeRank);

                    int postCount = 0;
                    foreach (var postModuleType in postActionModuleTypes)
                    {
                        var postNode = this.AddModule(postModuleType, nodeRank, null, (BaseTarget)node.Target, node.ModuleName + ".PostAction", postCount);

                        // ensure that those nodes with a dependent on the node with the post-action, also have
                        // a dependency on the post-action
                        if (null != node.ExternalDependentFor)
                        {
                            foreach (var dependee in node.ExternalDependentFor)
                            {
                                dependee.AddRequiredDependent(postNode);
                            }
                        }
                        if (null != node.RequiredDependentFor)
                        {
                            foreach (var dependee in node.RequiredDependentFor)
                            {
                                dependee.AddRequiredDependent(postNode);
                            }
                        }

                        node.AddPostActionNode(postNode);
                        this.ProcessNode(postNode, nodeRankOffsets, nodesWithForwardedDependencies);
                        ++postCount;
                    }
                }
            }

            node.AreDependenciesProcessed = true;
        }

        private void
        ConnectExternalDependencies(
            DependencyNode node,
            System.Collections.Generic.Dictionary<DependencyNode, int> nodeRankOffsets,
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

            var externalDeps = this.FindExistingOrCreateNewNodes(node, depTypes, nodeRankOffsets);
            foreach (var dep in externalDeps)
            {
                node.AddExternalDependent(dep);
                if (null != dep.PostActionNodes)
                {
                    // post action nodes on the dependent are also a dependency
                    foreach (var postAction in dep.PostActionNodes)
                    {
                        node.AddRequiredDependent(postAction);
                    }
                }
                this.ProcessNode(dep, nodeRankOffsets, nodesWithForwardedDependencies);

                if (dep.Module is IInjectModules)
                {
                    var baseTarget = (BaseTarget)dep.Target;
                    var childIndex = 0;
                    var currentRank = dep.NodeCollection.Rank;
                    if (nodeRankOffsets.ContainsKey(dep))
                    {
                        currentRank += nodeRankOffsets[dep];
                    }
                    else
                    {
                        nodeRankOffsets[dep] = dep.NodeCollection.Rank;
                    }

                    // move the dependency one rank down, to make room for the injected node
                    ++nodeRankOffsets[dep];

                    var injectedNode = this.InjectNodeAbove(dep, dep, baseTarget, childIndex, currentRank);
                    this.ProcessNode(injectedNode, nodeRankOffsets, nodesWithForwardedDependencies);
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
            System.Collections.Generic.Dictionary<DependencyNode, int> nodeRankOffsets,
            DependencyNodeCollection nodesWithForwardedDependencies)
        {
            // find all dependencies that are in the attribute metadata
            var depTypes = ModuleUtilities.GetRequiredDependents(node.Module, node.Target);
            if (null == depTypes)
            {
                return;
            }

            var requiredNodes = this.FindExistingOrCreateNewNodes(node, depTypes, nodeRankOffsets);
            foreach (var required in requiredNodes)
            {
                node.AddRequiredDependent(required);
                if (!required.AreDependenciesProcessed)
                {
                    this.ProcessNode(required, nodeRankOffsets, nodesWithForwardedDependencies);
                }
            }
        }

        private void
        SortTopLevelModules()
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

                var nodeRankOffsets = new System.Collections.Generic.Dictionary<DependencyNode, int>();
                var rankNodeCollection = this.rankList[currentRank];
                foreach (var node in rankNodeCollection)
                {
                    this.ProcessNode(node, nodeRankOffsets, nodesWithForwardedDependencies);
                }

                foreach (var node in nodeRankOffsets.Keys)
                {
                    var newRank = node.NodeCollection.Rank + nodeRankOffsets[node];
                    node.NodeCollection.Remove(node);
                    this.AddDependencyNodeToCollection(node, newRank);
                }

                ++currentRank;
            }
        }

        private void
        SquashEmptyNodeCollections()
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

        private void
        DetermineIfNodeNeedsToMove(
            DependencyNode node,
            int parentIntendedRank,
            System.Collections.Generic.Dictionary<DependencyNode, int> nodeRankOffsets,
            System.Collections.Generic.Dictionary<DependencyNode, int> updatedNodeRankOffsets)
        {
            var pastRank = node.NodeCollection.Rank;
            if (nodeRankOffsets.ContainsKey(node))
            {
                if (nodeRankOffsets[node] > 0)
                {
                    pastRank += nodeRankOffsets[node];
                }
            }
            var inUpdatedOffsets = updatedNodeRankOffsets.ContainsKey(node);
            if (inUpdatedOffsets)
            {
                if (updatedNodeRankOffsets[node] > 0)
                {
                    pastRank += updatedNodeRankOffsets[node];
                }
            }
            if (pastRank < (parentIntendedRank + 1))
            {
                // dependency of that moved no longer satisfies rank order
                // always add it to the re-evaluation list, in case an earlier evaluation
                // needs to be performed again
                // this will only be a problem if there are circular dependencies
                var delta = parentIntendedRank - pastRank + 1;
                if (inUpdatedOffsets)
                {
                    updatedNodeRankOffsets[node] += delta;
                }
                else
                {
                    updatedNodeRankOffsets[node] = delta;
                }
            }
        }

        private System.Collections.Generic.Dictionary<DependencyNode, int>
        DetermineNodesToMove(
            System.Collections.Generic.Dictionary<DependencyNode, int> nodeRankOffsets)
        {
            var updatedNodeRankOffsets = new System.Collections.Generic.Dictionary<DependencyNode, int>();
            foreach (var node in nodeRankOffsets.Keys)
            {
                var intendedRank = node.NodeCollection.Rank + nodeRankOffsets[node];

                if (node.Children != null)
                {
                    foreach (var child in node.Children)
                    {
                        this.DetermineIfNodeNeedsToMove(child, intendedRank, nodeRankOffsets, updatedNodeRankOffsets);
                    }
                }
                if (node.ExternalDependents != null)
                {
                    foreach (var dependent in node.ExternalDependents)
                    {
                        this.DetermineIfNodeNeedsToMove(dependent, intendedRank, nodeRankOffsets, updatedNodeRankOffsets);
                    }
                }
                if (node.RequiredDependents != null)
                {
                    foreach (var dependent in node.RequiredDependents)
                    {
                        this.DetermineIfNodeNeedsToMove(dependent, intendedRank, nodeRankOffsets, updatedNodeRankOffsets);
                    }
                }
            }

            return (updatedNodeRankOffsets.Count > 0) ? updatedNodeRankOffsets : null;
        }

        private void
        ValidateDependentRanks(
            DependencyNodeCollection nodeCollection)
        {
            // TODO: do we want to do this in all types of build?
            foreach (var node in nodeCollection)
            {
                var nodeRank = node.NodeCollection.Rank;
                if (null != node.Children)
                {
                    foreach (var child in node.Children)
                    {
                        var childRank = child.NodeCollection.Rank;
                        if (childRank <= nodeRank)
                        {
                            throw new Exception("Child '{0}' of '{1}' is at an insufficient rank", child.UniqueModuleName, node.UniqueModuleName);
                        }
                    }
                }
                if (null != node.ExternalDependents)
                {
                    foreach (var dep in node.ExternalDependents)
                    {
                        var depRank = dep.NodeCollection.Rank;
                        if (depRank <= nodeRank)
                        {
                            throw new Exception("Dependency '{0}' of '{1}' is at an insufficient rank", dep.UniqueModuleName, node.UniqueModuleName);
                        }
                    }
                }
                if (null != node.RequiredDependents)
                {
                    foreach (var dep in node.RequiredDependents)
                    {
                        var depRank = dep.NodeCollection.Rank;
                        if (depRank <= nodeRank)
                        {
                            throw new Exception("Required dependency '{0}' of '{1}' is at an insufficient rank", dep.UniqueModuleName, node.UniqueModuleName);
                        }
                    }
                }
            }
        }

        private void
        PopulateChildNodes(
            DependencyNodeCollection nodesWithForwardedDependencies)
        {
            int currentRank = 0;
            while (currentRank < this.RankCount)
            {
                var nodeRankOffsets = new System.Collections.Generic.Dictionary<DependencyNode, int>();
                var rankCollection = this.rankList[currentRank];
                foreach (var node in rankCollection)
                {
                    // always consider the current node for potential movement, so that it's dependencies are analysed
                    if (!nodeRankOffsets.ContainsKey(node))
                    {
                        nodeRankOffsets[node] = 0;
                    }

                    if (node.AreChildrenProcessed)
                    {
                        continue;
                    }

                    var nestedDependentsInterface = node.Module as INestedDependents;
                    if (null == nestedDependentsInterface)
                    {
                        node.AreChildrenProcessed = true;
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
                        this.ProcessNode(childNode, nodeRankOffsets, nodesWithForwardedDependencies);

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
                            this.ProcessNode(injectedNode, nodeRankOffsets, nodesWithForwardedDependencies);
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
                if (nodeRankOffsets.Count > 0)
                {
                    var nodesToExamine = nodeRankOffsets;
                    for (;;)
                    {
                        var moreDependentsToMove = this.DetermineNodesToMove(nodesToExamine);
                        if (null == moreDependentsToMove)
                        {
                            break;
                        }

                        // merge the nodes that have to move, but only if they are increasing
                        foreach (var toMove in moreDependentsToMove.Keys)
                        {
                            var newRank = moreDependentsToMove[toMove];
                            if (nodeRankOffsets.ContainsKey(toMove))
                            {
                                if (newRank > nodeRankOffsets[toMove])
                                {
                                    nodeRankOffsets[toMove] = newRank;
                                }
                            }
                            else
                            {
                                nodeRankOffsets[toMove] = moreDependentsToMove[toMove];
                            }
                        }

                        // now examine the latest set of nodes, so as not to repeat
                        nodesToExamine = moreDependentsToMove;
                    }
                }

                // finally move the desired nodes
                foreach (var node in nodeRankOffsets.Keys)
                {
                    var delta = nodeRankOffsets[node];
                    if (0 == delta)
                    {
                        continue;
                    }
                    var newRank = node.NodeCollection.Rank + delta;
                    node.NodeCollection.Remove(node);
                    this.AddDependencyNodeToCollection(node, newRank);
                }

                // validate that ranks obey dependency rules
                this.ValidateDependentRanks(rankCollection);

                ++currentRank;
            }
        }
    }
}
