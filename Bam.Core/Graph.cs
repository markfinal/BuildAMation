#region License
// Copyright (c) 2010-2016, Mark Final
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
    /// Singleton representing the single point of reference for all build functionality.
    /// This can be thought about as a layer on top of the DependencyGraph.
    /// </summary>
    public sealed class Graph :
        System.Collections.Generic.IEnumerable<ModuleCollection>
    {
        private string TheBuildRoot;

        static Graph()
        {
            Instance = new Graph();
            Instance.Initialize();
        }

        /// <summary>
        /// Obtain the singleton instance of the Graph.
        /// </summary>
        /// <value>Singleton instance.</value>
        public static Graph Instance
        {
            get;
            private set;
        }

        private void
        Initialize()
        {
            this.ProcessState = new BamState();

            OSUtilities.SetupPlatform();

            this.Modules = new System.Collections.Generic.Dictionary<Environment, System.Collections.Generic.List<Module>>();
            this.ReferencedModules = new System.Collections.Generic.Dictionary<Environment, System.Collections.Generic.List<Module>>();
            this.TopLevelModules = new System.Collections.Generic.List<Module>();
            this.Macros = new MacroList();
            this.BuildEnvironmentInternal = null;
            this.CommonModuleType = new System.Collections.Generic.Stack<System.Type>();
            this.DependencyGraph = new DependencyGraph();
            this.MetaData = null;

            this.PackageRepositories = new StringArray();
            var primaryPackageRepo = System.IO.Path.Combine(
                System.IO.Directory.GetParent(System.IO.Directory.GetParent(this.ProcessState.ExecutableDirectory).FullName).FullName,
                "packages");
            this.PackageRepositories.AddUnique(primaryPackageRepo);

            this.ForceDefinitionFileUpdate = CommandLineProcessor.Evaluate(new Options.ForceDefinitionFileUpdate());
            this.CompileWithDebugSymbols = CommandLineProcessor.Evaluate(new Options.UseDebugSymbols());
        }

        /// <summary>
        /// Add the module to the flat list of all modules in the current build environment.
        /// </summary>
        /// <param name="module">Module to be added</param>
        public void
        AddModule(
            Module module)
        {
            this.Modules[this.BuildEnvironmentInternal].Add(module);
        }

        /// <summary>
        /// Stack of module types, that are pushed when a new module is created, and popped post-creation.
        /// This is so that modules created as dependencies can inspect their module parental hierarchy at construction time.
        /// </summary>
        /// <value>The stack of module types</value>
        public System.Collections.Generic.Stack<System.Type> CommonModuleType
        {
            get;
            private set;
        }

        /// <summary>
        /// A referenced module is one that is referenced by it's class type. This is normally in use when specifying
        /// a dependency. There can be one and only one copy, in a build environment, of this type of module.
        /// A non-referenced module, is one that is never referred to explicitly in user scripts, but are created behind
        /// the scenes by packages. There can be many instances of these modules.
        /// The graph maintains a list of all referenced modules
        /// </summary>
        /// <returns>The instance of the referenced module.</returns>
        /// <typeparam name="T">The type of module being referenced.</typeparam>
        public T
        FindReferencedModule<T>() where T : Module, new()
        {
            if (null == this.BuildEnvironmentInternal)
            {
                throw new Exception("Unable to find a module within a patch - please change the calling code to invoke this outside of the patch, but in the module's Init method");
            }
            var referencedModules = this.ReferencedModules[this.BuildEnvironmentInternal];
            var matches = referencedModules.Where(item => item.GetType() == typeof(T));
            var matchedModule = matches.FirstOrDefault();
            if (null != matchedModule)
            {
                return matchedModule as T;
            }
            this.CommonModuleType.Push(typeof(T));
            var newModule = Module.Create<T>(preInitCallback: module =>
                {
                    if (null != module)
                    {
                        referencedModules.Add(module);
                    }
                });
            this.CommonModuleType.Pop();
            return newModule;
        }

        private Module
        MakeModuleOfType(
            System.Type moduleType)
        {
            try
            {
                var findReferencedModuleMethod = typeof(Graph).GetMethod("FindReferencedModule");
                var genericVersionForModuleType = findReferencedModuleMethod.MakeGenericMethod(moduleType);
                var newModule = genericVersionForModuleType.Invoke(this, null) as Module;
                return newModule;
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                var exModuleType = (ex.InnerException is ModuleCreationException) ? (ex.InnerException as ModuleCreationException).ModuleType : moduleType;
                var inner = ex.InnerException;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }
                throw new Exception(inner, "Failed to create module of type {0}", exModuleType.ToString());
            }
        }

        /// <summary>
        /// Create an unreferenced module instance of the specified type.
        /// </summary>
        /// <returns>The module of type.</returns>
        /// <param name="moduleType">Module type.</param>
        /// <typeparam name="ModuleType">The 1st type parameter.</typeparam>
        public ModuleType
        MakeModuleOfType<ModuleType>(
            System.Type moduleType) where ModuleType : Module
        {
            return this.MakeModuleOfType(moduleType) as ModuleType;
        }

        /// <summary>
        /// Create all modules in the top level namespace, which is the namespace of the package in which Bam is invoked.
        /// </summary>
        /// <param name="assembly">Package assembly</param>
        /// <param name="env">Environment to create the modules for.</param>
        /// <param name="ns">Namespace of the package in which Bam is invoked.</param>
        public void
        CreateTopLevelModules(
            System.Reflection.Assembly assembly,
            Environment env,
            string ns)
        {
            this.BuildEnvironment = env;
            var includeTests = CommandLineProcessor.Evaluate(new Options.UseTests());
            var allTypes = assembly.GetTypes();
            var allPackageTypes = allTypes.Where(type => ((type.Namespace == ns) || (includeTests && (type.Namespace == ns + ".tests"))) && type.IsSubclassOf(typeof(Module)) && type.IsSealed);
            foreach (var moduleType in allPackageTypes)
            {
                var newModule = MakeModuleOfType(moduleType);
                if (newModule != null)
                {
                    this.TopLevelModules.Add(newModule);
                }
            }
            this.BuildEnvironment = null;
            if (0 == this.TopLevelModules.Count)
            {
                throw new Exception("No modules found in the namespace '{0}'", ns);
            }
            // remove all top level modules that have a reference count > 1
            foreach (var tlm in this.TopLevelModules.Reverse<Module>())
            {
                if (!tlm.TopLevel)
                {
                    this.TopLevelModules.Remove(tlm);
                }
            }
        }

        /// <summary>
        /// Apply any patches associated with modules.
        /// </summary>
        public void
        ApplySettingsPatches()
        {
            Log.Detail("Apply settings to modules");
            foreach (var rank in this.DependencyGraph.Reverse())
            {
                foreach (var module in rank.Value)
                {
                    module.ApplySettingsPatches();
                }
            }
        }

        private System.Collections.Generic.List<Module> TopLevelModules
        {
            get;
            set;
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

        /// <summary>
        /// Obtain the build mode.
        /// </summary>
        /// <value>The mode.</value>
        public string Mode
        {
            get;
            set;
        }

        /// <summary>
        /// Obtain global macros.
        /// </summary>
        /// <value>The macros.</value>
        public MacroList Macros
        {
            get;
            private set;
        }

        /// <summary>
        /// Get or set metadata associated with the Graph. This is used for multi-threaded builds.
        /// </summary>
        /// <value>The meta data.</value>
        public object MetaData
        {
            get;
            set;
        }

        private Environment BuildEnvironmentInternal = null;
        /// <summary>
        /// Get the current build environment.
        /// </summary>
        /// <value>The build environment.</value>
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
                }
            }
        }

        /// <summary>
        /// Get the actual graph of module dependencies.
        /// </summary>
        /// <value>The dependency graph.</value>
        public DependencyGraph DependencyGraph
        {
            get;
            private set;
        }

        private void ApplyGroupDependenciesToChildren(
            System.Collections.ObjectModel.ReadOnlyCollection<Module> children,
            System.Collections.Generic.IEnumerable<Module> dependencies)
        {
            var nonChildDependents = dependencies.Where(item => !(item is IChildModule));
            foreach (var c in children)
            {
                c.DependsOn(nonChildDependents);
            }
        }

        private void ApplyGroupRequirementsToChildren(
            System.Collections.ObjectModel.ReadOnlyCollection<Module> children,
            System.Collections.Generic.IEnumerable<Module> dependencies)
        {
            var nonChildDependents = dependencies.Where(item => !(item is IChildModule));
            foreach (var c in children)
            {
                c.Requires(nonChildDependents);
            }
        }

        private void
        InternalArrangeDependents(
            Module m,
            int rank)
        {
            // predicate required, because eventually there will be a module without a Tool, e.g. a Tool itself
            if (m.Tool != null)
            {
                if (null == m.Settings)
                {
                    m.Requires(m.Tool);
                    var child = m as IChildModule;
                    if ((null == child) || (null == child.Parent))
                    {
                        // children inherit the settings from their parents
                        m.UsePublicPatches(m.Tool);
                    }
                    try
                    {
                        m.Settings = (m.Tool as ITool).CreateDefaultSettings(m);
                    }
                    catch (System.TypeInitializationException ex)
                    {
                        throw ex.InnerException;
                    }
                }
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

        /// <summary>
        /// Sort all dependencies, invoking Init functions, creating all additional dependencies, placing
        /// modules into their correct rank.
        /// Settings classes are also created and set to default property values if modules have a Tool associated with them.
        /// </summary>
        public void
        SortDependencies()
        {
            Log.Detail("Analysing module dependencies");
            var currentRank = this.DependencyGraph[0];
            foreach (var m in this.TopLevelModules)
            {
                currentRank.Add(m);
                this.InternalArrangeDependents(m, 0);
            }
            Module.CompleteModules();
        }

        /// <summary>
        /// Dump a representation of the dependency graph to the console.
        /// </summary>
        public void
        Dump()
        {
            Log.Message(this.VerbosityLevel, new string('*', 80));
            Log.Message(this.VerbosityLevel, "{0,50}", "DEPENDENCY GRAPH VIEW");
            Log.Message(this.VerbosityLevel, new string('*', 80));
            foreach (var rank in this.DependencyGraph)
            {
                var text = new System.Text.StringBuilder();
                text.AppendFormat("{2}Rank {0}: {1} modules{2}", rank.Key, rank.Value.Count(), System.Environment.NewLine);
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
                Log.Message(this.VerbosityLevel, text.ToString());
            }
            Log.Message(this.VerbosityLevel, new string('*', 80));
            Log.Message(this.VerbosityLevel, "{0,50}", "END DEPENDENCY GRAPH VIEW");
            Log.Message(this.VerbosityLevel, new string('*', 80));
        }

        private void
        InternalValidateGraph(
            int parentRank,
            System.Collections.ObjectModel.ReadOnlyCollection<Module> modules)
        {
            foreach (var c in modules)
            {
                var childCollection = c.OwningRank;
                if (null == childCollection)
                {
                    throw new Exception("Dependency has no rank");
                }
                var found = this.DependencyGraph.Where(item => item.Value == childCollection);
                if (0 == found.Count())
                {
                    throw new Exception("Module collection not found in graph");
                }
                if (found.Count() > 1)
                {
                    throw new Exception("Module collection found more than once in graph");
                }
                var childRank = found.First().Key;
                if (childRank <= parentRank)
                {
                    throw new Exception("Dependent module {0} found at a lower rank than the dependee", c);
                }
            }
        }

        /// <summary>
        /// Perform a validation step to ensure that all modules exist and are in correct ranks.
        /// </summary>
        public void
        Validate()
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

        /// <summary>
        /// Wrapper around the enumerator of the DependencyGraph, but only returning the rank module collections.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public System.Collections.Generic.IEnumerator<ModuleCollection>
        GetEnumerator()
        {
            foreach (var rank in this.DependencyGraph)
            {
                yield return rank.Value;
            }
        }

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Determines whether the specified module is referenced or unreferenced.
        /// </summary>
        /// <returns><c>true</c> if this instance is referenced; otherwise, <c>false</c>.</returns>
        /// <param name="module">Module to check.</param>
        public bool
        IsReferencedModule(
            Module module)
        {
            return this.ReferencedModules[module.BuildEnvironment].Contains(module);
        }

        /// <summary>
        /// Obtain the list of build environments defined for this Graph.
        /// </summary>
        /// <value>The build environments.</value>
        public System.Collections.Generic.List<Environment> BuildEnvironments
        {
            get
            {
                return this.Modules.Keys.ToList();
            }
        }

        private Array<PackageDefinition> PackageDefinitions
        {
            get;
            set;
        }

        /// <summary>
        /// Obtain the master package (the package in which Bam was invoked).
        /// </summary>
        /// <value>The master package.</value>
        public PackageDefinition MasterPackage
        {
            get
            {
                return this.PackageDefinitions[0];
            }
        }

        /// <summary>
        /// Assign the array of package definitions to the Graph.
        /// </summary>
        /// <param name="packages">Array of package definitions.</param>
        public void
        SetPackageDefinitions(
            Array<PackageDefinition> packages)
        {
            this.PackageDefinitions = packages;
            this.Macros.AddVerbatim("masterpackagename", this.MasterPackage.Name);
        }

        /// <summary>
        /// Enumerate the package definitions associated with the Graph.
        /// </summary>
        /// <value>The packages.</value>
        public System.Collections.Generic.IEnumerable<PackageDefinition> Packages
        {
            get
            {
                foreach (var package in this.PackageDefinitions)
                {
                    yield return package;
                }
            }
        }

        /// <summary>
        /// Obtain the metadata associated with the chosen build mode.
        /// </summary>
        /// <value>The build mode meta data.</value>
        public IBuildModeMetaData
        BuildModeMetaData
        {
            get;
            set;
        }

        /// <summary>
        /// For a given package, obtain the metadata and cast it to MetaDataType.
        /// </summary>
        /// <returns>The meta data.</returns>
        /// <param name="packageName">Package name.</param>
        /// <typeparam name="MetaDataType">The 1st type parameter.</typeparam>
        public MetaDataType
        PackageMetaData<MetaDataType>(
            string packageName)
            where MetaDataType : class
        {
            var package = Bam.Core.Graph.Instance.Packages.Where(item => item.Name == packageName).FirstOrDefault();
            if (null == package)
            {
                throw new Exception("Unable to locate package '{0}'", packageName);
            }
            return package.MetaData as MetaDataType;
        }

        /// <summary>
        /// Get or set the build root to write all build output to.
        /// </summary>
        /// <value>The build root.</value>
        public string BuildRoot
        {
            get
            {
                return this.TheBuildRoot;
            }
            set
            {
                if (null != this.TheBuildRoot)
                {
                    throw new Exception("The build root has already been set");
                }
                var absoluteBuildRootPath = RelativePathUtilities.MakeRelativePathAbsoluteToWorkingDir(value);
                this.TheBuildRoot = absoluteBuildRootPath;
                this.Macros.AddVerbatim("buildroot", absoluteBuildRootPath);
            }
        }

        /// <summary>
        /// Get or set the logging verbosity level to use across the build.
        /// </summary>
        /// <value>The verbosity level.</value>
        public EVerboseLevel
        VerbosityLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Obtain a list of package repositories used to locate packages.
        /// </summary>
        /// <value>The package repositories.</value>
        public StringArray PackageRepositories
        {
            get;
            private set;
        }

        /// <summary>
        /// Determine whether package definition files are automatically updated after being read.
        /// </summary>
        /// <value><c>true</c> if force definition file update; otherwise, <c>false</c>.</value>
        public bool ForceDefinitionFileUpdate
        {
            get;
            private set;
        }

        /// <summary>
        /// Determine whether package assembly compilation occurs with debug symbol information.
        /// </summary>
        /// <value><c>true</c> if compile with debug symbols; otherwise, <c>false</c>.</value>
        public bool CompileWithDebugSymbols
        {
            get;
            set;
        }

        /// <summary>
        /// Get or set the path of the package script assembly.
        /// </summary>
        /// <value>The script assembly pathname.</value>
        public string ScriptAssemblyPathname
        {
            get;
            set;
        }

        /// <summary>
        /// Get or set the compiled package script assembly.
        /// </summary>
        /// <value>The script assembly.</value>
        public System.Reflection.Assembly ScriptAssembly
        {
            get;
            set;
        }

        /// <summary>
        /// Obtain the current state of Bam.
        /// </summary>
        /// <value>The state of the process.</value>
        public BamState ProcessState
        {
            get;
            private set;
        }
    }
}
