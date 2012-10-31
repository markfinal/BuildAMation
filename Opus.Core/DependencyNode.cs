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
                    throw new Exception(System.String.Format("No package found for '{0}'", packageName));
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
                throw new Exception(System.String.Format("Could not find method 'object {0}.Build({1}, {2})' for module '{3}'",
                                                         builderInstance.GetType().ToString(),
                                                         moduleType.BaseType.ToString(),
                                                         System.Type.GetType("System.Boolean&").ToString(),
                                                         moduleType.ToString()), false);
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
            //IToolset toolset = this.Module.GetToolset(this.Target);
            IToolset toolset = this.Target.Toolset;

            System.Type moduleType = this.Module.GetType();
            AssignToolForModuleAttribute[] tools = moduleType.GetCustomAttributes(typeof(AssignToolForModuleAttribute), true) as AssignToolForModuleAttribute[];
            if (null == tools || 0 == tools.Length)
            {
                throw new Exception(System.String.Format("Module type '{0}' (base type '{1}') does not have a tool type assigned", moduleType.ToString(), moduleType.BaseType.ToString()), false);
            }

            // NEW STYLE
            // requires inheritence because the attribute is usually on the base class
            var moduleTools = moduleType.GetCustomAttributes(typeof(ModuleToolAssignmentAttribute), true);
            if (null == moduleTools || 0 == moduleTools.Length)
            {
                throw new Exception(System.String.Format("Module type '{0}' (base type '{1}') does not have any assigned tools", moduleType.ToString(), moduleType.BaseType.ToString()), false);
            }

            if (moduleTools.Length > 1)
            {
                throw new Exception(System.String.Format("There is more than one tool associated with this module '{0}'", moduleType.ToString()), false);
            }

            System.Type toolType = tools[0].ToolType;
            System.Type newToolType = (moduleTools[0] as ModuleToolAssignmentAttribute).ToolchainType;

            if (null == toolset)
            {
                Opus.Core.Log.MessageAll("DEBUG: No toolset for target '{0}' and tool '{1}' for module '{2}'", Target.ToString(), (null != newToolType) ? newToolType.ToString() : "undefined", moduleType.ToString());
            }
            else
            {
                Opus.Core.Log.MessageAll("DEBUG: Using toolset '{0}' for tool '{1}' for module '{2}'", toolset.ToString(), (null != newToolType) ? newToolType.ToString() : "undefined", moduleType.ToString());

                System.Type optionCollectionType2 = toolset.ToolOptionType(newToolType);
                if (null == optionCollectionType2)
                {
                    throw new Exception(System.String.Format("NEW STYLE: No option collection type for tool '{0}' from toolset '{1}'", newToolType.ToString(), toolset.ToString()), false);
                }

                var localAndExportTypes = newToolType.GetCustomAttributes(typeof(LocalAndExportTypesAttribute), false);

                if (localAndExportTypes.Length == 0)
                {
                    throw new Exception(System.String.Format("NEW STYLE: Missing local and export types attribute on tool type '{0}'", toolType.ToString()), false);
                }

                System.Type exportType2 = (localAndExportTypes[0] as LocalAndExportTypesAttribute).ExportType;
                System.Type localType2 = (localAndExportTypes[0] as LocalAndExportTypesAttribute).LocalType;

                System.Reflection.MethodInfo method2 = typeof(OptionUtilities).GetMethod("CreateOptionCollection", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                System.Reflection.MethodInfo genericMethod2 = method2.MakeGenericMethod(new System.Type[] { optionCollectionType2, exportType2, localType2 });
                this.Module.Options = genericMethod2.Invoke(null, new object[] { this }) as BaseOptionCollection;

                return;
            }

            if (toolType != newToolType)
            {
                Log.Full("NEW STYLE MISMATCH TOOL TYPE for module '{0}': was '{1}' now '{2}'", moduleType.ToString(), toolType.ToString(), newToolType.ToString());
                //throw new Exception(System.String.Format("NEW STYLE MISMATCH TOOL TYPE for module '{0}': was '{1}' now '{2}'", moduleType.ToString(), toolType.ToString(), newToolType.ToString()), false);
            }
            // do the switcheroo
            toolType = newToolType;
            // early out if the module does not have any build tools
            if (null == toolType)
            {
                return;
            }
            System.Type exportType = tools[0].ExportType;
            System.Type localType = tools[0].LocalType;
            {
                var localAndExportTypes = toolType.GetCustomAttributes(typeof(LocalAndExportTypesAttribute), false);

                if (localAndExportTypes.Length == 0)
                {
                    throw new Exception(System.String.Format("NEW STYLE: Missing local and export types attribute on tool type '{0}'", toolType.ToString()), false);
                }

                if (exportType != (localAndExportTypes[0] as LocalAndExportTypesAttribute).ExportType)
                {
                    throw new Exception("NEW STYLE MISMATCH EXPORT TYPE", false);
                }
                if (localType != (localAndExportTypes[0] as LocalAndExportTypesAttribute).LocalType)
                {
                    throw new Exception("NEW STYLE MISMATCH LOCAL TYPE", false);
                }
            }

            System.Type optionCollectionType = null;

            // if there is different toolsets in use, there may be a map for some but not all
            if (!State.Has("Toolchains", "Map") || !(State.Get("Toolchains", "Map") as System.Collections.Generic.Dictionary<System.Type, string>).ContainsKey(toolType))
            {
                var toolchainTypeMap = State.Get("ToolchainTypeMap", toolType.ToString());
                RegisterToolchainAttribute.ToolAndOptions toolAndOptions = (toolchainTypeMap as System.Collections.Generic.Dictionary<System.Type, RegisterToolchainAttribute.ToolAndOptions>)[toolType];
                optionCollectionType = toolAndOptions.OptionType;
            }
            else
            {
                var toolchainsMap = State.Get("Toolchains", "Map");
                string toolchainNameForThisTool = (toolchainsMap as System.Collections.Generic.Dictionary<System.Type, string>)[toolType];

                if (!State.Has("ToolchainTypeMap", toolchainNameForThisTool))
                {
                    throw new Exception(System.String.Format("NEW STYLE: Toolchain type map has not been registered for tool '{0}'", toolchainNameForThisTool), false);
                }

                var toolchainTypeMap = State.Get("ToolchainTypeMap", toolchainNameForThisTool);
#if true
                RegisterToolchainAttribute.ToolAndOptions toolAndOptions = (toolchainTypeMap as System.Collections.Generic.Dictionary<System.Type, RegisterToolchainAttribute.ToolAndOptions>)[toolType];
                optionCollectionType = toolAndOptions.OptionType;
#else
                System.Type realToolType = (toolchainTypeMap as System.Collections.Generic.Dictionary<System.Type, System.Type>)[toolType];

                // don't inherit here, as there is sometimes a hierarchy of classes for option collection
                var optionCollectionTypes = realToolType.GetCustomAttributes(typeof(AssignOptionCollectionAttribute), false);
                if (null == optionCollectionTypes || 0 == optionCollectionTypes.Length)
                {
                    throw new Exception(System.String.Format("NEW STYLE MISSING OPTION COLLECTION ASSIGNMENT for tool type '{0}'", realToolType.ToString()), false);
                }

                if (optionCollectionTypes.Length > 1)
                {
                    throw new Exception(System.String.Format("NEW STYLE TOO MANY OPTION COLLECTIONS for tool type '{0}'", realToolType.ToString()), false);
                }

                System.Type optionCollectionType = (optionCollectionTypes[0] as AssignOptionCollectionAttribute).OptionCollectionType;
#endif
            }

#if true
            // NEW STYLE
            System.Reflection.MethodInfo method = typeof(OptionUtilities).GetMethod("CreateOptionCollection", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            System.Reflection.MethodInfo genericMethod = method.MakeGenericMethod(new System.Type[] { optionCollectionType, exportType, localType });
            this.Module.Options = genericMethod.Invoke(null, new object[] { this }) as BaseOptionCollection;
#else
            string className = tools[0].ClassName; // TODO: could this be from the ToolType?
            System.Type toolOptionCollectionType = tools[0].OptionsType;
            if (null != toolType)
            {
                if (null == toolOptionCollectionType)
                {
                    string toolchainImplementation = ModuleUtilities.GetToolchainImplementation(moduleType);

                    if (!State.Has(toolchainImplementation, className))
                    {
                        throw new Exception(System.String.Format("Toolchain implementation '{0}' is missing or does not define '{1}'", toolchainImplementation, className), false);
                    }

                    toolOptionCollectionType = State.Get(toolchainImplementation, className) as System.Type;
                }
                System.Reflection.MethodInfo method = typeof(OptionUtilities).GetMethod("CreateOptionCollection", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                System.Reflection.MethodInfo genericMethod = method.MakeGenericMethod(new System.Type[] { toolOptionCollectionType, exportType, localType });
                this.Module.Options = genericMethod.Invoke(null, new object[] { this, className }) as BaseOptionCollection;
            }
#endif
        }

        public DependencyNode(IModule module, DependencyNode parent, Target target, int childIndex, bool nestedModule)
        {
            this.Initialize(module.GetType(), parent, target, childIndex, nestedModule);
            this.Module = module;
            module.OwningNode = this;
        }
        
        public DependencyNode(System.Type moduleType, DependencyNode parent, Target target, int childIndex, bool nestedModule)
        {
            this.Initialize(moduleType, parent, target, childIndex, nestedModule);

            IModule module = null;
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
                throw new Exception(System.String.Format("Cannot construct object of type '{0}'. Missing public constructor?", moduleType.ToString()));
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

        public IModule Module
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
                throw new Exception(System.String.Format("Circular dependency detected in external dependents for node '{0}'", this), false);
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
                throw new Exception(System.String.Format("Circular dependency detected in required dependents for node '{0}'", this), false);
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