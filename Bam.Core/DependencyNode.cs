// <copyright file="DependencyNode.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public sealed class DependencyNode
    {
        public bool
        AreDependenciesProcessed
        {
            get;
            set;
        }

        public bool
        AreChildrenProcessed
        {
            get;
            set;
        }

        public override string
        ToString()
        {
            var output = new System.Text.StringBuilder();
            output.AppendFormat("'{0}' from '{1}'", this.UniqueModuleName, this.ModuleName);
            if (this.Parent != null)
            {
                output.AppendFormat(" child of '{0}'", this.Parent.UniqueModuleName);
            }
            output.AppendFormat(" Type '{0}' (from '{1}')", this.Module.GetType().ToString(), this.Module.GetType().BaseType.ToString());
            output.AppendFormat(" in '{0}'", this.Target.Key);
            if (null != this.Children)
            {
                output.Append("\nChildren:\n");
                output.Append("\tRank\tName\n");
                foreach (var node in this.Children)
                {
                    output.AppendFormat("\t{0}\t{1}\n", node.NodeCollection.Rank, node.UniqueModuleName);
                }
            }
            if (null != this.ExternalDependents)
            {
                output.Append("\nDependents:\n");
                output.Append("\tRank\tName\n");
                foreach (var node in this.ExternalDependents)
                {
                    output.AppendFormat("\t{0}\t{1}\n", node.NodeCollection.Rank, node.UniqueModuleName);
                }
            }
            if (null != this.RequiredDependents)
            {
                output.Append("\nRequired dependents:\n");
                output.Append("\tRank\tName\n");
                foreach (var node in this.RequiredDependents)
                {
                    output.AppendFormat("\t{0}\t{1}\n", node.NodeCollection.Rank, node.UniqueModuleName);
                }
            }

            return output.ToString();
        }

        private EBuildState buildState;

        public delegate void
        CompleteEventHandler(
            DependencyNode node);

        public event CompleteEventHandler CompletedEvent;

        public TypeArray LocalUpdatesAdded
        {
            get;
            set;
        }

        public TypeArray ExportedUpdatesAdded
        {
            get;
            set;
        }

        private void
        AddChild(
            DependencyNode childNode)
        {
            if (null == this.Children)
            {
                this.Children = new DependencyNodeCollection();
            }

            this.Children.Add(childNode);
        }

        /// <summary>
        /// Creating option collections are called on each node, iterated by the graph from top to bottom.
        /// </summary>
        public void
        CreateOptionCollection()
        {
            var toolset = this.Target.Toolset;

            var moduleType = this.Module.GetType();
            Log.DebugMessage("Constructing option collection for node '{0}'", this.UniqueModuleName);

            // requires inheritence because the attribute is usually on the base class
            var moduleTools = moduleType.GetCustomAttributes(typeof(ModuleToolAssignmentAttribute), true);
            if (null == moduleTools || 0 == moduleTools.Length)
            {
                throw new Exception("Module type '{0}' (base type '{1}') does not have any assigned tools", moduleType.ToString(), moduleType.BaseType.ToString());
            }

            if (moduleTools.Length > 1)
            {
                throw new Exception("There is more than one tool associated with this module '{0}'", moduleType.ToString());
            }

            var toolType = (moduleTools[0] as ModuleToolAssignmentAttribute).ToolType;
            if (null == toolset)
            {
                Log.DebugMessage("\tNo toolset for target '{0}' and tool '{1}' for module '{2}'", Target.ToString(), (null != toolType) ? toolType.ToString() : "undefined", moduleType.ToString());
                return;
                //throw new Exception("No toolset for target '{0}' and tool '{1}' for module '{2}'", Target.ToString(), (null != toolType) ? toolType.ToString() : "undefined", moduleType.ToString());
            }

            Log.DebugMessage("\tUsing toolset '{0}' for tool '{1}' for module '{2}'", toolset.ToString(), (null != toolType) ? toolType.ToString() : "undefined", moduleType.ToString());
            if (!toolType.IsInterface)
            {
                throw new Exception("Tool '{0}' is NOT an interface", toolType.ToString());
            }

            var optionCollectionType = toolset.ToolOptionType(toolType);
            if (null == optionCollectionType)
            {
                Log.DebugMessage("\tNo option collection type for tool '{0}' from toolset '{1}'", toolType.ToString(), toolset.ToString());
                return;
                //throw new Exception("No option collection type for tool '{0}' from toolset '{1}'", toolType.ToString(), toolset.ToString());
            }

            var localAndExportTypes = toolType.GetCustomAttributes(typeof(LocalAndExportTypesAttribute), false);

            if (localAndExportTypes.Length == 0)
            {
                throw new Exception("Missing local and export types attribute on tool type '{0}'", toolType.ToString());
            }

            var exportType = (localAndExportTypes[0] as LocalAndExportTypesAttribute).ExportType;
            var localType = (localAndExportTypes[0] as LocalAndExportTypesAttribute).LocalType;

            var method = typeof(OptionUtilities).GetMethod("CreateOptionCollection", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            var genericMethod = method.MakeGenericMethod(new System.Type[] { optionCollectionType, exportType, localType });
            genericMethod.Invoke(null, new object[] { this });
        }

        /// <summary>
        /// Post creation of option collections are called on each node, iterated from the graph in a bottom to top motion.
        /// </summary>
        public void
        PostCreateOptionCollection()
        {
            if (null != this.Module.Options)
            {
                this.Module.Options.FinalizeOptions(this);

                if ((this.Module is ICommonOptionCollection) && (this.Children != null))
                {
                    if (1 == this.Children.Count)
                    {
                        var child = this.Children[0];
                        if ((child.Module is IModuleCollection) && (child.Children != null) && (child.Children.Count == 1))
                        {
                            // there are no common options, because it is just the one module to build
                        }
                        else
                        {
                            (this.Module as ICommonOptionCollection).CommonOptionCollection = child.Module.Options;
                        }
                    }
                    else
                    {
                        BaseOptionCollection intersectedOptions = null;
                        foreach (var child in this.Children)
                        {
                            if (intersectedOptions == null)
                            {
                                intersectedOptions = child.Module.Options.Clone() as BaseOptionCollection;
                            }
                            else
                            {
                                intersectedOptions = intersectedOptions.Intersect(child.Module.Options);
                            }
                        }

                        if (null == intersectedOptions)
                        {
                            throw new Exception("There were no intersecting options in both option collections. This is highly unlikely");
                        }

                        (this.Module as ICommonOptionCollection).CommonOptionCollection = intersectedOptions;
                    }
                }
            }
        }

        public
        DependencyNode(
            BaseModule module,
            DependencyNode parent,
            Target target,
            int childIndex,
            bool nestedModule,
            string uniqueNameSuffix)
        {
            var moduleType = module.GetType();
            this.NodeCollection = null;
            this.Parent = parent;
            this.Target = target;
            this.IsModuleNested = nestedModule;
            this.OutputStringBuilder = new System.Text.StringBuilder();
            this.ErrorStringBuilder = new System.Text.StringBuilder();

            if (null == parent || !nestedModule)
            {
                var packageName = moduleType.Namespace;
                // always take the first part of the namespace
                packageName = packageName.Split('.')[0];
                var packages = State.PackageInfo;
                var package = packages[packageName];
                if (null == package)
                {
                    throw new Exception("No package found for '{0}'", packageName);
                }
                this.Package = package;
                this.EncapsulatingNode = this;
                this.ModuleName = moduleType.Name;
                this.UniqueModuleName = moduleType.FullName;
            }
            else
            {
                this.Package = parent.Package;
                this.EncapsulatingNode = parent.EncapsulatingNode;
                this.ModuleName = parent.ModuleName;
                this.UniqueModuleName = parent.GetChildModuleName(moduleType, childIndex);
            }
            if (null != uniqueNameSuffix)
            {
                this.UniqueModuleName = System.String.Format("{0}.{1}", this.UniqueModuleName, uniqueNameSuffix);
            }

            var builderInstance = State.BuilderInstance;
            if (null == builderInstance)
            {
                throw new Exception("Builder instance not found");
            }
            var buildFunction = builderInstance.GetType().GetMethod("Build", new System.Type[] { moduleType, System.Type.GetType("System.Boolean&") });
            if (null == buildFunction)
            {
                throw new Exception("Could not find method 'object {0}.Build({1}, {2})' for module '{3}'",
                                    builderInstance.GetType().ToString(),
                                    moduleType.BaseType.ToString(),
                                    System.Type.GetType("System.Boolean&").ToString(),
                                    moduleType.ToString());
            }
            var emptyBuildFunctions = buildFunction.GetCustomAttributes(typeof(EmptyBuildFunctionAttribute), false);
            if (null == emptyBuildFunctions || 0 == emptyBuildFunctions.Length)
            {
                this.BuildFunction = buildFunction;
            }
            else
            {
                this.BuildFunction = null;
            }

            this.LocalUpdatesAdded = new TypeArray();
            this.ExportedUpdatesAdded = new TypeArray();

            if (null != parent)
            {
                parent.AddChild(this);
            }
            this.BuildState = EBuildState.NotStarted;

            // assign the module to the node, and the node to the module
            this.Module = module;
            module.OwningNode = this;

            var moduleBuildDir = this.GetTargettedModuleBuildDirectoryLocation();
            (module.Locations[State.ModuleBuildDirLocationKey] as ScaffoldLocation).SetReference(moduleBuildDir);

            this.AreDependenciesProcessed = false;
            this.AreChildrenProcessed = false;

            this.PostActionNodes = null;
            this.EncapsulatingRequirements = new DependencyNodeCollection();
        }

        public Target Target
        {
            get;
            private set;
        }

        public DependencyNode Parent
        {
            get;
            private set;
        }

        public DependencyNodeCollection Children
        {
            get;
            private set;
        }

        public PackageInformation Package
        {
            get;
            private set;
        }

        public DependencyNode EncapsulatingNode
        {
            get;
            private set;
        }

        public DependencyNodeCollection EncapsulatingRequirements
        {
            get;
            private set;
        }

        public string ModuleName
        {
            get;
            private set;
        }

        public string UniqueModuleName
        {
            get;
            private set;
        }

        public DependencyNodeCollection ExternalDependents
        {
            get;
            private set;
        }

        public DependencyNodeCollection ExternalDependentFor
        {
            get;
            private set;
        }

        public DependencyNodeCollection RequiredDependents
        {
            get;
            private set;
        }

        public DependencyNodeCollection RequiredDependentFor
        {
            get;
            private set;
        }

        public BaseModule Module
        {
            get;
            private set;
        }

        public DependencyNodeCollection PostActionNodes
        {
            get;
            private set;
        }

        private DependencyNodeCollection
        Ancestors()
        {
            var collection = this.ExternalDependentFor;
            if (collection != null)
            {
                var ancestors = new DependencyNodeCollection();
                ancestors.AddRange(collection);
                foreach (var node in collection)
                {
                    var more = node.Ancestors();
                    if (null != more)
                    {
                        ancestors.AddRange(more);
                    }
                }
                return ancestors;
            }
            else
            {
                return null;
            }
        }

        public void
        AddExternalDependent(
            DependencyNode dependent)
        {
            if (null == dependent)
            {
                throw new Exception("External dependent node is invalid");
            }

            if (dependent == this)
            {
                throw new Exception("Circular dependency detected in external dependents for node '{0}'", this);
            }
            var ancestors = this.Ancestors();
            if (null != ancestors)
            {
                if (ancestors.Contains(dependent))
                {
                    var text = new System.Text.StringBuilder();
                    foreach (var ancestor in ancestors)
                    {
                        text.AppendFormat("\t{0}\n", ancestor.UniqueModuleName);
                    }
                    throw new Exception("Circular dependency detected in external dependents for node '{0}', '{1}' already exists in the dependency hierarchy:\n{2}", this.UniqueModuleName, dependent.UniqueModuleName, text.ToString());
                }
            }

            if (null == this.ExternalDependents)
            {
                this.ExternalDependents = new DependencyNodeCollection();
            }
            this.ExternalDependents.Add(dependent);

            if (null == dependent.ExternalDependentFor)
            {
                dependent.ExternalDependentFor = new DependencyNodeCollection();
            }
            dependent.ExternalDependentFor.Add(this);
        }

        public void
        AddRequiredDependent(
            DependencyNode required)
        {
            if (null == required)
            {
                throw new Exception("Required dependent node is invalid");
            }

            if (required == this)
            {
                throw new Exception("Circular dependency detected in required dependents for node '{0}'", this);
            }

            if (null == this.RequiredDependents)
            {
                this.RequiredDependents = new DependencyNodeCollection();
            }
            this.RequiredDependents.Add(required);

            if (null == required.RequiredDependentFor)
            {
                required.RequiredDependentFor = new DependencyNodeCollection();
            }
            required.RequiredDependentFor.Add(this);

            // each requirement goes into the encapsulating node as well
            // so that project generation can honour requirements correctly
            this.EncapsulatingNode.EncapsulatingRequirements.Add(required);
            if (null != this.EncapsulatingNode.ExternalDependentFor)
            {
                // if the encapsulating node is a dependent of another node, forward on the
                // requirements, again to honour them
                foreach (var encapsulationDependee in this.EncapsulatingNode.ExternalDependentFor)
                {
                    encapsulationDependee.EncapsulatingNode.EncapsulatingRequirements.Add(required);
                }
            }
        }

        public EBuildState BuildState
        {
            get
            {
                return this.buildState;
            }

            set
            {
                this.buildState = value;
                if (EBuildState.Succeeded == value || EBuildState.Failed == value)
                {
                    if (this.CompletedEvent != null)
                    {
                        this.CompletedEvent(this);
                    }
                }
            }
        }

        public object Data
        {
            get;
            set;
        }

        public System.Reflection.MethodInfo BuildFunction
        {
            get;
            private set;
        }

        public Location
        GetModuleBuildDirectoryLocation()
        {
            var packageBuildDirectory = this.Package.BuildDirectoryLocation;
            var moduleBuildDirectory = packageBuildDirectory.SubDirectory(this.ModuleName);
            return moduleBuildDirectory;
        }

        private Location
        GetTargettedModuleBuildDirectoryLocation()
        {
            var moduleBuildDirectory = this.GetModuleBuildDirectoryLocation();
            var targettedModuleBuildDirectory = moduleBuildDirectory.SubDirectory(TargetUtilities.DirectoryName(this.Target));
            return targettedModuleBuildDirectory;
        }

        public Location
        GetTargettedModuleBuildDirectoryLocation(
            string subDirectory)
        {
            var targettedModuleBuildDirectory = this.GetTargettedModuleBuildDirectoryLocation();
            targettedModuleBuildDirectory = targettedModuleBuildDirectory.SubDirectory(subDirectory);
            return targettedModuleBuildDirectory;
        }

        public bool IsModuleNested
        {
            get;
            private set;
        }

        public System.Text.StringBuilder OutputStringBuilder
        {
            get;
            private set;
        }

        public void
        OutputDataReceived(
            object sender,
            System.Diagnostics.DataReceivedEventArgs e)
        {
            if (!System.String.IsNullOrEmpty(e.Data))
            {
                //System.Diagnostics.Process process = sender as System.Diagnostics.Process;
                this.OutputStringBuilder.Append(e.Data + '\n');
            }
        }

        public System.Text.StringBuilder ErrorStringBuilder
        {
            get;
            private set;
        }

        public void
        ErrorDataReceived(
            object sender,
            System.Diagnostics.DataReceivedEventArgs e)
        {
            if (!System.String.IsNullOrEmpty(e.Data))
            {
                //System.Diagnostics.Process process = sender as System.Diagnostics.Process;
                this.ErrorStringBuilder.Append(e.Data + '\n');
            }
        }

        public DependencyNodeCollection NodeCollection
        {
            get;
            set;
        }

        public string
        GetChildModuleName(
            System.Type childModuleType,
            int childIndex)
        {
            string parentName = this.UniqueModuleName;
            string childName = childModuleType.Name;
            string childModuleName = System.String.Format("{0}.{1}{2}", parentName, childName, childIndex);
            return childModuleName;
        }

        public void
        FilterOutputLocations(
            Array<LocationKey> filterKeys,
            LocationArray filteredLocations)
        {
            var locationMap = this.Module.Locations;
            filteredLocations.AddRange(locationMap.FilterByKey(filterKeys));
        }

        public bool
        IsReadyToBuild()
        {
            if (this.BuildState != EBuildState.NotStarted)
            {
                return false;
            }

            if (null != this.Children)
            {
                bool complete = System.Threading.WaitHandle.WaitAll(this.Children.AllNodesCompletedEvent, 0);
                if (!complete)
                {
                    return false;
                }
            }

            if (null != this.ExternalDependents)
            {
                bool complete = System.Threading.WaitHandle.WaitAll(this.ExternalDependents.AllNodesCompletedEvent, 0);
                if (!complete)
                {
                    return false;
                }
            }

            if (null != this.RequiredDependents)
            {
                bool complete = System.Threading.WaitHandle.WaitAll(this.RequiredDependents.AllNodesCompletedEvent, 0);
                if (!complete)
                {
                    return false;
                }
            }

            return true;
        }

        public void
        AddPostActionNode(
            DependencyNode postActionNode)
        {
            // cache a list of the post action events for this node
            if (null == this.PostActionNodes)
            {
                this.PostActionNodes = new DependencyNodeCollection();
            }
            this.PostActionNodes.Add(postActionNode);

            postActionNode.AddExternalDependent(this);
        }
    }
}
