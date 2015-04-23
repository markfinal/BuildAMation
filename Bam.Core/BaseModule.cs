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
        void Execute(string mode);
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
                throw new System.Exception("No build environment");
            }

            graph.AddModule(this);
            this.Macros = new MacroList();
            // TODO: Can this be generalized to be a collection of files?
            this.GeneratedPaths = new System.Collections.Generic.Dictionary<FileKey, TokenizedString>();

            // add the package root
            var packageNameSpace = graph.CommonModuleType.Peek().Namespace;
            this.Macros.Add("pkgroot", new TokenizedString(System.String.Format(@"c:\dev\testing\{0}", packageNameSpace), null));
            this.Macros.Add("modulename", new TokenizedString(this.GetType().Name, null));

            this.OwningRank = null;
        }

        // TODO: is this virtual or abstract?
        protected virtual void init()
        { }

        public static T Create<T>() where T : Module, new()
        {
            var module = new T();
            module.init();
            return module;
        }

        protected void RegisterGeneratedFile(FileKey key, TokenizedString path)
        {
            this.GeneratedPaths.Add(key, path);
        }

        private void RegisterGeneratedFile(FileKey key)
        {
            this.RegisterGeneratedFile(key, null);
        }

        private void InternalDependsOn(Module module)
        {
            this.dependents.Add(module);
            module.dependees.Add(this);
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
            this.requiredDependents.Add(module);
            module.requiredDependees.Add(this);
        }

        public void Requires(Module module, params Module[] moreModules)
        {
            this.InternalRequires(module);
            foreach (var m in moreModules)
            {
                this.InternalRequires(m);
            }
        }

        public delegate void PatchDelegate(Settings settings);
        public void PatchSettings(PatchDelegate dlg)
        {
            this.patches.Add(dlg);
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<Module> Dependents
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<Module>(this.dependents);
            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<Module> Requirements
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<Module>(this.requiredDependents);
            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<Module> Children
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<Module>(this.dependents.Where(item => (item as IChildModule).Parent == this).ToList());
            }
        }

        private System.Collections.Generic.List<Module> dependents = new System.Collections.Generic.List<Module>();
        private System.Collections.Generic.List<Module> dependees = new System.Collections.Generic.List<Module>();

        private System.Collections.Generic.List<Module> requiredDependents = new System.Collections.Generic.List<Module>();
        private System.Collections.Generic.List<Module> requiredDependees = new System.Collections.Generic.List<Module>();

        private System.Collections.Generic.List<PatchDelegate> patches = new System.Collections.Generic.List<PatchDelegate>();

        public System.Collections.Generic.Dictionary<FileKey, TokenizedString> GeneratedPaths
        {
            get;
            private set;
        }

        protected abstract void ExecuteInternal(string mode);

        void IModuleExecution.Execute(string mode)
        {
            this.ExecuteInternal(mode);
        }

        public bool TopLevel
        {
            get
            {
                var isTopLevel = (0 == this.dependees.Count) && (0 == this.requiredDependees.Count);
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
