// <copyright file="DependencyNode.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public enum EBuildState
    {
        NotStarted,
        Pending,
        Succeeded,
        Failed
    }

    public sealed class DependencyNode
    {
        public override string ToString()
        {
            System.Text.StringBuilder output = new System.Text.StringBuilder();
            output.AppendFormat("Family '{0}'", this.ModuleName);
            output.AppendFormat(" Instance '{0}'", this.UniqueModuleName);
            if (this.Parent != null)
            {
                output.AppendFormat(" Parent '{0}'", this.Parent.UniqueModuleName);
            }
            output.AppendFormat(" Type '{0}'", this.Module.GetType().ToString());
            output.AppendFormat(" Base '{0}'", this.Module.GetType().BaseType.ToString());
            output.AppendFormat(" Target '{0}'", this.Target.Key);
            if (-1 != this.Rank)
            {
                output.AppendFormat(" Rank {0}", this.Rank);
            }
            if (null != this.Children)
            {
                output.Append(" Children { ");
                foreach (DependencyNode node in this.Children)
                {
                    output.AppendFormat("{0} ", node.UniqueModuleName);
                }
                output.Append("}");
            }
            if (null != this.ExternalDependents)
            {
                output.Append(" Deps { ");
                foreach (DependencyNode node in this.ExternalDependents)
                {
                    output.AppendFormat("{0} ", node.UniqueModuleName);
                }
                output.Append("}");
            }
            if (null != this.RequiredDependents)
            {
                output.Append(" Reqs { ");
                foreach (DependencyNode node in this.RequiredDependents)
                {
                    output.AppendFormat("{0} ", node.UniqueModuleName);
                }
                output.Append("}");
            }

            return output.ToString();
        }

        private EBuildState buildState;
        
        public delegate void CompleteEventHandler(DependencyNode node);
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

        private void AddChild(DependencyNode childNode)
        {
            if (null == this.Children)
            {
                this.Children = new DependencyNodeCollection();
            }

            this.Children.Add(childNode);
        }
        
        private void Initialize(System.Type moduleType, DependencyNode parent, Target target, int childIndex, bool nestedModule)
        {
            if (!typeof(BaseModule).IsAssignableFrom(moduleType))
            {
                throw new Exception("Module type '{0}' does not derive from the Opus.Core.BaseModule class", moduleType.ToString());
            }

            this.Rank = -1;
            this.Parent = parent;
            this.Target = target;
            this.IsModuleNested = nestedModule;
            this.OutputStringBuilder = new System.Text.StringBuilder();
            this.ErrorStringBuilder = new System.Text.StringBuilder();

            if (null == parent || !nestedModule)
            {
                string packageName = moduleType.Namespace;
                PackageInformationCollection packages = State.PackageInfo;
                PackageInformation package = packages[packageName];
                if (null == package)
                {
                    throw new Exception("No package found for '{0}'", packageName);
                }
                this.Package = package;
                this.ModuleName = moduleType.Name;
                this.UniqueModuleName = moduleType.FullName;
            }
            else
            {
                this.Package = parent.Package;
                this.ModuleName = parent.ModuleName;
                this.UniqueModuleName = parent.GetChildModuleName(moduleType, childIndex);
            }

            IBuilder builderInstance = State.BuilderInstance;
            if (null == builderInstance)
            {
                throw new Exception("Builder instance not found");
            }
            System.Reflection.MethodInfo buildFunction = builderInstance.GetType().GetMethod("Build", new System.Type[] { moduleType, System.Type.GetType("System.Boolean&") });
            if (null == buildFunction)
            {
                throw new Exception("Could not find method 'object {0}.Build({1}, {2})' for module '{3}'",
                                    builderInstance.GetType().ToString(),
                                    moduleType.BaseType.ToString(),
                                    System.Type.GetType("System.Boolean&").ToString(),
                                    moduleType.ToString());
            }
            object[] emptyBuildFunctions = buildFunction.GetCustomAttributes(typeof(EmptyBuildFunctionAttribute), false);
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
        }

        public void CreateOptionCollection()
        {
            IToolset toolset = this.Target.Toolset;

            System.Type moduleType = this.Module.GetType();

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

            System.Type toolType = (moduleTools[0] as ModuleToolAssignmentAttribute).ToolchainType;
            if (null == toolset)
            {
                Opus.Core.Log.DebugMessage("No toolset for target '{0}' and tool '{1}' for module '{2}'", Target.ToString(), (null != toolType) ? toolType.ToString() : "undefined", moduleType.ToString());
                return;
                //throw new Exception("No toolset for target '{0}' and tool '{1}' for module '{2}'", Target.ToString(), (null != toolType) ? toolType.ToString() : "undefined", moduleType.ToString());
            }

            Opus.Core.Log.DebugMessage("Using toolset '{0}' for tool '{1}' for module '{2}'", toolset.ToString(), (null != toolType) ? toolType.ToString() : "undefined", moduleType.ToString());
            if (!toolType.IsInterface)
            {
                throw new Exception("Tool '{0}' is NOT an interface", toolType.ToString());
            }

            System.Type optionCollectionType = toolset.ToolOptionType(toolType);
            if (null == optionCollectionType)
            {
                Opus.Core.Log.DebugMessage("No option collection type for tool '{0}' from toolset '{1}'", toolType.ToString(), toolset.ToString());
                return;
                //throw new Exception("No option collection type for tool '{0}' from toolset '{1}'", toolType.ToString(), toolset.ToString());
            }

            var localAndExportTypes = toolType.GetCustomAttributes(typeof(LocalAndExportTypesAttribute), false);

            if (localAndExportTypes.Length == 0)
            {
                throw new Exception("Missing local and export types attribute on tool type '{0}'", toolType.ToString());
            }

            System.Type exportType = (localAndExportTypes[0] as LocalAndExportTypesAttribute).ExportType;
            System.Type localType = (localAndExportTypes[0] as LocalAndExportTypesAttribute).LocalType;

            System.Reflection.MethodInfo method = typeof(OptionUtilities).GetMethod("CreateOptionCollection", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            System.Reflection.MethodInfo genericMethod = method.MakeGenericMethod(new System.Type[] { optionCollectionType, exportType, localType });
            this.Module.Options = genericMethod.Invoke(null, new object[] { this }) as BaseOptionCollection;
        }

        public DependencyNode(BaseModule module, DependencyNode parent, Target target, int childIndex, bool nestedModule)
        {
            this.Initialize(module.GetType(), parent, target, childIndex, nestedModule);
            this.Module = module;
            module.OwningNode = this;
        }
        
        public DependencyNode(System.Type moduleType, DependencyNode parent, Target target, int childIndex, bool nestedModule)
        {
            this.Initialize(moduleType, parent, target, childIndex, nestedModule);

            BaseModule module = null;
            try
            {
                if (null != moduleType.GetConstructor(new System.Type[] { typeof(Target) }))
                {
                    module = ModuleFactory.CreateModule(moduleType, target);
                }
                else
                {
                    module = ModuleFactory.CreateModule(moduleType);
                }
            }
            catch (System.MissingMethodException)
            {
                throw new Exception("Cannot construct object of type '{0}'. Missing public constructor?", moduleType.ToString());
            }

            this.Module = module;
            module.OwningNode = this;
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

        public BaseModule Module
        {
            get;
            private set;
        }
        
        public void AddExternalDependent(DependencyNode dependent)
        {
            if (null == dependent)
            {
                throw new Exception("External dependent node is invalid");
            }

            if (dependent == this)
            {
                throw new Exception("Circular dependency detected in external dependents for node '{0}'", this);
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

        public void AddRequiredDependent(DependencyNode required)
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

        public string GetModuleBuildDirectory()
        {
            string packageBuildDirectory = this.Package.BuildDirectory;
            string moduleBuildDirectory = System.IO.Path.Combine(packageBuildDirectory, this.ModuleName);
            return moduleBuildDirectory;
        }

        public string GetTargettedModuleBuildDirectory(string subDirectory)
        {
            string moduleBuildDirectory = this.GetModuleBuildDirectory();
            string targettedModuleBuildDirectory = System.IO.Path.Combine(moduleBuildDirectory, TargetUtilities.DirectoryName(this.Target));
            if (null != subDirectory)
            {
                targettedModuleBuildDirectory = System.IO.Path.Combine(targettedModuleBuildDirectory, subDirectory);
            }

            return targettedModuleBuildDirectory;
        }

        public string GetTargetPathName(string subDirectory, string targetPrefix, string targetLeafName, string targetExtension)
        {
            string targetFileName = System.String.Format("{0}{1}{2}", targetPrefix, targetLeafName, targetExtension);
            string moduleTargetPathName = System.IO.Path.Combine(this.GetTargettedModuleBuildDirectory(subDirectory), targetFileName);
            return moduleTargetPathName;
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

        public void OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
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

        public void ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (!System.String.IsNullOrEmpty(e.Data))
            {
                //System.Diagnostics.Process process = sender as System.Diagnostics.Process;
                this.ErrorStringBuilder.Append(e.Data + '\n');
            }
        }

        public int Rank
        {
            get;
            set;
        }

        public string GetChildModuleName(System.Type childModuleType, int childIndex)
        {
            string parentName = this.UniqueModuleName;
            string childName = childModuleType.Name;
            string childModuleName = System.String.Format("{0}.{1}{2}", parentName, childName, childIndex);
            return childModuleName;
        }

        public void FilterOutputPaths(System.Enum filter, StringArray paths)
        {
            BaseOptionCollection options = this.Module.Options;
            if (null == options)
            {
                return;
            }

            options.FilterOutputPaths(filter, paths);
        }

        public bool IsReadyToBuild()
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
    }
}