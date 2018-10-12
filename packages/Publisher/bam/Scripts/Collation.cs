#region License
// Copyright (c) 2010-2018, Mark Final
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
using Bam.Core;
using System.Linq;
namespace Publisher
{
    // TODO: move these to another source file
    abstract class PreExistingObject :
        Bam.Core.Module
    {
        public Bam.Core.Module ParentOfCollationModule
        {
            get;
            private set;
        }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var graph = Bam.Core.Graph.Instance;
            var index = (graph.CommonModuleType.Count > 1) ? 1 : 0;
            // Peek(0) returns the Collation
            // Peek(1) returns whatever asked for the Collation, either:
            // - nothing, in which case the Collation was in the master package and is buildable
            // - the module that asked for the Collation, generally used when library headers are published to a public include path
            var anchorType = graph.CommonModuleType.Peek(index);
            this.ParentOfCollationModule = graph.GetReferencedModule(
                this.BuildEnvironment,
                anchorType
            );
        }

        protected override void
        EvaluateInternal()
        {
            // pre-existing so never needs evaluating
            this.ReasonToExecute = null;
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            // pre-existing so never needs executing
        }
    }

    sealed class PreExistingFile :
        PreExistingObject
    {
        public const string ExistingFileKey = "Preexisting file to be copied";

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.RegisterGeneratedFile(ExistingFileKey, this.Macros["ExistingFile"]);
        }
    }

    sealed class PreExistingDirectory :
        PreExistingObject
    {
        public const string ExistingDirectoryKey = "Preexisting directory to be copied";

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.RegisterGeneratedFile(ExistingDirectoryKey, this.Macros["ExistingDirectory"]);
        }
    }

    /// <summary>
    /// Derive from this module to collate files and directories into a runnable distribution.
    /// Collation occurs within a folder in the build root called the 'publishing root'.
    /// There are default distribution layouts and mapping of module output files to subdirectories of
    /// the layouts. These can be set by calling SetDefaultMacrosAndMappings(). Otherwise, the user is
    /// free to define their own layouts.
    /// Collating an executable, using the Include() function call, will walk it's dependency hierarchy
    /// and include any runtime dependents. The collation of the executable is called the anchor, and its
    /// dependents are relative to that anchor.
    /// You can collate more than one anchor. Shared dependent runtime files may be either copied once,
    /// or duplicated per anchor, depending on the build mode, in order to create runnable executables.
    /// Any preexisting files and directories can also be attached to an anchor using the IncludeFiles()
    /// and IncludeDirectories() functions.
    /// All collated paths are specified as absolute next to the publishing root using TokenizedStrings.
    /// This class has properties that expose standad locations, e.g. ExecutableDir, PluginDir.
    /// </summary>
    public abstract class Collation :
        Bam.Core.Module
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Mapping = new ModuleOutputDefaultPublishingPathMapping();

            // TODO: if this is used as a position argument, it might end up in an infinite recursion during string parsing
            this.PublishRoot = Bam.Core.TokenizedString.Create("$(publishdir)", null);
        }

        protected sealed override void
        EvaluateInternal()
        {
            // TODO
        }

        protected sealed override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            switch (Bam.Core.Graph.Instance.Mode)
            {
#if D_PACKAGE_MAKEFILEBUILDER
                case "MakeFile":
                    MakeFileBuilder.Support.AddCheckpoint(this);
                    break;
#endif

                default:
                    // does not need to do anything
                    break;
            }
        }

        /// <summary>
        /// The type of application being published.
        /// </summary>
        public enum EPublishingType
        {
            /// <summary>
            /// Application in a console application.
            /// </summary>
            ConsoleApplication,

            /// <summary>
            /// Application is a GUI application.
            /// On OSX, this is an application bundle, and will automatically appear in a <name>.app/Contents/MacOS folder
            /// under the publishing root.
            /// </summary>
            WindowedApplication,

            /// <summary>
            /// Distributing a library, with headers, import/static/dynamic libraries, and potentially tools or tests.
            /// </summary>
            Library

            // TODO: macosFramework
        }

        // this is doubling up the cost of the this.Requires list, but at less runtime cost
        // for expanding each CollatedObject to peek as it's properties
        private System.Collections.Generic.Dictionary<System.Tuple<Bam.Core.Module, string>, ICollatedObject> collatedObjects = new System.Collections.Generic.Dictionary<System.Tuple<Module, string>, ICollatedObject>();
        private System.Collections.Generic.List<System.Tuple<CollatedObject, Bam.Core.TokenizedString>> preExistingCollatedObjects = new System.Collections.Generic.List<System.Tuple<CollatedObject, Bam.Core.TokenizedString>>();

        private Bam.Core.TokenizedString PublishRoot
        {
            get;
            set;
        }

        /// <summary>
        /// Set or get the directory where executables are published.
        /// </summary>
        public Bam.Core.TokenizedString ExecutableDir
        {
            get
            {
                return this.Macros["ExecutableDir"];
            }

            set
            {
                this.Macros["ExecutableDir"] = value;
            }
        }

        /// <summary>
        /// Set or get the directory where dynamic libraries are published.
        /// </summary>
        public Bam.Core.TokenizedString DynamicLibraryDir
        {
            get
            {
                return this.Macros["DynamicLibraryDir"];
            }

            set
            {
                this.Macros["DynamicLibraryDir"] = value;
            }
        }

        /// <summary>
        /// Set or get the directory where static libraries are published.
        /// </summary>
        public Bam.Core.TokenizedString StaticLibraryDir
        {
            get
            {
                return this.Macros["StaticLibraryDir"];
            }

            set
            {
                this.Macros["StaticLibraryDir"] = value;
            }
        }

        /// <summary>
        /// Set or get the directory where Windows import libraries are published.
        /// </summary>
        public Bam.Core.TokenizedString ImportLibraryDir
        {
            get
            {
                return this.Macros["ImportLibraryDir"];
            }

            set
            {
                this.Macros["ImportLibraryDir"] = value;
            }
        }

        /// <summary>
        /// Set or get the directory where plugins are published.
        /// </summary>
        public Bam.Core.TokenizedString PluginDir
        {
            get
            {
                return this.Macros["PluginDir"];
            }

            set
            {
                this.Macros["PluginDir"] = value;
            }
        }

        /// <summary>
        /// Set or get the directory where resource files are published.
        /// </summary>
        public Bam.Core.TokenizedString ResourceDir
        {
            get
            {
                return this.Macros["ResourceDir"];
            }

            set
            {
                this.Macros["ResourceDir"] = value;
            }
        }

        /// <summary>
        /// Set or get the directory where header files are published.
        /// </summary>
        public Bam.Core.TokenizedString HeaderDir
        {
            get
            {
                return this.Macros["HeaderDir"];
            }

            set
            {
                this.Macros["HeaderDir"] = value;
            }
        }

        private void
        SetConsoleApplicationDefaultMacros()
        {
            this.Macros.Add("ExecutableDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
            this.Macros.Add("DynamicLibraryDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
            this.Macros.Add("StaticLibraryDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
            if (Bam.Core.OSUtilities.IsWindowsHosting)
            {
                this.Macros.Add("ImportLibraryDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
            }
            this.Macros.Add("PluginDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
            this.Macros.Add("ResourceDir", this.CreateTokenizedString("$(0)/resources", new[] { this.PublishRoot }));
            this.Macros.Add("HeaderDir", this.CreateTokenizedString("$(0)/include", new[] { this.PublishRoot }));
        }

        private void
        setWindowedApplicationDefaultMacros()
        {
            switch (Bam.Core.OSUtilities.CurrentOS)
            {
                case Bam.Core.EPlatform.Windows:
                    {
                        this.Macros.Add("ExecutableDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
                        this.Macros.Add("DynamicLibraryDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
                        this.Macros.Add("StaticLibraryDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
                        this.Macros.Add("ImportLibraryDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
                        this.Macros.Add("PluginDir", this.CreateTokenizedString("$(0)/plugins", new[] { this.PublishRoot }));
                        this.Macros.Add("ResourceDir", this.CreateTokenizedString("$(0)/resources", new[] { this.PublishRoot }));
                        this.Macros.Add("HeaderDir", this.CreateTokenizedString("$(0)/include", new[] { this.PublishRoot }));
                    }
                    break;

                case Bam.Core.EPlatform.Linux:
                    {
                        this.Macros.Add("ExecutableDir", this.CreateTokenizedString("$(0)/bin", new[] { this.PublishRoot }));
                        this.Macros.Add("DynamicLibraryDir", this.CreateTokenizedString("$(0)/lib", new[] { this.PublishRoot }));
                        this.Macros.Add("StaticLibraryDir", this.CreateTokenizedString("$(0)/lib", new[] { this.PublishRoot }));
                        //this.Macros.Add("ImportLibraryDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
                        this.Macros.Add("PluginDir", this.CreateTokenizedString("$(0)/plugins", new[] { this.PublishRoot }));
                        this.Macros.Add("ResourceDir", this.CreateTokenizedString("$(0)/resources", new[] { this.PublishRoot }));
                        this.Macros.Add("HeaderDir", this.CreateTokenizedString("$(0)/include", new[] { this.PublishRoot }));
                    }
                    break;

                case Bam.Core.EPlatform.OSX:
                    {
                        this.Macros.Add("macOSAppBundleContentsDir", this.CreateTokenizedString("$(0)/$(AnchorOutputName).app/Contents", new[] { this.PublishRoot }));
                        this.Macros.Add("macOSAppBundleMacOSDir", this.CreateTokenizedString("$(macOSAppBundleContentsDir)/MacOS"));
                        this.Macros.Add("macOSAppBundleFrameworksDir", this.CreateTokenizedString("$(macOSAppBundleContentsDir)/Frameworks"));
                        this.Macros.Add("macOSAppBundlePluginsDir", this.CreateTokenizedString("$(macOSAppBundleContentsDir)/Plugins"));
                        this.Macros.Add("macOSAppBundleResourcesDir", this.CreateTokenizedString("$(macOSAppBundleContentsDir)/Resources"));
                        this.Macros.Add("macOSAppBundleSharedSupportDir", this.CreateTokenizedString("$(macOSAppBundleContentsDir)/SharedSupport"));
                        this.Macros.Add("ExecutableDir", this.CreateTokenizedString("$(macOSAppBundleMacOSDir)"));
                        this.Macros.Add("DynamicLibraryDir", this.CreateTokenizedString("$(macOSAppBundleFrameworksDir)"));
                        this.Macros.Add("StaticLibraryDir", this.CreateTokenizedString("$(macOSAppBundleFrameworksDir)"));
                        //this.Macros.Add("ImportLibraryDir", this.CreateTokenizedString("$(macOSAppBundleFrameworksDir)"));
                        this.Macros.Add("PluginDir", this.CreateTokenizedString("$(macOSAppBundlePluginsDir)"));
                        this.Macros.Add("ResourceDir", this.CreateTokenizedString("$(macOSAppBundleResourcesDir)"));
                        this.Macros.Add("HeaderDir", this.CreateTokenizedString("$(macOSAppBundleResourcesDir)/include"));
                    }
                    break;

                default:
                    throw new Bam.Core.Exception("Unsupported OS: '{0}'", Bam.Core.OSUtilities.CurrentOS);
            }
        }

        private void
        setLibraryDefaultMacros()
        {
            this.Macros.Add("ExecutableDir", this.CreateTokenizedString("$(0)/bin", new[] { this.PublishRoot }));
            this.Macros.Add("DynamicLibraryDir", this.CreateTokenizedString("$(0)/bin", new[] { this.PublishRoot }));
            this.Macros.Add("StaticLibraryDir", this.CreateTokenizedString("$(0)/lib", new[] { this.PublishRoot }));
            if (Bam.Core.OSUtilities.CurrentOS == Bam.Core.EPlatform.Windows)
            {
                this.Macros.Add("ImportLibraryDir", this.CreateTokenizedString("$(0)/lib", new[] { this.PublishRoot }));
            }
            this.Macros.Add("HeaderDir", this.CreateTokenizedString("$(0)/include", new[] { this.PublishRoot }));
            this.Macros.Add("PluginDir", this.CreateTokenizedString("$(0)/plugins", new[] { this.PublishRoot }));
            this.Macros.Add("ResourceDir", this.CreateTokenizedString("$(0)/resources", new[] { this.PublishRoot }));
        }

        /// <summary>
        /// Retrieve the application publishing type set in SetDefaultMacrosAndMappings.
        /// </summary>
        /// <value>The type of the publishing.</value>
        public EPublishingType PublishingType
        {
            get;
            protected set;
        }

        /// <summary>
        /// Invoke this function prior to including any modules into a collation, in order to configure defaults
        /// for the standard module publishing properties (e.g. ExecutableDir), and mapping standard C package
        /// module types to locations.
        /// The argument is to create layouts suitable for the type of application being published based on the platform
        /// being built for. For example, a Windows application on macOS will create an Application Bundle, while a Console
        /// application wil use a flat publishing directory structure.
        /// </summary>
        /// <param name="type">Type of application being published.</param>
        public void
        SetDefaultMacrosAndMappings(
            EPublishingType type)
        {
            this.PublishingType = type;

            // TODO: can any of these paths be determined from the C package for RPATHs etc?
            // i.e. whatever layout the user wants, is honoured here as a default
            switch (type)
            {
                case EPublishingType.ConsoleApplication:
                    this.SetConsoleApplicationDefaultMacros();
                    break;

                case EPublishingType.WindowedApplication:
                    this.setWindowedApplicationDefaultMacros();
                    break;

                case EPublishingType.Library:
                    this.setLibraryDefaultMacros();
                    break;
            }

            // order matters here, for any sub-classes of module types
            this.Mapping.Register(typeof(C.Cxx.Plugin), C.Cxx.Plugin.ExecutableKey, this.PluginDir, true);
            this.Mapping.Register(typeof(C.Plugin), C.Plugin.ExecutableKey, this.PluginDir, true);
            this.Mapping.Register(typeof(C.Cxx.DynamicLibrary), C.Cxx.DynamicLibrary.ExecutableKey, this.DynamicLibraryDir, true);
            if (Bam.Core.OSUtilities.IsWindowsHosting)
            {
                this.Mapping.Register(typeof(C.Cxx.DynamicLibrary), C.Cxx.DynamicLibrary.ImportLibraryKey, this.ImportLibraryDir, false);
            }
            this.Mapping.Register(typeof(C.DynamicLibrary), C.DynamicLibrary.ExecutableKey, this.DynamicLibraryDir, true);
            if (Bam.Core.OSUtilities.IsWindowsHosting)
            {
                this.Mapping.Register(typeof(C.DynamicLibrary), C.DynamicLibrary.ImportLibraryKey, this.ImportLibraryDir, false);
            }
            this.Mapping.Register(typeof(C.SharedObjectSymbolicLink), C.SharedObjectSymbolicLink.SOSymLinkKey, this.DynamicLibraryDir, true);
            this.Mapping.Register(typeof(C.Cxx.ConsoleApplication), C.Cxx.ConsoleApplication.ExecutableKey, this.ExecutableDir, true);
            this.Mapping.Register(typeof(C.ConsoleApplication), C.ConsoleApplication.ExecutableKey, this.ExecutableDir, true);
            this.Mapping.Register(typeof(C.StaticLibrary), C.StaticLibrary.LibraryKey, this.StaticLibraryDir, false);
            if (Bam.Core.OSUtilities.IsOSXHosting && EPublishingType.WindowedApplication == type)
            {
                this.Mapping.Register(typeof(C.OSXFramework), C.OSXFramework.FrameworkKey, this.Macros["macOSAppBundleFrameworksDir"], true);
            }
        }

        /// <summary>
        /// Helper function, mostly for package unittests, to be able to automatically find all modules within a specified
        /// namespace, and collate them beside each other.
        /// Each found module will be an anchor. Any shared dependencies should appear once depending on the build mode. IDE
        /// build projects should each have a copy of shared dependencies in order for them to be debuggable.
        /// Only modules that have generated the specified pathkey will be included.
        /// </summary>
        /// <param name="nameSpace">Namespace containing all modules of interest.</param>
        /// <param name="key">String key of the modules that will be collated.</param>
        /// <param name="anchorPublishRoot">Custom publishing root for the anchors. May be null to use default.</param>
        /// <param name="filter">Filter modules matched by name, as a last step. May be null, to include all matching modules.</param>
        public void
        IncludeAllModulesInNamespace(
            string nameSpace,
            string key,
            Bam.Core.TokenizedString anchorPublishRoot = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var genericFindReference = typeof(Bam.Core.Graph).GetMethod("FindReferencedModule", System.Type.EmptyTypes);
            var genericInclude = this.GetType().GetMethod("Include", new[] { typeof(string), typeof(Bam.Core.TokenizedString) });
            var moduleTypes = global::System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(
                item => item.Namespace.Equals(nameSpace, System.StringComparison.Ordinal) &&
                item.IsSubclassOf(typeof(Bam.Core.Module)));
            if (null != filter)
            {
                moduleTypes = moduleTypes.Where(item => filter.IsMatch(item.Name));
            }
            foreach (var type in moduleTypes)
            {
                var findModule = genericFindReference.MakeGenericMethod(new[] { type });
                var module = findModule.Invoke(Bam.Core.Graph.Instance, null) as Bam.Core.Module;
                if (!module.GeneratedPaths.ContainsKey(key))
                {
                    continue;
                }

                var moduleTypeInclude = genericInclude.MakeGenericMethod(new[] { type });
                moduleTypeInclude.Invoke(this, new object[] { key, anchorPublishRoot });
            }
        }

        /// <summary>
        /// Locate a collated module by type. There may be multiple return values, if the search is based on a common sub-class
        /// of modules.
        /// </summary>
        /// <typeparam name="DependentModule">Module type to search for.</typeparam>
        /// <returns>A list of matching collated modules, or an empty list.</returns>
        public Bam.Core.Array<ICollatedObject>
        Find<DependentModule>() where DependentModule : Bam.Core.Module
        {
            var results = new Bam.Core.Array<ICollatedObject>();
            foreach (var dep in this.Requirements)
            {
                var collatedObj = dep as ICollatedObject;
                if (null == collatedObj)
                {
                    // can happen if non-collated objects end up in the requirements, e.g. tools to generate output from collated objects
                    continue;
                }
                if (collatedObj.SourceModule is DependentModule)
                {
                    results.AddUnique(collatedObj);
                }
            }
            return results;
        }

        /// <summary>
        /// Do not collate any modules that match the module type.
        /// </summary>
        /// <typeparam name="DependentModule">Module type to search for, and subsequently ignore from Collation.</typeparam>
        public void
        Ignore<DependentModule>() where DependentModule : Bam.Core.Module
        {
            var matches = this.Find<DependentModule>();
            foreach (var match in matches)
            {
                (match as CollatedObject).Ignore = true;
            }
        }

        private void
        EncodeDependentModuleAndPathKey(
            Bam.Core.Module dependent,
            System.Collections.Generic.Dictionary<Bam.Core.Module, string> allDependents,
            System.Collections.Generic.Queue<System.Tuple<Bam.Core.Module, string>> toDealWith)
        {
            var runtimePathKey = this.Mapping.GetRuntimePathKey(dependent);
            if (null == runtimePathKey)
            {
                // no explicit mapping, but the dependency may contain more dependencies that are
                toDealWith.Enqueue(System.Tuple.Create(dependent, runtimePathKey));
                return;
            }
            if (allDependents.ContainsKey(dependent))
            {
                return;
            }
            toDealWith.Enqueue(System.Tuple.Create(dependent, runtimePathKey));
        }

        private void
        FindPublishableDependents(
            Bam.Core.Module module,
            System.Collections.Generic.Dictionary<Bam.Core.Module, string> allDependents,
            System.Collections.Generic.Queue<System.Tuple<Bam.Core.Module, string>> toDealWith)
        {
            // now look at all the dependencies and accumulate a list of child dependencies
            // TODO: need a configurable list of types, not just C.DynamicLibrary, that the user can specify to find
            foreach (var dep in module.Dependents)
            {
                this.EncodeDependentModuleAndPathKey(dep, allDependents, toDealWith);
            }
            foreach (var req in module.Requirements)
            {
                this.EncodeDependentModuleAndPathKey(req, allDependents, toDealWith);
            }
        }

        private void
        gatherAllDependencies(
            Bam.Core.Module initialModule,
            string key,
            ICollatedObject anchor,
            Bam.Core.TokenizedString anchorPublishRoot)
        {
            var allDependents = new System.Collections.Generic.Dictionary<Bam.Core.Module, string>();
            var toDealWith = new System.Collections.Generic.Queue<System.Tuple<Bam.Core.Module, string>>();
            toDealWith.Enqueue(System.Tuple.Create(initialModule, key));
            // iterate over each dependent, stepping into each of their dependencies
            while (toDealWith.Count > 0)
            {
                var next = toDealWith.Dequeue();
                this.FindPublishableDependents(next.Item1, allDependents, toDealWith);
                if (next.Item1 == initialModule)
                {
                    continue;
                }
                if (next.Item2.Equals(default(string), System.StringComparison.Ordinal))
                {
                    Bam.Core.Log.DebugMessage("Ignoring '{0}' for collation, with no string path key", next.Item1.ToString());
                    continue;
                }
                var moduleShouldBePublished = true;
                moduleShouldBePublished &= !allDependents.ContainsKey(next.Item1);
                moduleShouldBePublished &= !this.collatedObjects.ContainsKey(next);
                if (anchor != null)
                {
                    var anchorAsCollatedObject = anchor as CollatedObject;
                    moduleShouldBePublished &= !anchorAsCollatedObject.DependentCollations.ContainsKey(next);
                }
                if (moduleShouldBePublished)
                {
                    allDependents.Add(next.Item1, next.Item2);
                }
            }
            // now add each as a publishable dependent
            foreach (var dep in allDependents)
            {
                var modulePublishDir = this.Mapping.FindPublishDirectory(dep.Key, dep.Value);
                var dependentCollation = this.IncludeNoGather(dep.Key, dep.Value, modulePublishDir, anchor, anchorPublishRoot);

                // dependents might reference the anchor's OutputName macro, e.g. dylibs copied into an application bundle
                (dependentCollation as CollatedObject).Macros.Add("AnchorOutputName", (anchor as CollatedObject).Macros["AnchorOutputName"]);
            }
        }

        struct ModuleOutputDefaultPublishingPath
        {
            public System.Type type;
            public string pathKey;
            public Bam.Core.TokenizedString defaultPublishPath;
            public bool runtimeDependency;

            public ModuleOutputDefaultPublishingPath(
                System.Type moduleType,
                string modulePathKey,
                Bam.Core.TokenizedString defaultPublishPath,
                bool isRuntimeDependency)
            {
                this.type = moduleType;
                this.pathKey = modulePathKey;
                this.defaultPublishPath = defaultPublishPath;
                this.runtimeDependency = isRuntimeDependency;
            }
        }

        /// <summary>
        /// Mapping of module to publishing directory.
        /// </summary>
        public class ModuleOutputDefaultPublishingPathMapping
        {
            private Bam.Core.Array<ModuleOutputDefaultPublishingPath> mapping = new Bam.Core.Array<ModuleOutputDefaultPublishingPath>();

            /// <summary>
            /// Register a new module and PathKey to a location.
            /// </summary>
            /// <param name="moduleType">Type of the module of interest.</param>
            /// <param name="modulePathKey">String key of the module of interest.</param>
            /// <param name="defaultPublishPath">The default location to which to publish.</param>
            /// <param name="isRuntimeDependency">Is the module a runtime dependency? Such things that are not are static and import libraries.</param>
            public void
            Register(
                System.Type moduleType,
                string modulePathKey,
                Bam.Core.TokenizedString defaultPublishPath,
                bool isRuntimeDependency)
            {
                this.mapping.Add(new ModuleOutputDefaultPublishingPath(moduleType, modulePathKey, defaultPublishPath, isRuntimeDependency));
            }

            /// <summary>
            /// Retrieve the publishing directory for the module and PathKey pair.
            /// </summary>
            /// <param name="module">Module of interest.</param>
            /// <param name="modulePathKey">String key of the module of interest.</param>
            /// <returns>The publishing directory registered for the pair, or an exception is thrown.</returns>
            public Bam.Core.TokenizedString
            FindPublishDirectory(
                Bam.Core.Module module,
                string modulePathKey)
            {
                foreach (var mod in this.mapping)
                {
                    if (!mod.type.IsInstanceOfType(module))
                    {
                        continue;
                    }
                    if (null == mod.pathKey)
                    {
                        return mod.defaultPublishPath;
                    }
                    if (modulePathKey.Equals(mod.pathKey, System.StringComparison.Ordinal))
                    {
                        return mod.defaultPublishPath;
                    }
                }
                throw new Bam.Core.Exception("Unable to identify a publish directory for module {0} with path key {1}", module.ToString(), modulePathKey.ToString());
            }

            /// <summary>
            /// For the module type, retrieve the PathKey registered with it, if the registration is for a runtime dependency.
            /// </summary>
            /// <param name="module">Module of interest</param>
            /// <returns>String Key registered with the module, or null if there is not one registered.</returns>
            public string
            GetRuntimePathKey(
                Bam.Core.Module module)
            {
                foreach (var mod in this.mapping)
                {
                    if (!mod.type.IsInstanceOfType(module))
                    {
                        continue;
                    }
                    if (!mod.runtimeDependency)
                    {
                        continue;
                    }
                    return mod.pathKey;
                }
                Bam.Core.Log.DebugMessage("Unable to locate collation mapping for modules of type '{0}'", module.GetType().ToString());
                return null;
            }
        }

        /// <summary>
        /// Access to the mapping from module to default publish path.
        /// </summary>
        public ModuleOutputDefaultPublishingPathMapping Mapping
        {
            get;
            private set;
        }

        private void
        recordCollatedObject(
            ICollatedObject collatedFile,
            Bam.Core.Module dependent,
            string key,
            ICollatedObject anchor)
        {
            var tuple = System.Tuple.Create(dependent, key);
            try
            {
                if (Bam.Core.Graph.Instance.BuildModeMetaData.PublishBesideExecutable)
                {
                    // a dependency may be copied for each anchor that references it in order
                    // to make that anchor fully resolved and debuggable
                    if (null != anchor)
                    {
                        var anchorAsCollatedObject = anchor as CollatedObject;
                        anchorAsCollatedObject.DependentCollations.Add(tuple, collatedFile);
                    }
                    else
                    {
                        this.collatedObjects.Add(tuple, collatedFile);
                    }
                }
                else
                {
                    // as everything goes to a single publishdir, remember each instance of a module
                    this.collatedObjects.Add(tuple, collatedFile);
                }
            }
            catch (System.ArgumentException)
            {
                var message = new System.Text.StringBuilder();
                message.AppendFormat("Module {0} with path key {1} has already been added for collation", dependent.ToString(), key.ToString());
                message.AppendLine();
                message.AppendLine("Please use Collation.Find<ModuleType>() in order to modify any of it's traits.");
                throw new Bam.Core.Exception(message.ToString());
            }
        }

        private ICollatedObject
        IncludeNoGather(
            Bam.Core.Module dependent,
            string key,
            Bam.Core.TokenizedString modulePublishDir,
            ICollatedObject anchor,
            Bam.Core.TokenizedString anchorPublishRoot)
        {
            ICollatedObject collatedFile;
            if (dependent is C.OSXFramework)
            {
                collatedFile = this.CreateCollatedModuleGeneratedOSXFramework(
                    dependent,
                    key,
                    modulePublishDir,
                    anchor,
                    anchorPublishRoot
                );
            }
            else if (dependent is Bam.Core.ICommandLineTool)
            {
                collatedFile = this.CreateCollatedModuleGeneratedFile<CollatedCommandLineTool>(
                    dependent,
                    key,
                    modulePublishDir,
                    anchor,
                    anchorPublishRoot
                );
            }
            else
            {
                collatedFile = this.CreateCollatedModuleGeneratedFile<CollatedFile>(
                    dependent,
                    key,
                    modulePublishDir,
                    anchor,
                    anchorPublishRoot
                );
            }
            this.recordCollatedObject(
                collatedFile,
                dependent,
                key,
                anchor
            );
            return collatedFile;
        }

        private ICollatedObject
        Include(
            Bam.Core.Module dependent,
            string key,
            Bam.Core.TokenizedString anchorPublishRoot)
        {
            var modulePublishDir = this.Mapping.FindPublishDirectory(dependent, key);
            var collatedFile = this.IncludeNoGather(dependent, key, modulePublishDir, null, anchorPublishRoot);
            (collatedFile as Bam.Core.Module).Macros.Add("AnchorOutputName", dependent.Macros["OutputName"]);
            this.gatherAllDependencies(dependent, key, collatedFile, anchorPublishRoot);
            return collatedFile;
        }

        /// <summary>
        /// Include, as an anchor, the module of the specified type with the PathKey an output from that module.
        /// </summary>
        /// <typeparam name="DependentModule">Module type to become an anchor.</typeparam>
        /// <param name="key">String key as an output of that module instance to collate.</param>
        /// <param name="anchorPublishRoot">Custom directory to use as the root for the anchor's publishing, or null to use the default.</param>
        /// <returns></returns>
        public ICollatedObject
        Include<DependentModule>(
            string key,
            Bam.Core.TokenizedString anchorPublishRoot = null) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return null;
            }
            return this.Include(dependent, key, anchorPublishRoot);
        }

        /// <summary>
        /// Include additional files (non-module based preexisting files).
        /// </summary>
        /// <param name="wildcardedSourcePath">Wildcarded path to all matching files.</param>
        /// <param name="destinationDir">Destination directory in which to publish files.</param>
        /// <param name="anchor">Anchor associated with files (used in IDE projects).</param>
        /// <param name="filter">Optional regular expression to filter the expanded wildcarded path.</param>
        /// <returns>List containing each collated file.</returns>
        public Bam.Core.Array<ICollatedObject>
        IncludeFiles(
            Bam.Core.TokenizedString wildcardedSourcePath,
            Bam.Core.TokenizedString destinationDir,
            ICollatedObject anchor,
            System.Text.RegularExpressions.Regex filter = null)
        {
            // Note: very similar to that code in C.CModuleContainer.AddFiles
            if (!wildcardedSourcePath.IsParsed)
            {
                wildcardedSourcePath.Parse();
            }
            var wildcardPaths = wildcardedSourcePath.ToString();
            var dir = System.IO.Path.GetDirectoryName(wildcardPaths);
            if (!System.IO.Directory.Exists(dir))
            {
                throw new Bam.Core.Exception("The directory {0} does not exist", dir);
            }
            var leafname = System.IO.Path.GetFileName(wildcardPaths);
            var option = leafname.Contains("**") ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;
            var files = System.IO.Directory.GetFiles(dir, leafname, option);
            if (0 == files.Length)
            {
                throw new Bam.Core.Exception("No files were found that matched the pattern '{0}'", wildcardPaths);
            }
            if (filter != null)
            {
                var filteredFiles = files.Where(pathname => filter.IsMatch(pathname)).ToArray();
                if (0 == filteredFiles.Length)
                {
                    throw new Bam.Core.Exception("No files were found that matched the pattern '{0}', after applying the regex filter. {1} were found prior to applying the filter.", wildcardPaths, files.Count());
                }
                files = filteredFiles;
            }
            var results = new Bam.Core.Array<ICollatedObject>();
            foreach (var filepath in files)
            {
                results.Add(this.CreateCollatedPreExistingFile(filepath, destinationDir, anchor));
            }
            return results;
        }

        /// <summary>
        /// Include additional files (non-module based preexisting files).
        /// </summary>
        /// <param name="wildcardedSourcePaths">Array of wildcarded path to all matching files.</param>
        /// <param name="destinationDir">Destination directory in which to publish files.</param>
        /// <param name="anchor">Anchor associated with files (used in IDE projects).</param>
        /// <param name="filter">Optional regular expression to filter the expanded wildcarded path.</param>
        /// <returns>List containing each collated file.</returns>
        public Bam.Core.Array<ICollatedObject>
        IncludeFiles(
            Bam.Core.TokenizedStringArray wildcardedSourcePaths,
            Bam.Core.TokenizedString destinationDir,
            ICollatedObject anchor,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var results = new Bam.Core.Array<ICollatedObject>();
            foreach (var path in wildcardedSourcePaths)
            {
                results.AddRange(this.IncludeFiles(path, destinationDir, anchor, filter));
            }
            return results;
        }

        /// <summary>
        /// Include additional files (non-module based preexisting files).
        /// </summary>
        /// <typeparam name="DependentModule">Module type that the path may use macros from.</typeparam>
        /// <param name="wildcardedSourcePath">Wildcarded path to all matching files.</param>
        /// <param name="destinationDir">Destination directory in which to publish files.</param>
        /// <param name="anchor">Anchor associated with files (used in IDE projects).</param>
        /// <param name="filter">Optional regular expression to filter the expanded wildcarded path.</param>
        /// <returns>List containing each collated file.</returns>
        public Bam.Core.Array<ICollatedObject>
        IncludeFiles<DependentModule>(
            string wildcardedSourcePath,
            Bam.Core.TokenizedString destinationDir,
            ICollatedObject anchor,
            System.Text.RegularExpressions.Regex filter = null) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return null;
            }
            return this.IncludeFiles(dependent.CreateTokenizedString(wildcardedSourcePath), destinationDir, anchor, filter);
        }

        /// <summary>
        /// Include additional directories into the collation.
        /// </summary>
        /// <param name="wildcardedSourcePath">Wildcarded path to all matching directories.</param>
        /// <param name="destinationDir">Destination directory in which to publish directories.</param>
        /// <param name="anchor">Anchor associated with directories (used in IDE projects).</param>
        /// <param name="filter">Optional regular expression to filter the expanded wildcarded path.</param>
        /// <param name="renameLeaf">Optional rename of the top-level directory upon collation.</param>
        /// <returns>List containing each collated directory.</returns>
        public Bam.Core.Array<ICollatedObject>
        IncludeDirectories(
            Bam.Core.TokenizedString wildcardedSourcePath,
            Bam.Core.TokenizedString destinationDir,
            ICollatedObject anchor,
            System.Text.RegularExpressions.Regex filter = null,
            string renameLeaf = null)
        {
            // Note: very similar to that code in C.CModuleContainer.AddFiles
            if (!wildcardedSourcePath.IsParsed)
            {
                wildcardedSourcePath.Parse();
            }
            var wildcardPaths = wildcardedSourcePath.ToString();
            var dir = System.IO.Path.GetDirectoryName(wildcardPaths);
            if (!System.IO.Directory.Exists(dir))
            {
                throw new Bam.Core.Exception("The directory {0} does not exist", dir);
            }
            var leafname = System.IO.Path.GetFileName(wildcardPaths);
            var option = leafname.Contains("**") ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;
            var files = System.IO.Directory.GetDirectories(dir, leafname, option);
            if (0 == files.Length)
            {
                throw new Bam.Core.Exception("No files were found that matched the pattern '{0}'", wildcardPaths);
            }
            if (filter != null)
            {
                var filteredFiles = files.Where(pathname => filter.IsMatch(pathname)).ToArray();
                if (0 == filteredFiles.Length)
                {
                    throw new Bam.Core.Exception("No files were found that matched the pattern '{0}', after applying the regex filter. {1} were found prior to applying the filter.", wildcardPaths, files.Count());
                }
                files = filteredFiles;
            }
            var results = new Bam.Core.Array<ICollatedObject>();
            foreach (var filepath in files)
            {
                results.Add(this.CreateCollatedPreExistingDirectory(filepath, destinationDir, anchor, renameLeaf));
            }
            return results;
        }

        /// <summary>
        /// Include additional directories into the collation.
        /// </summary>
        /// <param name="wildcardedSourcePaths">Wildcarded paths to all matching directories.</param>
        /// <param name="destinationDir">Destination directory in which to publish directories.</param>
        /// <param name="anchor">Anchor associated with directories (used in IDE projects).</param>
        /// <param name="filter">Optional regular expression to filter the expanded wildcarded path.</param>
        /// <param name="renameLeaf">Optional rename of the top-level directory upon collation.</param>
        /// <returns>List containing each collated directory.</returns>
        public Bam.Core.Array<ICollatedObject>
        IncludeDirectories(
            Bam.Core.TokenizedStringArray wildcardedSourcePaths,
            Bam.Core.TokenizedString destinationDir,
            ICollatedObject anchor,
            System.Text.RegularExpressions.Regex filter = null,
            string renameLeaf = null)
        {
            var results = new Bam.Core.Array<ICollatedObject>();
            foreach (var path in wildcardedSourcePaths)
            {
                results.AddRange(this.IncludeDirectories(path, destinationDir, anchor, filter, renameLeaf));
            }
            return results;
        }

        /// <summary>
        /// Include additional directories into the collation.
        /// </summary>
        /// <typeparam name="DependentModule">Module type in which the provided path can use macros from.</typeparam>
        /// <param name="wildcardedSourcePath">Wildcarded path to all matching directories.</param>
        /// <param name="destinationDir">Destination directory in which to publish directories.</param>
        /// <param name="anchor">Anchor associated with directories (used in IDE projects).</param>
        /// <param name="filter">Optional regular expression to filter the expanded wildcarded path.</param>
        /// <param name="renameLeaf">Optional rename of the top-level directory upon collation.</param>
        /// <returns>List containing each collated directory.</returns>
        public Bam.Core.Array<ICollatedObject>
        IncludeDirectories<DependentModule>(
            string wildcardedSourcePath,
            Bam.Core.TokenizedString destinationDir,
            ICollatedObject anchor,
            System.Text.RegularExpressions.Regex filter = null,
            string renameLeaf = null) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return null;
            }
            return this.IncludeDirectories(dependent.CreateTokenizedString(wildcardedSourcePath), destinationDir, anchor, filter, renameLeaf);
        }

        private ICollatedObject
        CreateCollatedPreExistingFile(
            string sourcePath,
            Bam.Core.TokenizedString destinationDir,
            ICollatedObject anchor)
        {
            var preexisting = Bam.Core.Module.Create<PreExistingFile>(
                preInitCallback: module =>
                {
                    module.Macros.Add("ExistingFile", sourcePath);
                }
            );
            var collatedFile = this.CreateCollatedModuleGeneratedFile<CollatedFile>(
                preexisting,
                PreExistingFile.ExistingFileKey,
                destinationDir,
                anchor,
                null
            );
            this.recordCollatedObject(
                collatedFile,
                preexisting,
                PreExistingFile.ExistingFileKey,
                anchor
            );
            if (null != anchor)
            {
                // dependents might reference the anchor's OutputName macro, e.g. dylibs copied into an application bundle
                (collatedFile as CollatedObject).Macros.Add(
                    "AnchorOutputName",
                    (anchor as Bam.Core.Module).Macros["AnchorOutputName"]
                );
            }
            return collatedFile;
        }

        private ICollatedObject
        CreateCollatedPreExistingDirectory(
            string sourcePath,
            Bam.Core.TokenizedString destinationDir,
            ICollatedObject anchor,
            string renameLeaf)
        {
            var preexisting = Bam.Core.Module.Create<PreExistingDirectory>(
                preInitCallback: module =>
                {
                    module.Macros.Add("ExistingDirectory", sourcePath);
                }
            );
            var collatedDir = this.CreateCollatedModuleGeneratedFile<CollatedDirectory>(
                preexisting,
                PreExistingDirectory.ExistingDirectoryKey,
                destinationDir,
                anchor,
                null,
                renameLeaf: renameLeaf
            );
            this.recordCollatedObject(
                collatedDir,
                preexisting,
                PreExistingDirectory.ExistingDirectoryKey,
                anchor
            );
            if (null != anchor)
            {
                // dependents might reference the anchor's OutputName macro, e.g. dylibs copied into an application bundle
                (collatedDir as CollatedObject).Macros.Add(
                    "AnchorOutputName",
                    (anchor as Bam.Core.Module).Macros["AnchorOutputName"]
                );
            }
            return collatedDir;
        }

        private CollatedObject
        CreateCollatedModuleGeneratedFile<CollationType>(
            Bam.Core.Module dependent,
            string key,
            Bam.Core.TokenizedString modulePublishDir,
            ICollatedObject anchor,
            Bam.Core.TokenizedString anchorPublishRoot,
            string renameLeaf = null) where CollationType: CollatedObject, new()
        {
            var collatedFile = Bam.Core.Module.Create<CollationType>(preInitCallback: module =>
                {
                    module.SourceModule = dependent;
                    module.SourcePathKey = key;
                    module.SetPublishingDirectory("$(0)", new[] { modulePublishDir });
                    module.Anchor = anchor;
                    if (null != renameLeaf)
                    {
                        module.Macros.AddVerbatim("RenameLeaf", renameLeaf);
                    }
                });
            if (Bam.Core.Graph.Instance.BuildModeMetaData.PublishBesideExecutable)
            {
                // the publishdir is different for each anchor, so dependents may be duplicated
                // if referenced by multiple anchors
                if (null != anchor)
                {
                    collatedFile.Macros.Add("publishroot", collatedFile.CreateTokenizedString("@dir($(0))", new[] { (anchor as CollatedObject).SourcePath }));
                }
                else
                {
                    collatedFile.Macros.Add("publishroot", collatedFile.CreateTokenizedString("@dir($(0))", new[] { collatedFile.SourcePath }));
                }
            }
            else
            {
                // publishdir is the same for all anchors, and thus all dependents are unique for all anchors
                collatedFile.Macros.Add("publishroot", this.CreateTokenizedString("$(buildroot)/$(modulename)-$(config)"));
            }

            // for PublishBesideExecutable, a custom anchor publish root won't work, as the debugger won't run any file
            // other than that built, and if copied elsewhere (as would expect from a custom anchor publish root), so
            // will it's dependencies, which won't be found by the debugged executable
            if (null != anchorPublishRoot && !Bam.Core.Graph.Instance.BuildModeMetaData.PublishBesideExecutable)
            {
                collatedFile.Macros.Add("publishdir", collatedFile.CreateTokenizedString("$(0)", anchorPublishRoot));
            }
            else
            {
                collatedFile.Macros.Add("publishdir", collatedFile.CreateTokenizedString("$(publishroot)"));
            }

            this.Requires(collatedFile);
            return collatedFile;
        }

        private ICollatedObject
        CreateCollatedModuleGeneratedOSXFramework(
            Bam.Core.Module dependent,
            string key,
            Bam.Core.TokenizedString modulePublishDir,
            ICollatedObject anchor,
            Bam.Core.TokenizedString anchorPublishRoot)
        {
            var collatedFramework = Bam.Core.Module.Create<CollatedOSXFramework>(preInitCallback: module =>
                {
                    module.SourceModule = dependent;
                    module.SourcePathKey = key;
                    module.SetPublishingDirectory("$(0)", new[] { modulePublishDir });
                    module.Anchor = anchor;
                });

            if (Bam.Core.Graph.Instance.BuildModeMetaData.PublishBesideExecutable)
            {
                // the publishdir is different for each anchor, so dependents may be duplicated
                // if referenced by multiple anchors
                if (null != anchor)
                {
                    collatedFramework.Macros.Add("publishroot", collatedFramework.CreateTokenizedString("@dir($(0))", new[] { (anchor as CollatedObject).SourcePath }));
                }
                else
                {
                    collatedFramework.Macros.Add("publishroot", collatedFramework.CreateTokenizedString("@dir($(0))", new[] { collatedFramework.SourcePath }));
                }
            }
            else
            {
                // publishdir is the same for all anchors, and thus all dependents are unique for all anchors
                collatedFramework.Macros.Add("publishroot", this.CreateTokenizedString("$(buildroot)/$(modulename)-$(config)"));
            }

            // for PublishBesideExecutable, a custom anchor publish root won't work, as the debugger won't run any file
            // other than that built, and if copied elsewhere (as would expect from a custom anchor publish root), so
            // will it's dependencies, which won't be found by the debugged executable
            if (null != anchorPublishRoot && !Bam.Core.Graph.Instance.BuildModeMetaData.PublishBesideExecutable)
            {
                collatedFramework.Macros.Add("publishdir", collatedFramework.CreateTokenizedString("$(0)", anchorPublishRoot));
            }
            else
            {
                collatedFramework.Macros.Add("publishdir", collatedFramework.CreateTokenizedString("$(publishroot)"));
            }

            this.Requires(collatedFramework);
            return collatedFramework;
        }

        /// <summary>
        /// Delegate type used for iterating over anchors.
        /// </summary>
        /// <param name="collation">The Collation owning the Anchors</param>
        /// <param name="anchor">Current Anchor</param>
        /// <param name="customData">Any custom data.</param>
        public delegate void ForEachAnchorDelegate(Collation collation, ICollatedObject anchor, object customData);

        /// <summary>
        /// Iterate over all anchors using the specified delegate and passing the specified custom data.
        /// </summary>
        /// <param name="anchorDelegate">Delegate to use for each anchor.</param>
        /// <param name="customData">Optional custom data to pass into the delegate for each anchor.</param>
        public void
        ForEachAnchor(
            ForEachAnchorDelegate anchorDelegate,
            object customData)
        {
            foreach (var obj in this.collatedObjects)
            {
                var collatedObjectInterface = obj.Value as ICollatedObject;
                if (null == collatedObjectInterface.Anchor)
                {
                    anchorDelegate(this, collatedObjectInterface, customData);
                }
            }
            foreach (var obj in this.preExistingCollatedObjects)
            {
                // TODO: this is slightly more expensive, as it may end up iterating over
                // this.collatedObjects for the preexisting objects
                anchorDelegate(this, obj.Item1, customData);
            }
        }

        /// <summary>
        /// Delegate executed on each collated object.
        /// </summary>
        /// <param name="collatedObj">Current collated object.</param>
        /// <param name="customData">Optional custom data.</param>
        public delegate void ForEachCollatedObjectDelegate(ICollatedObject collatedObj, object customData);

        /// <summary>
        /// Helper function to iterate over each collated object associated with the anchor.
        /// </summary>
        /// <param name="anchor">Anchor to iterate over all collated objects it is associated with.</param>
        /// <param name="collatedObjectDelegate">Delegate to execute on each collated object.</param>
        /// <param name="customData">Optional custom data to pass to each delegate.</param>
        public void
        ForEachCollatedObjectFromAnchor(
            ICollatedObject anchor,
            ForEachCollatedObjectDelegate collatedObjectDelegate,
            object customData)
        {
            collatedObjectDelegate(anchor, customData);
            foreach (var obj in this.collatedObjects)
            {
                var collatedObjectInterface = obj.Value as ICollatedObject;
                if (anchor == collatedObjectInterface.Anchor)
                {
                    collatedObjectDelegate(collatedObjectInterface, customData);
                }
            }
        }
    }
}
