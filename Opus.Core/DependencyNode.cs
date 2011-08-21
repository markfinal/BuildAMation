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
            return System.String.Format("Family '{0}', instance '{1}', parent '{2}', type '{3}' (base '{4}'), state '{5}', target '{6}', rank {7}, {8}, {9}, {10}",
                                        this.ModuleName,
                                        this.UniqueModuleName,
                                        (this.Parent != null) ? this.Parent.UniqueModuleName : null,
                                        this.Module.GetType().ToString(),
                                        this.Module.GetType().BaseType.ToString(),
                                        this.BuildState.ToString(),
                                        this.Target.Key,
                                        this.Rank.ToString(),
                                        (this.Children != null) ? "Has children" : "No children",
                                        (this.ExternalDependents != null) ? "Has dependents" : "No dependents",
                                        (this.RequiredDependents != null) ? "Has required" : "No required");
        }

        private EBuildState buildState;
        
        public delegate void CompleteEventHandler(DependencyNode node);
        public event CompleteEventHandler CompletedEvent;

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
            this.Parent = parent;
            if (null != parent)
            {
                parent.AddChild(this);
            }
            this.BuildState = EBuildState.NotStarted;
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
        }

        public void CreateOptionCollection()
        {
            System.Type moduleType = this.Module.GetType();
            AssignToolForModuleAttribute[] tools = moduleType.GetCustomAttributes(typeof(AssignToolForModuleAttribute), true) as AssignToolForModuleAttribute[];
            if (null == tools || 0 == tools.Length)
            {
                throw new Exception(System.String.Format("Module type '{0}' (base type '{1}') does not have a tool type assigned", moduleType.ToString(), moduleType.BaseType.ToString()), false);
            }
            System.Type toolType = tools[0].ToolType;
            System.Type exportType = tools[0].ExportType;
            System.Type localType = tools[0].LocalType;
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
                if ((EBuildState.Succeeded == value || EBuildState.Failed == value) && this.CompletedEvent != null)
                {
                    this.CompletedEvent(this);
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
            string targettedModuleBuildDirectory = System.IO.Path.Combine(moduleBuildDirectory, this.Target.DirectoryName);
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
    }
}