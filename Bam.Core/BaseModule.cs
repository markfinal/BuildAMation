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

    public interface IModuleExecution
    {
        void
        Execute(
            ExecutionContext context);

        bool IsUpToDate
        {
            get;
        }

        System.Threading.Tasks.Task ExecutionTask
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Abstract concept of a module, the base class for all buildables in BAM
    /// </summary>
    public abstract class Module :
        IModuleExecution
    {
        // private so that the factory method must be used
        protected Module()
        {
            var graph = Graph.Instance;
            if (null == graph.BuildEnvironment)
            {
                throw new Bam.Core.Exception("No build environment for module {0}", this.GetType().ToString());
            }

            graph.AddModule(this);
            this.Macros = new MacroList();
            // TODO: Can this be generalized to be a collection of files?
            this.GeneratedPaths = new System.Collections.Generic.Dictionary<FileKey, TokenizedString>();

            // add the package root
            var packageNameSpace = graph.CommonModuleType.Peek().Namespace;
            // TODO: temporarily check whether a V2 has been used in the namespace- trim if so
            if (packageNameSpace.EndsWith(".V2"))
            {
                packageNameSpace = packageNameSpace.Replace(".V2", string.Empty);
            }
            var packageInfo = Core.State.PackageInfo[packageNameSpace];
            this.Package = packageInfo;
            var packageRoot = packageInfo.Identifier.Location.AbsolutePath;
            this.Macros.Add("pkgroot", packageRoot);
            this.Macros.Add("modulename", this.GetType().Name);
            this.Macros.Add("packagename", packageInfo.Name);
            this.Macros.Add("pkgbuilddir", packageInfo.BuildDirectory);

            this.OwningRank = null;
            this.Tool = null;
            this.IsUpToDate = false;
            this.MetaData = null;
            this.BuildEnvironment = graph.BuildEnvironment;
            this.Macros.Add("config", this.BuildEnvironment.Configuration.ToString());
        }

        // TODO: is this virtual or abstract?
        protected virtual void
        Init(
            Module parent)
        { }

        public static T
        Create<T>(
            Module parent = null) where T : Module, new()
        {
            var filters = typeof(T).GetCustomAttributes(typeof(PlatformFilterAttribute), true) as PlatformFilterAttribute[];
            if (filters.Length > 0 && !filters[0].Platform.Includes(Graph.Instance.BuildEnvironment.Platform))
            {
                return null;
            }

            var module = new T();
            module.Init(parent);
            module.GetExecutionPolicy(Graph.Instance.Mode);
            return module;
        }

        protected void RegisterGeneratedFile(FileKey key, TokenizedString path)
        {
            if (this.GeneratedPaths.ContainsKey(key))
            {
                Core.Log.DebugMessage("Key '{0}' already exists", key);
                return;
            }
            this.GeneratedPaths.Add(key, path);
        }

        private void RegisterGeneratedFile(FileKey key)
        {
            this.RegisterGeneratedFile(key, null);
        }

        private void InternalDependsOn(Module module)
        {
            if (this.DependentsList.Contains(module))
            {
                return;
            }
            this.DependentsList.Add(module);
            module.DependeesList.Add(this);
        }

        public void DependsOn(Module module, params Module[] moreModules)
        {
            this.InternalDependsOn(module);
            foreach (var m in moreModules)
            {
                this.InternalDependsOn(m);
            }
        }

        private void InternalRequires(Module module)
        {
            if (this.RequiredDependentsList.Contains(module))
            {
                return;
            }
            this.RequiredDependentsList.Add(module);
            module.RequiredDependeesList.Add(this);
        }

        public void Requires(Module module, params Module[] moreModules)
        {
            this.InternalRequires(module);
            foreach (var m in moreModules)
            {
                this.InternalRequires(m);
            }
        }

        public Settings Settings
        {
            get;
            set;
        }

        public PackageInformation Package
        {
            get;
            private set;
        }

        public delegate void PrivatePatchDelegate(Settings settings);
        public void PrivatePatch(PrivatePatchDelegate dlg)
        {
            this.PrivatePatches.Add(dlg);
        }

        public delegate void PublicPatchDelegate(Settings settings, Module appliedTo);
        public void PublicPatch(PublicPatchDelegate dlg)
        {
            this.PublicPatches.Add(dlg);
        }

        public void UsePublicPatches(Module module)
        {
            this.ExternalPatches.Add(module.PublicPatches);
        }

        public bool HasPatches
        {
            get
            {
                return (this.PrivatePatches.Count() > 0) ||
                       (this.PublicPatches.Count() > 0) ||
                       (this.ExternalPatches.Count() > 0);
            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<Module> Dependents
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<Module>(this.DependentsList);
            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<Module> Requirements
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<Module>(this.RequiredDependentsList);
            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<Module> Children
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<Module>(this.DependentsList.Where(item => (item is IChildModule) && ((item as IChildModule).Parent == this)).ToList());
            }
        }

        private System.Collections.Generic.List<Module> DependentsList = new System.Collections.Generic.List<Module>();
        private System.Collections.Generic.List<Module> DependeesList = new System.Collections.Generic.List<Module>();

        private System.Collections.Generic.List<Module> RequiredDependentsList = new System.Collections.Generic.List<Module>();
        private System.Collections.Generic.List<Module> RequiredDependeesList = new System.Collections.Generic.List<Module>();

        private System.Collections.Generic.List<PrivatePatchDelegate> PrivatePatches = new System.Collections.Generic.List<PrivatePatchDelegate>();
        private System.Collections.Generic.List<PublicPatchDelegate> PublicPatches = new System.Collections.Generic.List<PublicPatchDelegate>();
        private System.Collections.Generic.List<System.Collections.Generic.List<PublicPatchDelegate>> ExternalPatches = new System.Collections.Generic.List<System.Collections.Generic.List<PublicPatchDelegate>>();

        public System.Collections.Generic.Dictionary<FileKey, TokenizedString> GeneratedPaths
        {
            get;
            private set;
        }

        public object MetaData
        {
            get;
            set;
        }

        protected abstract void
        ExecuteInternal(
            ExecutionContext context);

        void
        IModuleExecution.Execute(
            ExecutionContext context)
        {
            this.ExecuteInternal(context);
        }

        public bool TopLevel
        {
            get
            {
                var isTopLevel = (0 == this.DependeesList.Count) && (0 == this.RequiredDependeesList.Count);
                return isTopLevel;
            }
        }

        public MacroList Macros
        {
            get;
            private set;
        }

        public ModuleCollection OwningRank
        {
            get;
            set;
        }

        protected abstract void GetExecutionPolicy(string mode);

        public Tool Tool
        {
            get;
            protected set;
        }

        public void ApplySettingsPatches()
        {
            this.ApplySettingsPatches(this.Settings, true);
        }

        public void
        ApplySettingsPatches(
            Settings settings,
            bool honourParents)
        {
            if (null == settings)
            {
                return;
            }
            // Note: first private patches, followed by public patches
            // TODO: they could override each other - anyway to check?
            var parentModule = (this is IChildModule) && honourParents ? (this as IChildModule).Parent : null;
            if (parentModule != null)
            {
                foreach (var patch in parentModule.PrivatePatches)
                {
                    patch(settings);
                }
            }
            foreach (var patch in this.PrivatePatches)
            {
                patch(settings);
            }
            if (parentModule != null)
            {
                foreach (var patch in parentModule.PublicPatches)
                {
                    patch(settings, this);
                }
            }
            foreach (var patch in this.PublicPatches)
            {
                patch(settings, this);
            }
            if (parentModule != null)
            {
                foreach (var patchList in parentModule.ExternalPatches)
                {
                    foreach (var patch in patchList)
                    {
                        patch(settings, this);
                    }
                }
            }
            foreach (var patchList in this.ExternalPatches)
            {
                foreach (var patch in patchList)
                {
                    patch(settings, this);
                }
            }
        }

        public bool IsUpToDate
        {
            get;
            protected set;
        }

        public System.Threading.Tasks.Task ExecutionTask
        {
            get;
            set;
        }

        public abstract void Evaluate();

        public Environment BuildEnvironment
        {
            get;
            private set;
        }

        public Module GetEncapsulatingReferencedModule()
        {
            if (Graph.Instance.IsReferencedModule(this))
            {
                return this;
            }
            if (this.DependeesList.Count > 1)
            {
                throw new Exception("Too many dependees!");
            }
            if (0 == this.DependeesList.Count)
            {
                throw new Exception("Too few dependees!");
            }
            return this.DependeesList[0].GetEncapsulatingReferencedModule();
        }

        public void
        Complete()
        {
            var graph = Graph.Instance;
            var encapsulatingModule = this.GetEncapsulatingReferencedModule();
            this.Macros.Add("moduleoutputdir", System.IO.Path.Combine(encapsulatingModule.GetType().Name, this.BuildEnvironment.Configuration.ToString()));
        }
    }
}
    /// <summary>
    /// BaseModules are the base class for all real modules in package scripts.
    /// These are constructed by the Bam Core when they are required.
    /// Nested modules that appear as fields are either constructed automatically by
    /// the default constructor of their parent, or in the custom construct required to be
    /// written by the package author. As such, there must always be a default constructor
    /// in BaseModule.
    /// </summary>
    public abstract class BaseModule :
        IModule
    {
        private readonly LocationKey PackageDirKey = new LocationKey("PackageDirectory", ScaffoldLocation.ETypeHint.Directory);

        private void
        StubOutputLocations(
            System.Type moduleType)
        {
            this.Locations[State.ModuleBuildDirLocationKey] = new ScaffoldLocation(ScaffoldLocation.ETypeHint.Directory);

            var toolAssignment = moduleType.GetCustomAttributes(typeof(ModuleToolAssignmentAttribute), true);
            // this is duplicating work, as the toolset is in the Target.Toolset, but passing a Target down to
            // the BaseModule constructor will break a lot of existing scripts, and their simplicity
            // TODO: it may be considered a change in a future version
            var toolset = ModuleUtilities.GetToolsetForModule(moduleType);
            var toolAttr = toolAssignment[0] as ModuleToolAssignmentAttribute;
            if (!toolset.HasTool(toolAttr.ToolType))
            {
                return;
            }
            var tool = toolset.Tool(toolAttr.ToolType);
            if (null != tool)
            {
                foreach (var locationKey in tool.OutputLocationKeys(this))
                {
                    this.Locations[locationKey] = new ScaffoldLocation(locationKey.Type);
                }
            }
        }

        protected
        BaseModule()
        {
            this.ProxyPath = new ProxyModulePath();
            this.Locations = new LocationMap();
            this.Locations[State.BuildRootLocationKey] = State.BuildRootLocation;

            var moduleType = this.GetType();
            this.StubOutputLocations(moduleType);

            var packageName = moduleType.Namespace;
            var package = State.PackageInfo[packageName];
            if (null != package)
            {
                var root = new ScaffoldLocation(package.Identifier.Location, this.ProxyPath, ScaffoldLocation.ETypeHint.Directory, Location.EExists.Exists);
                this.PackageLocation = root;
            }
        }

        /// <summary>
        /// Locations are only valid for named modules
        /// </summary>
        public LocationMap Locations
        {
            get;
            private set;
        }

        public Location PackageLocation
        {
            get
            {
                return this.Locations[PackageDirKey];
            }

            private set
            {
                this.Locations[PackageDirKey] = value;
            }
        }

        public event UpdateOptionCollectionDelegate UpdateOptions;

        public virtual BaseOptionCollection Options
        {
            get;
            set;
        }

        public ProxyModulePath ProxyPath
        {
            get;
            private set;
        }

        public void
        ExecuteOptionUpdate(
            Target target)
        {
            if (null != this.UpdateOptions)
            {
                this.UpdateOptions(this as IModule, target);
            }
        }

        private DependencyNode owningNode = null;
        public DependencyNode OwningNode
        {
            get
            {
                return this.owningNode;
            }

            set
            {
                if (null != this.owningNode)
                {
                    throw new Exception("Module {0} cannot have it's node reassigned to {1}", this.owningNode.UniqueModuleName, value.UniqueModuleName);
                }

                this.owningNode = value;
            }
        }
    }
}
