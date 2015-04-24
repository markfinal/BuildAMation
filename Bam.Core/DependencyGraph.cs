#region License
// Copyright 2010-2015 Mark Final
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
#endregion // License
namespace Bam.Core
{
namespace V2
{
    using System.Linq;

    /// <summary>
    /// Representation of a dependency graph of modules
    /// </summary>
    public sealed class DependencyGraph :
        System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int, ModuleCollection>>
    {
        private System.Collections.Generic.Dictionary<int, ModuleCollection> graph = new System.Collections.Generic.Dictionary<int, ModuleCollection>();

        public ModuleCollection this[int rankIndex]
        {
            get
            {
                if (!this.graph.ContainsKey(rankIndex))
                {
                    this.graph.Add(rankIndex, new ModuleCollection());
                }
                return this.graph[rankIndex];
            }
        }

        public int this[ModuleCollection collection]
        {
            get
            {
                var pair = this.graph.Where(item => item.Value == collection);
                if (pair.Count() > 0)
                {
                    return pair.ElementAt(0).Key;
                }
                return -1;
            }
        }

        public System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int, ModuleCollection>> GetEnumerator()
        {
            int rankIndex = 0;
            while (rankIndex < this.graph.Count)
            {
                yield return new System.Collections.Generic.KeyValuePair<int, ModuleCollection>(rankIndex, this.graph[rankIndex]);
                ++rankIndex;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    /// <summary>
    /// Singleton representing the single point of reference for all build functionality
    /// </summary>
    public sealed class Graph :
        System.Collections.Generic.IEnumerable<ModuleCollection>
    {
        static Graph()
        {
            Instance = new Graph();
        }

        public static Graph Instance
        {
            get;
            private set;
        }

        private Graph()
        {
            this.Modules = new System.Collections.Generic.Dictionary<Environment, System.Collections.Generic.List<Module>>();
            this.ReferencedModules = new System.Collections.Generic.Dictionary<Environment, System.Collections.Generic.List<Module>>();
            this.TopLevelModules = new System.Collections.Generic.List<Module>();
            this.Macros = new MacroList();
            this.Macros.Add("buildroot", new TokenizedString(@"c:\dev\build", null));
            this.BuildEnvironmentInternal = null;
            this.CommonModuleType = new System.Collections.Generic.Stack<System.Type>();
            this.DependencyGraph = new DependencyGraph();
            this.Mode = null;
            this.OutputStringBuilder = new System.Text.StringBuilder();
            this.ErrorStringBuilder = new System.Text.StringBuilder();
        }

        public void AddModule(Module m)
        {
            this.Modules[this.BuildEnvironmentInternal].Add(m);
        }

        public System.Collections.Generic.Stack<System.Type> CommonModuleType
        {
            get;
            private set;
        }

        public T FindReferencedModule<T>() where T : Module, new()
        {
            var referencedModules = this.ReferencedModules[this.BuildEnvironmentInternal];
            var matches = referencedModules.Where(item => item.GetType() == typeof(T));
            if (matches.Count() > 0)
            {
                var module = matches.ElementAt(0) as T;
                return module;
            }
            this.CommonModuleType.Push(typeof(T));
            var newModule = Module.Create<T>();
            this.CommonModuleType.Pop();
            referencedModules.Add(newModule);
            return newModule;
        }

        public void CreateTopLevelModules(System.Reflection.Assembly assembly, Environment env, string ns)
        {
            this.BuildEnvironment = env;
            var allTypes = assembly.GetTypes();
            var allPackageTypes = allTypes.Where(type => type.Namespace == ns && type.IsSubclassOf(typeof(Module)));
            foreach (var moduleType in allPackageTypes)
            {
                var m = typeof(Graph).GetMethod("FindReferencedModule");
                var gm = m.MakeGenericMethod(moduleType);
                var module = gm.Invoke(Graph.Instance, null) as Module;
                this.TopLevelModules.Add(module);
            }
            this.BuildEnvironment = null;
            // remove all top level modules that have a reference count > 1
            foreach (var tlm in this.TopLevelModules.Reverse<Module>())
            {
                if (!tlm.TopLevel)
                {
                    this.TopLevelModules.Remove(tlm);
                }
            }
        }

        public void ApplySettingsPatches()
        {
            foreach (var rank in this.DependencyGraph.Reverse())
            {
                foreach (var module in rank.Value)
                {
                    module.ApplySettingsPatches();
                }
            }
        }

        public void ExpandPaths()
        {
            foreach (var rank in this.DependencyGraph.Reverse())
            {
                foreach (var module in rank.Value)
                {
                    // input paths first, since generated paths may use macros based on these
                    var setPath = module as IInputPath;
                    if (null != setPath)
                    {
                        // TODO: not necessary - there should always be an input path if the interface is implemented
                        if (null != setPath.InputPath)
                        {
                            setPath.InputPath.Parse(this.Macros, module.Macros);
                        }
                    }

                    // parse the generated paths next
                    foreach (var pair in module.GeneratedPaths)
                    {
                        var path = pair.Value;
                        if (null == path || path.Empty)
                        {
                            continue;
                        }
                        module.GeneratedPaths[pair.Key].Parse(this.Macros, module.Macros);
                    }
                }
            }
        }

        public System.Collections.Generic.List<Module> TopLevelModules
        {
            get;
            private set;
        }

        private System.Collections.Generic.Dictionary<Environment, System.Collections.Generic.List<Module>> Modules
        {
            get;
            set;
        }

        private System.Collections.Generic.Dictionary<Environment, System.Collections.Generic.List<Module>> ReferencedModules
        {
            get;
            set;
        }

        public string Mode
        {
            get;
            set;
        }

        public MacroList Macros
        {
            get;
            private set;
        }

        private Environment BuildEnvironmentInternal = null;
        public Environment BuildEnvironment
        {
            get
            {
                return this.BuildEnvironmentInternal;
            }

            private set
            {
                this.BuildEnvironmentInternal = value;
                if (null != value)
                {
                    this.Modules.Add(value, new System.Collections.Generic.List<Module>());
                    this.ReferencedModules.Add(value, new System.Collections.Generic.List<Module>());
                    // merge into the macros
                    // TODO: does it need the concept of a clean and a working macros?
                    this.Macros.Add("config", new TokenizedString(value.Configuration, null));
                }
            }
        }

        public DependencyGraph DependencyGraph
        {
            get;
            private set;
        }

        private void ApplyGroupDependenciesToChildren(
            System.Collections.ObjectModel.ReadOnlyCollection<Module> children,
            System.Collections.Generic.IEnumerable<Module> dependencies)
        {
            var nonChildDependents = new System.Collections.Generic.List<Module>();
            foreach (var c in dependencies)
            {
                if (!(c is IChildModule))
                {
                    nonChildDependents.Add(c);
                }
            }
            foreach (var d in nonChildDependents)
            {
                foreach (var c in children)
                {
                    c.DependsOn(d);
                }
            }
        }

        private void ApplyGroupRequirementsToChildren(
            System.Collections.ObjectModel.ReadOnlyCollection<Module> children,
            System.Collections.Generic.IEnumerable<Module> dependencies)
        {
            var nonChildDependents = new System.Collections.Generic.List<Module>();
            foreach (var c in dependencies)
            {
                if (!(c is IChildModule))
                {
                    nonChildDependents.Add(c);
                }
            }
            foreach (var d in nonChildDependents)
            {
                foreach (var c in children)
                {
                    c.Requires(d);
                }
            }
        }

        private void InternalArrangeDependents(Module m, int rank)
        {
            // predicate required, because eventually there will be a module without a Tool, e.g. a Tool itself
            // TODO: should a Tool on a module actually be an interface to formalize this?
            if (m.Tool != null)
            {
                m.Requires(m.Tool);
                m.Settings = m.Tool.CreateDefaultSettings(m);
                // TODO: Tool can also inject macros into the module
            }
            if ((0 == m.Dependents.Count) && (0 == m.Requirements.Count))
            {
                return;
            }
            if (m is IModuleGroup)
            {
                var children = m.Children;
                this.ApplyGroupDependenciesToChildren(children, m.Dependents);
                this.ApplyGroupRequirementsToChildren(children, m.Requirements);
            }
            var nextRank = rank + 1;
            var currentRank = this.DependencyGraph[nextRank];
            foreach (var c in m.Dependents)
            {
                currentRank.Add(c);
                this.InternalArrangeDependents(c, nextRank);
            }
            foreach (var c in m.Requirements)
            {
                currentRank.Add(c);
                this.InternalArrangeDependents(c, nextRank);
            }
        }

        public void SortDependencies()
        {
            var currentRank = this.DependencyGraph[0];
            foreach (var m in this.TopLevelModules)
            {
                currentRank.Add(m);
                this.InternalArrangeDependents(m, 0);
            }
        }

        public void Dump()
        {
            foreach (var rank in this.DependencyGraph)
            {
                var text = new System.Text.StringBuilder();
                text.AppendFormat("{1}Rank {0} modules{1}", rank.Key, System.Environment.NewLine);
                text.AppendLine(new string('-', 80));
                foreach (var m in rank.Value)
                {
                    text.AppendLine(m.ToString());
                    if (m is IInputPath)
                    {
                        text.AppendFormat("\tInput: {0}{1}", (m as IInputPath).InputPath, System.Environment.NewLine);
                    }
                    foreach (var s in m.GeneratedPaths)
                    {
                        text.AppendFormat("\t{0} : {1}{2}", s.Key, s.Value, System.Environment.NewLine);
                    }
                }
                Core.Log.DebugMessage(text.ToString());
            }
        }

        private void InternalValidateGraph(int parentRank, System.Collections.ObjectModel.ReadOnlyCollection<Module> modules)
        {
            foreach (var c in modules)
            {
                var childCollection = c.OwningRank;
                if (null == childCollection)
                {
                    throw new System.Exception("Dependency has no rank");
                }
                var found = this.DependencyGraph.Where(item => item.Value == childCollection);
                if (0 == found.Count())
                {
                    throw new System.Exception("Module collection not found in graph");
                }
                if (found.Count() > 1)
                {
                    throw new System.Exception("Module collection found more than once in graph");
                }
                var childRank = found.ElementAt(0).Key;
                if (childRank <= parentRank)
                {
                    throw new System.Exception(System.String.Format("Dependent module {0} found at a lower rank than the dependee", c));
                }
            }
        }

        public void Validate()
        {
            foreach (var rank in this.DependencyGraph)
            {
                foreach (var m in rank.Value)
                {
                    this.InternalValidateGraph(rank.Key, m.Dependents);
                    this.InternalValidateGraph(rank.Key, m.Requirements);
                }
            }
        }

        public System.Collections.Generic.IEnumerator<ModuleCollection> GetEnumerator()
        {
            foreach (var rank in this.DependencyGraph)
            {
                yield return rank.Value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public System.Text.StringBuilder OutputStringBuilder
        {
            get;
            private set;
        }

        public System.Text.StringBuilder ErrorStringBuilder
        {
            get;
            private set;
        }

        public void
        OutputDataReceived(
            object sender,
            System.Diagnostics.DataReceivedEventArgs e)
        {
            if (System.String.IsNullOrEmpty(e.Data))
            {
                return;
            }
            //System.Diagnostics.Process process = sender as System.Diagnostics.Process;
            this.OutputStringBuilder.Append(e.Data + '\n');
        }

        public void
        ErrorDataReceived(
            object sender,
            System.Diagnostics.DataReceivedEventArgs e)
        {
            if (System.String.IsNullOrEmpty(e.Data))
            {
                return;
            }
            //System.Diagnostics.Process process = sender as System.Diagnostics.Process;
            this.ErrorStringBuilder.Append(e.Data + '\n');
        }
    }
}

    public class DependencyGraph :
        System.Collections.IEnumerable
    {
        private class ConstructionData
        {
            public ConstructionData()
            {
                this.forwardedNodes = new DependencyNodeCollection();
                this.postActionNodes = new System.Collections.Generic.Dictionary<DependencyNode, DependencyNodeCollection>();
            }

            public void
            AddPostActionNode(
                DependencyNode owner,
                DependencyNode postAction)
            {
                owner.AddPostActionNode(postAction);
                if (!this.postActionNodes.ContainsKey(owner))
                {
                    this.postActionNodes.Add(owner, new DependencyNodeCollection());
                }
                this.postActionNodes[owner].Add(postAction);
            }

            public DependencyNodeCollection forwardedNodes;
            public System.Collections.Generic.Dictionary<DependencyNode, DependencyNodeCollection> postActionNodes;
        }

        public System.Collections.IEnumerator
        GetEnumerator()
        {
            return new DependencyNodeEnumerator(this);
        }

        private System.Collections.Generic.List<DependencyNodeCollection> rankList = new System.Collections.Generic.List<DependencyNodeCollection>();

        private System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, DependencyNode>> uniqueNameToNodeDictionary = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, DependencyNode>>();

        private ConstructionData constructionData = new ConstructionData();

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
            var node = this.AddModule(moduleType, 0, null, baseTarget, null, -1);
            if (null == node)
            {
                return;
            }

            // if the module has specified sibling modules, these must be added as top
            // level modules as well
            var module = node.Module;
            var siblingTypes = ModuleUtilities.GetSiblingModuleTypes(module, node.Target);
            if (null != siblingTypes)
            {
                foreach (var sibling in siblingTypes)
                {
                    this.AddTopLevelModule(sibling, baseTarget);
                }
            }
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
            // override the base target if the module attributes specify it
            var targetOverrideFilters = moduleType.GetCustomAttributes(typeof(TargetOverrideAttribute), false) as TargetOverrideAttribute[];
            if (targetOverrideFilters.Length > 0)
            {
                baseTarget = BaseTarget.GetInstance(baseTarget, platformOverride: targetOverrideFilters[0].Platform, configurationOverride: targetOverrideFilters[0].Configuration);
            }

            var toolset = ModuleUtilities.GetToolsetForModule(moduleType);
            var targetUsed = Target.GetInstance(baseTarget, toolset);

            // early out if module attributes specify a filter that is matched
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

            // this module may be a special case, which has additional modules attached to it
            // in which case the intended rank will be different
            var postActionInterface = module as IPostActionModules;
            var postActionTypes = (null != postActionInterface) ? postActionInterface.GetPostActionModuleTypes(baseTarget) : null;
            var hasPostActionTypes = (null != postActionTypes) && (postActionTypes.Count > 0);
            var injectedModuleInterface = module as IInjectModules;
            var hasInjectedModules = (null != injectedModuleInterface);
            var originalRank = rank;
            if (hasPostActionTypes || hasInjectedModules)
            {
                ++originalRank;
            }

            // TODO: need a better way to figure out whether a node is nested or not than by the unique index etc.
            var isNested = (-1 != uniqueIndex);
            var moduleNode = new DependencyNode(module, parent, targetUsed, uniqueIndex, isNested, uniqueNameSuffix);
            AddDependencyNodeToCollection(moduleNode, originalRank);

            // handle special extra associated modules
            if (hasPostActionTypes)
            {
                var postCount = 0;
                var suffix = moduleType.ToString() + ".PostAction";
                foreach (var postActionType in postActionTypes)
                {
                    var postActionNode = this.AddModule(postActionType, rank, null, baseTarget, suffix, postCount++);
                    // adding a requirement on the post-action node for all of its owner's dependencies
                    // is deferred until the end, when all dependencies have been registered
                    this.constructionData.AddPostActionNode(moduleNode, postActionNode);
                }
            }
            if (hasInjectedModules)
            {
                // TODO: the arguments are not right, as it does not know about where the connection needs to be made
                // for injected source
                int injectedUniqueIndex = 0;
                var injectedNode = this.InjectNodeAbove(null, moduleNode, baseTarget, injectedUniqueIndex, rank);
                moduleNode.AddInjectedNode(injectedNode);
            }

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

            // TODO: these statements could probably be combined, if NodeCollection.set had side-effects
            // but currently it is an automatic property
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
                Log.DebugMessage("=> Rank {0} <=", rank);
                Log.DebugMessage(new string('=', 80));
                int index = 0;
                foreach (var node in nodeCollection)
                {
                    Log.DebugMessage("r{0}({1})", rank, index++);
                    Log.DebugMessage(new string('-', 80));
                    Log.DebugMessage(node.ToString());
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

            {
                var profile = new TimeProfile(ETimingProfiles.PopulateGraph);
                profile.StartProfile();

                this.SortTopLevelModules();

                // populate the graph
                this.AddDependents();
                this.PopulateChildNodes();
                // now perform fixup across the whole graph
                this.ForwardOnDependencies();
                this.AddRequiredPostActionDependencies();

                // perform validation
                this.ValidateAllDependents("After graph construction");

                // tidy up
                this.SquashEmptyNodeCollections();
                this.constructionData = null; // no longer need this

                profile.StopProfile();
                State.TimingProfiles[(int)ETimingProfiles.PopulateGraph] = profile;
            }

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

        private void
        MoveDependentIfRequired(
            DependencyNode node,
            DependencyNode dependent,
            System.Collections.Generic.Dictionary<DependencyNode, int> nodeRankOffsets)
        {
            // the node already exists, but does not satisfy all dependencies at it's current position
            // so record where it should be placed (but don't move it yet)
            // since each rank is considered in turn, the movement of existing nodes generally doesn't
            // extend further than the next rank
            var isInjectionModule = dependent.Module is IInjectModules;
            var rankOffset = isInjectionModule ? 2 : 1;
            var depNodeRank = dependent.NodeCollection.Rank;
            // if the dependency is currently ranked less than the parent node, the parent
            // node needs to move to a higher rank
            if (nodeRankOffsets.ContainsKey(node))
            {
                var difference = depNodeRank - (node.NodeCollection.Rank + nodeRankOffsets[node]);
                if (difference <= 0)
                {
                    nodeRankOffsets[dependent] = nodeRankOffsets[node] + rankOffset - difference;
                }
            }
            else
            {
                var difference = depNodeRank - node.NodeCollection.Rank;
                if (difference <= 0)
                {
                    nodeRankOffsets[dependent] = rankOffset - difference;
                }
            }
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
                    this.MoveDependentIfRequired(node, depNode, nodeRankOffsets);
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
            // if this module is going to be injected as a child of some node associated with the owner node,
            // we need a hook to figure out that relationship
            var parentNode = (owningNode != null) ? injectInterface.GetInjectedParentNode(owningNode) : null;

            var injectedNode = this.AddModule(injectType, insertionRank, parentNode, baseTarget, uniqueSuffix, uniqueIndex);

            // a loose ordering is only needed - no hard dependency
            injectedNode.AddRequiredDependent(nodePerformingInjection);

            injectInterface.ModuleCreationFixup(injectedNode, nodePerformingInjection);

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
            System.Collections.Generic.Dictionary<DependencyNode, int> nodeRankOffsets)
        {
            if (node.AreDependenciesProcessed)
            {
                if (node.ExternalDependents != null)
                {
                    foreach (var dep in node.ExternalDependents)
                    {
                        this.MoveDependentIfRequired(node, dep, nodeRankOffsets);
                    }
                }
                if (node.RequiredDependents != null)
                {
                    foreach (var dep in node.RequiredDependents)
                    {
                        this.MoveDependentIfRequired(node, dep, nodeRankOffsets);
                    }
                }
                return;
            }

            this.ConnectExternalDependencies(node, nodeRankOffsets);
            this.ConnectRequiredDependencies(node, nodeRankOffsets);

            // TODO: should this only be set if node is not in the moved list?
            node.AreDependenciesProcessed = true;
        }

        private void
        ConnectExternalDependencies(
            DependencyNode node,
            System.Collections.Generic.Dictionary<DependencyNode, int> nodeRankOffsets)
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
#if false
                if (null != dep.PostActionNodes)
                {
                    // post action nodes on the dependent are also a dependency
                    foreach (var postAction in dep.PostActionNodes)
                    {
                        node.AddRequiredDependent(postAction);
                    }
                }
                //this.ProcessNode(dep, nodeRankOffsets);

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
                        // TODO: this looks wrong - it should be an offset
                        nodeRankOffsets[dep] = dep.NodeCollection.Rank;
                    }

                    // move the dependency one rank down, to make room for the injected node
                    ++nodeRankOffsets[dep];

                    var injectedNode = this.InjectNodeAbove(dep, dep, baseTarget, childIndex, currentRank);
                    // this ProcessNode appears to be required
                    this.ProcessNode(injectedNode, nodeRankOffsets);
                }
#endif
            }

            var hasForwardedDependencies = (node.Module is IForwardDependenciesOn);
            if (hasForwardedDependencies)
            {
                this.constructionData.forwardedNodes.Add(node);
            }
        }

        private void
        ConnectRequiredDependencies(
            DependencyNode node,
            System.Collections.Generic.Dictionary<DependencyNode, int> nodeRankOffsets)
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
#if false
                if (!required.AreDependenciesProcessed)
                {
                    this.ProcessNode(required, nodeRankOffsets);
                }
#endif
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
        ForwardOnDependencies()
        {
            foreach (var node in this.constructionData.forwardedNodes)
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

            if (State.Pedantic)
            {
                this.ValidateAllDependents("After forwarding on dependencies");
            }
        }

        private void
        MoveNode(
            DependencyNode node,
            int rankToMoveTo)
        {
            node.NodeCollection.Remove(node);
            this.AddDependencyNodeToCollection(node, rankToMoveTo);
        }

        private void
        AddRequiredPostActionDependencies()
        {
            foreach (var nodeWithPostAction in this.constructionData.postActionNodes.Keys)
            {
                var postActionNodes = this.constructionData.postActionNodes[nodeWithPostAction];
                foreach (var postActionNode in postActionNodes)
                {
                    if (null != nodeWithPostAction.ExternalDependentFor)
                    {
                        foreach (var dependee in nodeWithPostAction.ExternalDependentFor)
                        {
                            // don't depend on any of the post actions added, including itself
                            if (postActionNodes.Contains(dependee))
                            {
                                continue;
                            }
                            dependee.AddRequiredDependent(postActionNode);

                            // ensure that all rank orders are satisfied
                            if (dependee.NodeCollection.Rank >= postActionNode.NodeCollection.Rank)
                            {
                                var intendedRank = dependee.NodeCollection.Rank + 1;
                                this.ShiftUpRanksFrom(intendedRank);
                                this.MoveNode(postActionNode, intendedRank);
                            }
                        }
                    }
                    if (null != nodeWithPostAction.RequiredDependentFor)
                    {
                        foreach (var dependee in nodeWithPostAction.RequiredDependentFor)
                        {
                            // don't depend on any of the post actions added, including itself
                            if (postActionNodes.Contains(dependee))
                            {
                                continue;
                            }
                            dependee.AddRequiredDependent(postActionNode);

                            // ensure that all rank orders are satisfied
                            if (dependee.NodeCollection.Rank >= postActionNode.NodeCollection.Rank)
                            {
                                var intendedRank = dependee.NodeCollection.Rank + 1;
                                this.ShiftUpRanksFrom(intendedRank);
                                this.MoveNode(postActionNode, intendedRank);
                            }
                        }
                    }
                }
            }
        }

        private void
        AddDependents()
        {
            // DependencyNodes have a loose connection with Rank and their DependencyNodeCollection at this stage
            // There is some need for it, in order to determine whether a node satisfies all dependencies
            int currentRank = 0;
            while (currentRank < this.RankCount)
            {
                var nodeRankOffsets = new System.Collections.Generic.Dictionary<DependencyNode, int>();
                var rankNodeCollection = this.rankList[currentRank];
                foreach (var node in rankNodeCollection)
                {
                    if (nodeRankOffsets.ContainsKey(node))
                    {
                        // defer processing any nodes that are due to be moved out of this rank
                        continue;
                    }
                    // this appears to be required
                    this.ProcessNode(node, nodeRankOffsets);
                }

                foreach (var node in nodeRankOffsets.Keys)
                {
                    var offset = nodeRankOffsets[node];
                    var newRank = node.NodeCollection.Rank + offset;
                    this.MoveNode(node, newRank);

                    // if you move a node, also move it's post action nodes at the same rate
                    if (null != node.PostActionNodes)
                    {
                        foreach (var postAction in node.PostActionNodes)
                        {
                            newRank = postAction.NodeCollection.Rank + offset;
                            this.MoveNode(postAction, newRank);
                        }
                    }
                }

                ++currentRank;
            }

            if (State.Pedantic)
            {
                this.ValidateAllDependents("After adding dependents");
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
            System.Collections.Generic.Dictionary<DependencyNode, int> updatedNodeRankOffsets,
            int rankOffset)
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
            var thisIntendedRank = parentIntendedRank + rankOffset;
            if (pastRank < thisIntendedRank)
            {
                // dependency of that moved no longer satisfies rank order
                // always add it to the re-evaluation list, in case an earlier evaluation
                // needs to be performed again
                // this will only be a problem if there are circular dependencies
                var delta = thisIntendedRank - pastRank;
                if (!inUpdatedOffsets)
                {
                    updatedNodeRankOffsets[node] = 0;
                }
                updatedNodeRankOffsets[node] += delta;
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
                        // to avoid infinite loops, since injected nodes are circularly coupled with their owner
                        if (child.Module is IInjectModules)
                        {
                            continue;
                        }
                        if (child.PostActionNodes != null)
                        {
                            // to avoid infinite loops, don't re-add a post-action's dependents, which will have added it earlier
                            if (child.PostActionNodes.Contains(node))
                            {
                                continue;
                            }
                        }
                        this.DetermineIfNodeNeedsToMove(child, intendedRank, nodeRankOffsets, updatedNodeRankOffsets, 1);
                    }
                }
                if (node.ExternalDependents != null)
                {
                    foreach (var dependent in node.ExternalDependents)
                    {
                        // to avoid infinite loops, since injected nodes are circularly coupled with their owner
                        if (dependent.Module is IInjectModules)
                        {
                            continue;
                        }
                        if (dependent.PostActionNodes != null)
                        {
                            // to avoid infinite loops, don't re-add a post-action's dependents, which will have added it earlier
                            if (dependent.PostActionNodes.Contains(node))
                            {
                                continue;
                            }
                        }
                        this.DetermineIfNodeNeedsToMove(dependent, intendedRank, nodeRankOffsets, updatedNodeRankOffsets, 1);
                    }
                }
                if (node.RequiredDependents != null)
                {
                    foreach (var dependent in node.RequiredDependents)
                    {
                        // to avoid infinite loops, since injected nodes are circularly coupled with their owner
                        if (dependent.Module is IInjectModules)
                        {
                            continue;
                        }
                        if (dependent.PostActionNodes != null)
                        {
                            // to avoid infinite loops, don't re-add a post-action's dependents, which will have added it earlier
                            if (dependent.PostActionNodes.Contains(node))
                            {
                                continue;
                            }
                        }
                        this.DetermineIfNodeNeedsToMove(dependent, intendedRank, nodeRankOffsets, updatedNodeRankOffsets, 1);
                    }
                }
                if (node.InjectedNodes != null)
                {
                    foreach (var dependent in node.InjectedNodes)
                    {
                        this.DetermineIfNodeNeedsToMove(dependent, intendedRank, nodeRankOffsets, updatedNodeRankOffsets, -1);
                    }
                }
                if (node.PostActionNodes != null)
                {
                    foreach (var dependent in node.PostActionNodes)
                    {
                        this.DetermineIfNodeNeedsToMove(dependent, intendedRank, nodeRankOffsets, updatedNodeRankOffsets, -1);
                    }
                }
            }

            return (updatedNodeRankOffsets.Count > 0) ? updatedNodeRankOffsets : null;
        }

        private void
        ValidateDependentsInRank(
            DependencyNodeCollection nodeCollection,
            string callingMessage)
        {
            // This is quite expensive, but always better to find out these problems sooner rather than later
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
                            this.Dump();
                            var message = new System.Text.StringBuilder();
                            message.AppendFormat("{0}, child '{1}' of '{2}' is at an insufficient rank\n", callingMessage, child.UniqueModuleName, node.UniqueModuleName);
                            message.AppendFormat("\t{0}:\trank {1}\n", node.UniqueModuleName, nodeRank);
                            message.AppendFormat("\t{0}:\trank {1}\n", child.UniqueModuleName, childRank);
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
                            this.Dump();
                            var message = new System.Text.StringBuilder();
                            message.AppendFormat("{0}, dependency '{1}' of '{2}' is at an insufficient rank\n", callingMessage, dep.UniqueModuleName, node.UniqueModuleName);
                            message.AppendFormat("\t{0}:\trank {1}\n", node.UniqueModuleName, nodeRank);
                            message.AppendFormat("\t{0}:\trank {1}\n", dep.UniqueModuleName, depRank);
                            throw new Exception(message.ToString());
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
                            this.Dump();
                            var message = new System.Text.StringBuilder();
                            message.AppendFormat("{0}, required dependency '{1}' of '{2}' is at an insufficient rank\n", callingMessage, dep.UniqueModuleName, node.UniqueModuleName);
                            message.AppendFormat("\t{0}:\trank {1}\n", node.UniqueModuleName, nodeRank);
                            message.AppendFormat("\t{0}:\trank {1}\n", dep.UniqueModuleName, depRank);
                            throw new Exception(message.ToString());
                        }
                    }
                }
            }
        }

        private void
        ValidateAllDependents(
            string callingMessage)
        {
            foreach (var rank in this.rankList)
            {
                this.ValidateDependentsInRank(rank, callingMessage);
            }
        }

        private void
        PopulateChildNodes()
        {
            int currentRank = 0;
            while (currentRank < this.RankCount)
            {
                var nodeRankOffsets = new System.Collections.Generic.Dictionary<DependencyNode, int>();
                var rankCollection = this.rankList[currentRank];
                foreach (var node in rankCollection)
                {
                    // try processing again, in case new nodes have been added through connecting dependencies of children
                    // this must happen before the early out on children processed, or it results in
                    // nodes not being in the correct rank
                    this.ProcessNode(node, nodeRankOffsets);

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
                        throw new Exception("Module '{0}' implements Bam.Core.INestedDependents but returns null", node.UniqueModuleName);
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
                        this.ProcessNode(childNode, nodeRankOffsets);

                        // all children inherit their collection's dependents
                        // TODO: this should only be for those modules that are not injected
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
                            this.ProcessNode(injectedNode, nodeRankOffsets);
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
                    this.MoveNode(node, newRank);
                }

                if (State.Pedantic)
                {
                    this.ValidateDependentsInRank(rankCollection, System.String.Format("After adding children for rank {0}", currentRank));
                }

                ++currentRank;
            }

            if (State.Pedantic)
            {
                this.ValidateAllDependents("After adding children");
            }
        }
    }
}
