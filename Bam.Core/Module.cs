#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
using System.Linq;
namespace Bam.Core
{
    /// <summary>
    /// Abstract concept of a module, the base class for all buildables in BAM
    /// </summary>
    public abstract class Module :
        IModuleExecution
    {
        static protected System.Collections.Generic.List<Module> AllModules = new System.Collections.Generic.List<Module>();

        // private so that the factory method must be used
        protected Module()
        {
            var graph = Graph.Instance;
            if (null == graph.BuildEnvironment)
            {
                throw new Exception("No build environment for module {0}", this.GetType().ToString());
            }

            graph.AddModule(this);
            this.Macros = new MacroList();
            // TODO: Can this be generalized to be a collection of files?
            this.GeneratedPaths = new System.Collections.Generic.Dictionary<FileKey, TokenizedString>();

            // add the package root
            var packageNameSpace = graph.CommonModuleType.Peek().Namespace;
            var packageDefinition = graph.Packages.Where(item => item.Name == packageNameSpace).FirstOrDefault();
            if (null == packageDefinition)
            {
                var includeTests = CommandLineProcessor.Evaluate(new UseTests());
                if (includeTests && packageNameSpace.EndsWith(".tests"))
                {
                    packageNameSpace = packageNameSpace.Replace(".tests", string.Empty);
                    packageDefinition = graph.Packages.Where(item => item.Name == packageNameSpace).FirstOrDefault();
                }

                if (null == packageDefinition)
                {
                    throw new Exception("Unable to locate package for namespace '{0}'", packageNameSpace);
                }
            }
            this.PackageDefinition = packageDefinition;
            this.Macros.AddVerbatim("packagedir", packageDefinition.GetPackageDirectory());
            this.Macros.AddVerbatim("packagename", packageDefinition.Name);
            this.Macros.AddVerbatim("packagebuilddir", packageDefinition.GetBuildDirectory());
            this.Macros.AddVerbatim("modulename", this.GetType().Name);

            this.OwningRank = null;
            this.Tool = null;
            this.MetaData = null;
            this.BuildEnvironment = graph.BuildEnvironment;
            this.Macros.AddVerbatim("config", this.BuildEnvironment.Configuration.ToString());
            this.ReasonToExecute = ExecuteReasoning.Undefined();
        }

        // TODO: is this virtual or abstract?
        protected virtual void
        Init(
            Module parent)
        { }

        public static bool
        CanCreate(
            System.Type moduleType)
        {
            var filters = moduleType.GetCustomAttributes(typeof(PlatformFilterAttribute), true) as PlatformFilterAttribute[];
            if (0 == filters.Length)
            {
                // unconditional
                return true;
            }
            if (filters[0].Platform.Includes(Graph.Instance.BuildEnvironment.Platform))
            {
                // platform is a match
                return true;
            }
            Log.DebugMessage("Cannot create module of type {0} as it does not satisfy the platform filter", moduleType.ToString());
            return false;
        }

        public delegate void ModulePreInitDelegate(Module module);

        public static T
        Create<T>(
            Module parent = null,
            ModulePreInitDelegate preInitCallback = null) where T : Module, new()
        {
            try
            {
                if (!CanCreate(typeof(T)))
                {
                    return null;
                }

                var module = new T();
                if (preInitCallback != null)
                {
                    preInitCallback(module);
                }
                module.Init(parent);
                module.GetExecutionPolicy(Graph.Instance.Mode);
                AllModules.Add(module);
                return module;
            }
            catch (ModuleCreationException exception)
            {
                // persist the module type from the inner-most module creation call
                throw exception;
            }
            catch (System.Exception exception)
            {
                throw new ModuleCreationException(typeof(T), exception);
            }
        }

        protected void
        RegisterGeneratedFile(
            FileKey key,
            TokenizedString path)
        {
            if (this.GeneratedPaths.ContainsKey(key))
            {
                Log.DebugMessage("Key '{0}' already exists", key);
                return;
            }
            this.GeneratedPaths.Add(key, path);
        }

        private void
        RegisterGeneratedFile(
            FileKey key)
        {
            this.RegisterGeneratedFile(key, null);
        }

        private void
        InternalDependsOn(
            Module module)
        {
            if (this.DependentsList.Contains(module))
            {
                return;
            }
            this.DependentsList.Add(module);
            module.DependeesList.Add(this);
        }

        public void
        DependsOn(
            Module module,
            params Module[] moreModules)
        {
            this.InternalDependsOn(module);
            foreach (var m in moreModules)
            {
                this.InternalDependsOn(m);
            }
        }

        public void
        DependsOn(
            System.Collections.Generic.IEnumerable<Module> modules)
        {
            this.DependentsList.AddRangeUnique(modules);
            foreach (var module in modules)
            {
                module.DependeesList.Add(this);
            }
        }

        private void
        InternalRequires(
            Module module)
        {
            if (this.RequiredDependentsList.Contains(module))
            {
                return;
            }
            this.RequiredDependentsList.Add(module);
            module.RequiredDependeesList.Add(this);
        }

        public void
        Requires(
            Module module,
            params Module[] moreModules)
        {
            this.InternalRequires(module);
            foreach (var m in moreModules)
            {
                this.InternalRequires(m);
            }
        }

        public void
        Requires(
            System.Collections.Generic.IEnumerable<Module> modules)
        {
            this.RequiredDependentsList.AddRangeUnique(modules);
            foreach (var module in modules)
            {
                module.RequiredDependeesList.Add(this);
            }
        }

        public Settings Settings
        {
            get;
            set;
        }

        public PackageDefinition PackageDefinition
        {
            get;
            private set;
        }

        public delegate void PrivatePatchDelegate(Settings settings);

        public void
        PrivatePatch(
            PrivatePatchDelegate dlg)
        {
            this.PrivatePatches.Add(dlg);
        }

        public delegate void PublicPatchDelegate(Settings settings, Module appliedTo);

        public void
        PublicPatch(
            PublicPatchDelegate dlg)
        {
            this.PublicPatches.Add(dlg);
        }

        public void
        UsePublicPatches(
            Module module)
        {
            this.ExternalPatches.Add(module.PublicPatches);
            this.ExternalPatches.AddRange(module.ExternalPatches);
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
                return this.DependentsList.ToReadOnlyCollection();
            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<Module> Dependees
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<Module>(this.DependeesList);
            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<Module> Requirements
        {
            get
            {
                return this.RequiredDependentsList.ToReadOnlyCollection();
            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<Module> Children
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<Module>(this.DependentsList.Where(item => (item is IChildModule) && ((item as IChildModule).Parent == this)).ToList());
            }
        }

        private Array<Module> DependentsList = new Array<Module>();
        private System.Collections.Generic.List<Module> DependeesList = new System.Collections.Generic.List<Module>();

        private Array<Module> RequiredDependentsList = new Array<Module>();
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
            if (context.Evaluate)
            {
                if (null != this.EvaluationTask)
                {
                    this.EvaluationTask.Wait();
                }
                if (null == this.ReasonToExecute)
                {
                    Log.Message(context.ExplainLoggingLevel, "Module {0} is up-to-date", this.ToString());
                    return;
                }
                Log.Message(context.ExplainLoggingLevel, "Module {0} will change because {1}.", this.ToString(), this.ReasonToExecute.ToString());
            }
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

        protected abstract void
        GetExecutionPolicy(
            string mode);

        private Module TheTool;
        public Module Tool
        {
            get
            {
                return this.TheTool;
            }

            protected set
            {
                if ((null != value) && !(value is ITool))
                {
                    throw new Exception("Tool {0} does not implement {1}", value.GetType().ToString(), typeof(ITool).ToString());
                }
                this.TheTool = value;
            }
        }

        public void
        ApplySettingsPatches()
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

        public ExecuteReasoning ReasonToExecute
        {
            get;
            protected set;
        }

        public System.Threading.Tasks.Task ExecutionTask
        {
            get;
            set;
        }

        public System.Threading.Tasks.Task
        EvaluationTask
        {
            get;
            protected set;
        }

        public abstract void
        Evaluate();

        public Environment BuildEnvironment
        {
            get;
            private set;
        }

        public Module
        GetEncapsulatingReferencedModule()
        {
            if (Graph.Instance.IsReferencedModule(this))
            {
                return this;
            }
            if (this.DependeesList.Count > 1)
            {
                Log.DebugMessage("More than one dependee attached to {0}, so taking the first as the encapsulating module. This may be incorrect.", this.ToString());
            }
            if (this.RequiredDependeesList.Count > 1)
            {
                Log.DebugMessage("More than one requiree attached to {0}, so taking the first as the encapsulating module. This may be incorrect.", this.ToString());
            }
            Module encapsulating;
            if (0 == this.DependeesList.Count)
            {
                if (0 == this.RequiredDependeesList.Count)
                {
                    throw new Exception("No dependees or requirees attached to {0}. Cannot determine the encapsulating module", this.ToString());
                }
                encapsulating = this.RequiredDependeesList[0].GetEncapsulatingReferencedModule();
            }
            else
            {
                encapsulating = this.DependeesList[0].GetEncapsulatingReferencedModule();
            }
            this.Macros.Add("encapsulatingbuilddir", encapsulating.Macros["packagebuilddir"]);
            return encapsulating;
        }

        private void
        Complete()
        {
            var graph = Graph.Instance;
            var encapsulatingModule = this.GetEncapsulatingReferencedModule();
            // TODO: there may have to be a more general module type for something that is not built, as this affects modules referred to prebuilts too
            // note that this cannot be a class, as modules already are derived from another base class (generally)
            if (!(encapsulatingModule is PreBuiltTool))
            {
                this.Macros.Add("moduleoutputdir", graph.BuildModeMetaData.ModuleOutputDirectory(this, encapsulatingModule));
            }

            // modules that are encapsulated, have settings, and aren't a child (as their parent is also encapsulated, and thus gets this too), inherit the
            // public patches from the encapsulating module, since this is identical behavior to 'using public patches'
            if (encapsulatingModule != this)
            {
                if (this.Settings != null)
                {
                    if (!(this is IChildModule))
                    {
                        this.UsePublicPatches(encapsulatingModule);
                    }
                }
            }
        }

        static public void
        CompleteModules()
        {
            foreach (var module in AllModules.Reverse<Module>())
            {
                module.Complete();
            }
        }

        public TokenizedString
        MakePlaceholderPath()
        {
            return TokenizedString.Create(string.Empty, this);
        }

        public TokenizedString
        CreateTokenizedString(
            string format,
            params TokenizedString[] argv)
        {
            if (0 == argv.Length)
            {
                return TokenizedString.Create(format, this);
            }
            var positionalTokens = new TokenizedStringArray(argv);
            return TokenizedString.Create(format, this, positionalTokens);
        }

        public static int
        Count
        {
            get
            {
                return AllModules.Count;
            }
        }
    }
}
