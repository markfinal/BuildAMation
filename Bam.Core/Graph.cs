#region License
// Copyright (c) 2010-2019, Mark Final
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
        public static Graph Instance { get; private set; }

        private void
        Initialize()
        {
            this.ProcessState = new BamState();

            this.Modules = new System.Collections.Generic.Dictionary<Environment, System.Collections.Generic.List<Module>>();
            this.ReferencedModules = new System.Collections.Generic.Dictionary<Environment, Array<Module>>();
            this.TopLevelModules = new System.Collections.Generic.List<Module>();
            this.Macros = new MacroList(this.GetType().FullName);
            this.BuildEnvironmentInternal = null;
            this.CommonModuleType = new PeekableStack<System.Type>();
            this.DependencyGraph = new DependencyGraph();
            this.MetaData = null;

            this.InternalPackageRepositories = new Array<PackageRepository>();
            try
            {
                var primaryPackageRepo = System.IO.Path.Combine(
                    System.IO.Directory.GetParent(
                        System.IO.Directory.GetParent(
                            System.IO.Directory.GetParent(
                                this.ProcessState.ExecutableDirectory
                            ).FullName
                        ).FullName
                    ).FullName,
                    "packages"
                );
                if (!System.IO.Directory.Exists(primaryPackageRepo))
                {
                    throw new Exception(
                        $"Standard BAM package directory '{primaryPackageRepo}' does not exist"
                    );
                }
                this.AddPackageRepository(primaryPackageRepo, false);
            }
            catch (System.ArgumentNullException)
            {
                // this can happen during unit testing
            }

            this.ForceDefinitionFileUpdate = CommandLineProcessor.Evaluate(new Options.ForceDefinitionFileUpdate());
            this.UpdateBamAssemblyVersions = CommandLineProcessor.Evaluate(new Options.UpdateBamAssemblyVersion());
            this.CompileWithDebugSymbols = CommandLineProcessor.Evaluate(new Options.UseDebugSymbols());
        }

        /// <summary>
        /// Add the module to the flat list of all modules in the current build environment.
        /// </summary>
        /// <param name="module">Module to be added</param>
        public void
        AddModule(
            Module module) => this.Modules[this.BuildEnvironmentInternal].Add(module);

        /// <summary>
        /// Stack of module types, that are pushed when a new module is created, and popped post-creation.
        /// This is so that modules created as dependencies can inspect their module parental hierarchy at construction time.
        /// </summary>
        /// <value>The stack of module types</value>
        public PeekableStack<System.Type> CommonModuleType { get; private set; }

        /// <summary>
        /// A referenced module is one that is referenced by it's class type. This is normally in use when specifying
        /// a dependency. There can be one and only one copy, in a build environment, of this type of module.
        /// A non-referenced module, is one that is never referred to explicitly in user scripts, but are created behind
        /// the scenes by packages. There can be many instances of these modules.
        /// The graph maintains a list of all referenced modules.
        /// This function either finds an existing referenced module in the current build Environment, or will create an
        /// instance. Since the current build Environment is inspected, this function cal only be called from within the
        /// Init() calling hierarchy of a Module.
        /// </summary>
        /// <returns>The instance of the referenced module.</returns>
        /// <typeparam name="T">The type of module being referenced.</typeparam>
        public T
        FindReferencedModule<T>() where T : Module, new()
        {
            if (null == this.BuildEnvironmentInternal)
            {
                var message = new System.Text.StringBuilder();
                message.AppendLine("Unable to find a module either within a patch or after the build has started.");
                message.AppendLine("If called within a patch function, please modify the calling code to invoke this call within the module's Init method.");
                message.AppendLine("If it must called elsewhere, please use the overloaded version accepting an Environment argument.");
                throw new Exception(message.ToString());
            }
            var referencedModules = this.ReferencedModules[this.BuildEnvironmentInternal];
            var matchedModule = referencedModules.FirstOrDefault(item => item.GetType() == typeof(T));
            if (null != matchedModule)
            {
                return matchedModule as T;
            }
            this.CommonModuleType.Push(typeof(T));
            try
            {
                var newModule = Module.Create<T>(preInitCallback: module =>
                    {
                        if (null != module)
                        {
                            referencedModules.Add(module);
                        }
                    });
                return newModule;
            }
            catch (UnableToBuildModuleException)
            {
                // remove the failed to create module from the referenced list
                // and also any modules and strings created in its Init function, potentially
                // of child module types
                var moduleTypeToRemove = this.CommonModuleType.Peek();
                TokenizedString.RemoveEncapsulatedStrings(moduleTypeToRemove);
                Module.RemoveEncapsulatedModules(moduleTypeToRemove);
                referencedModules.Remove(referencedModules.First(item => item.GetType() == typeof(T)));
                var moduleEnvList = this.Modules[this.BuildEnvironmentInternal];
                moduleEnvList.Remove(moduleEnvList.First(item => item.GetType() == typeof(T)));
                throw;
            }
            finally
            {
                this.CommonModuleType.Pop();
            }
        }

        /// <summary>
        /// Find an existing instance of a referenced module, by its type, in the provided build Environment.
        /// If no such instance can be found, an exception is thrown.
        /// </summary>
        /// <typeparam name="T">Type of the referenced Module sought</typeparam>
        /// <param name="env">Environment in which the referenced Module should exist.</param>
        /// <returns>Instance of the matched Module type in the Environment's referenced Modules.</returns>
        public T
        FindReferencedModule<T>(
            Environment env) where T : Module, new()
        {
            if (null == env)
            {
                throw new Exception("Must provide a valid Environment");
            }
            var referencedModules = this.ReferencedModules[env];
            var matchedModule = referencedModules.FirstOrDefault(item => item.GetType() == typeof(T));
            if (null == matchedModule)
            {
                throw new Exception(
                    $"Unable to locate a referenced module of type {typeof(T).ToString()} in the provided build environment"
                );
            }
            return matchedModule as T;
        }

        private readonly System.Collections.Generic.Dictionary<System.Type, System.Func<Module>> compiledFindRefModuleCache = new System.Collections.Generic.Dictionary<System.Type, System.Func<Module>>();

        private Module
        MakeModuleOfType(
            System.Type moduleType)
        {
            try
            {
                if (!this.compiledFindRefModuleCache.ContainsKey(moduleType))
                {
                    // find method for the module type requested
                    // (the caching is based on this being 'expensive' as it's based on reflection)
                    var findReferencedModuleMethod = typeof(Graph).GetMethod("FindReferencedModule", System.Type.EmptyTypes);
                    var genericVersionForModuleType = findReferencedModuleMethod.MakeGenericMethod(moduleType);

                    // now compile it, so that we don't have to repeat the above
                    var instance = System.Linq.Expressions.Expression.Constant(this);
                    var call = System.Linq.Expressions.Expression.Call(
                        instance,
                        genericVersionForModuleType);
                    var lambda = System.Linq.Expressions.Expression.Lambda<System.Func<Module>>(call);
                    var func = lambda.Compile();

                    // and store it
                    this.compiledFindRefModuleCache.Add(moduleType, func);
                }
                var newModule = this.compiledFindRefModuleCache[moduleType]();
                Log.DetailProgress(Module.Count.ToString());
                return newModule;
            }
            catch (UnableToBuildModuleException exception)
            {
                Log.Info(
                    $"Unable to instantiate module of type {moduleType.ToString()} because {exception.Message} from {exception.ModuleType.ToString()}"
                );
                return null;
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                var exModuleType = (ex.InnerException is ModuleCreationException) ? (ex.InnerException as ModuleCreationException).ModuleType : moduleType;
                var realException = ex.InnerException;
                if (null == realException)
                {
                    realException = ex;
                }
                throw new Exception(realException, $"Failed to create module of type {exModuleType.ToString()}");
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
            System.Type moduleType) where ModuleType : Module => this.MakeModuleOfType(moduleType) as ModuleType;

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
            var includeTests = CommandLineProcessor.Evaluate(new Options.UseTests());
            var allTypes = assembly.GetTypes();
            var allModuleTypesInPackage = allTypes.Where(type =>
                (System.String.Equals(ns, type.Namespace, System.StringComparison.Ordinal) ||
                (includeTests && System.String.Equals(ns + ".tests", type.Namespace, System.StringComparison.Ordinal))) &&
                type.IsSubclassOf(typeof(Module))
            );
            if (!allModuleTypesInPackage.Any())
            {
                throw new Exception(
                    $"No modules found in the namespace '{ns}'. Please define some modules in the build scripts to use {ns} as a master package."
                );
            }
            System.Collections.Generic.IEnumerable<System.Type> allTopLevelModuleTypesInPackage;
            var commandLineTopLevelModules = CommandLineProcessor.Evaluate(new Options.SetTopLevelModules());
            if (null != commandLineTopLevelModules && commandLineTopLevelModules.Any())
            {
                allTopLevelModuleTypesInPackage = allModuleTypesInPackage.Where(
                    allItem => commandLineTopLevelModules.First().Any(
                        cmdModuleName => allItem.Name.Contains(cmdModuleName, System.StringComparison.Ordinal)
                    )
                );
            }
            else
            {
                allTopLevelModuleTypesInPackage = allModuleTypesInPackage.Where(type => type.IsSealed);
            }
            if (!allTopLevelModuleTypesInPackage.Any())
            {
                var message = new System.Text.StringBuilder();
                message.AppendLine(
                    $"No top-level modules found in the namespace '{ns}'. Please mark some of the modules below as 'sealed' to identify them as top-level, and thus buildable when {ns} is the master package:"
                );
                foreach (var moduleType in allModuleTypesInPackage)
                {
                    message.AppendLine($"\t{moduleType.ToString()}");
                }
                throw new Exception(message.ToString());
            }
            try
            {
                this.CreateTopLevelModuleFromTypes(allTopLevelModuleTypesInPackage, env);
            }
            catch (Exception ex)
            {
                throw new Exception(ex, $"An error occurred creating top-level modules in namespace '{ns}':");
            }
        }

        /// <summary>
        /// Create top-level modules from a list of types.
        /// </summary>
        /// <param name="moduleTypes">List of module types to create.</param>
        /// <param name="env">Build environment to create modules for.</param>
        public void
        CreateTopLevelModuleFromTypes(
            System.Collections.Generic.IEnumerable<System.Type> moduleTypes,
            Environment env)
        {
            this.BuildEnvironment = env;
            foreach (var moduleType in moduleTypes)
            {
                MakeModuleOfType(moduleType);
            }
            this.BuildEnvironment = null;
            // scan all modules in the build environment for "top-level" status
            // as although they should just be from the list of incoming moduleTypes
            // it's possible for new modules to be introduced that depend on them
            foreach (var module in this.Modules[env])
            {
                if (module.TopLevel)
                {
                    this.TopLevelModules.Add(module);
                }
            }
            if (!this.TopLevelModules.Any())
            {
                var message = new System.Text.StringBuilder();
                message.AppendLine("Top-level module types detected, but none could be instantiated:");
                foreach (var moduleType in moduleTypes)
                {
                    message.AppendLine($"\t{moduleType.ToString()}");
                }
                throw new Exception(message.ToString());
            }
        }

        /// <summary>
        /// Apply any patches associated with modules.
        /// </summary>
        public void
        ApplySettingsPatches()
        {
            Log.Detail("Apply settings to modules...");
            var scale = 100.0f / Module.Count;
            var count = 0;
            foreach (var rank in this.DependencyGraph.Reverse())
            {
                foreach (var module in rank.Value)
                {
                    module.ApplySettingsPatches();
                    Log.DetailProgress("{0,3}%", (int)(++count * scale));
                }
            }
        }

        private System.Collections.Generic.List<Module> TopLevelModules { get; set; }

        private System.Collections.Generic.Dictionary<Environment, System.Collections.Generic.List<Module>> Modules { get; set; }

        private System.Collections.Generic.Dictionary<Environment, Array<Module>> ReferencedModules { get; set; }

        /// <summary>
        /// Obtain the build mode.
        /// </summary>
        /// <value>The mode.</value>
        public string Mode { get; set; }

        /// <summary>
        /// Obtain global macros.
        /// </summary>
        /// <value>The macros.</value>
        public MacroList Macros { get; set; }

        /// <summary>
        /// Get or set metadata associated with the Graph. This is used for multi-threaded builds.
        /// </summary>
        /// <value>The meta data.</value>
        public object MetaData { get; set; }

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
                    this.ReferencedModules.Add(value, new Array<Module>());
                }
            }
        }

        /// <summary>
        /// Get the actual graph of module dependencies.
        /// </summary>
        /// <value>The dependency graph.</value>
        public DependencyGraph DependencyGraph { get; private set; }

        private void ApplyGroupDependenciesToChildren(
            Module module,
            System.Collections.ObjectModel.ReadOnlyCollection<Module> children,
            System.Collections.Generic.IEnumerable<Module> dependencies)
        {
            // find all dependencies that are not children of this module
            var nonChildDependents = dependencies.Where(item =>
                !(item is IChildModule) || (item as IChildModule).Parent != module);
            if (!nonChildDependents.Any())
            {
                return;
            }
            foreach (var c in children)
            {
                c.DependsOn(nonChildDependents);
            }
        }

        private void ApplyGroupRequirementsToChildren(
            Module module,
            System.Collections.ObjectModel.ReadOnlyCollection<Module> children,
            System.Collections.Generic.IEnumerable<Module> dependencies)
        {
            // find all dependencies that are not children of this module
            var nonChildDependents = dependencies.Where(item =>
                !(item is IChildModule) || (item as IChildModule).Parent != module);
            if (!nonChildDependents.Any())
            {
                return;
            }
            foreach (var c in children)
            {
                c.Requires(nonChildDependents);
            }
        }

        private void
        SetModuleRank(
            System.Collections.Generic.Dictionary<Module, int> map,
            Module module,
            int rankIndex)
        {
            if (map.ContainsKey(module))
            {
                throw new Exception($"Module {module.ToString()} rank initialized more than once");
            }
            map.Add(module, rankIndex);
        }

        private void
        MoveModuleRankBy(
            System.Collections.Generic.Dictionary<Module, int> map,
            Module module,
            int rankDelta)
        {
            if (!map.ContainsKey(module))
            {
                // a dependency hasn't yet been initialized, so don't try to move it
                return;
            }
            map[module] += rankDelta;
            foreach (var dep in module.Dependents)
            {
                MoveModuleRankBy(map, dep, rankDelta);
            }
            foreach (var dep in module.Requirements)
            {
                MoveModuleRankBy(map, dep, rankDelta);
            }
        }

        private void
        MoveModuleRankTo(
            System.Collections.Generic.Dictionary<Module, int> map,
            Module module,
            int rankIndex)
        {
            if (!map.ContainsKey(module))
            {
                throw new Exception($"Module {module.ToString()} has yet to be initialized");
            }
            var currentRank = map[module];
            var rankDelta = rankIndex - currentRank;
            MoveModuleRankBy(map, module, rankDelta);
        }

        private void
        ProcessModule(
            System.Collections.Generic.Dictionary<Module, int> map,
            System.Collections.Generic.Queue<Module> toProcess,
            Module module,
            int rankIndex)
        {
            if (module.Tool != null)
            {
                if (null == module.Settings)
                {
                    module.Requires(module.Tool);
                    var child = module as IChildModule;
                    if ((null == child) || (null == child.Parent))
                    {
                        // children inherit the settings from their parents
                        module.UsePublicPatches(module.Tool);
                    }

                    try
                    {
                        module.Settings = module.MakeSettings();
                    }
                    catch (System.TypeInitializationException ex)
                    {
                        throw ex.InnerException;
                    }
                    catch (System.Reflection.TargetInvocationException ex)
                    {
                        var realException = ex.InnerException;
                        if (null == realException)
                        {
                            realException = ex;
                        }
                        throw new Exception(realException, "Settings creation:");
                    }
                }
            }
            if (!module.Dependents.Any() && !module.Requirements.Any())
            {
                return;
            }
            if (module is IModuleGroup)
            {
                var children = module.Children;
                this.ApplyGroupDependenciesToChildren(module, children, module.Dependents);
                this.ApplyGroupRequirementsToChildren(module, children, module.Requirements);
            }
            var nextRankIndex = rankIndex + 1;
            foreach (var dep in module.Dependents)
            {
                if (map.ContainsKey(dep))
                {
                    if (map[dep] < nextRankIndex)
                    {
                        MoveModuleRankTo(map, dep, nextRankIndex);
                    }
                }
                else
                {
                    SetModuleRank(map, dep, nextRankIndex);
                    toProcess.Enqueue(dep);
                }
            }
            foreach (var dep in module.Requirements)
            {
                if (map.ContainsKey(dep))
                {
                    if (map[dep] < nextRankIndex)
                    {
                        MoveModuleRankTo(map, dep, nextRankIndex);
                    }
                }
                else
                {
                    SetModuleRank(map, dep, nextRankIndex);
                    toProcess.Enqueue(dep);
                }
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
            Log.Detail("Analysing module dependencies...");
            var moduleRanks = new System.Collections.Generic.Dictionary<Module, int>();
            var modulesToProcess = new System.Collections.Generic.Queue<Module>();
            var totalProgress = 3 * Module.Count; // all modules are iterated over three times (twice in here, and once in CompleteModules)
            var scale = 100.0f / totalProgress;
            // initialize the map with top-level modules
            // and populate the to-process list
            var progress = 0;
            Log.DetailProgress("{0,3}%", (int)(progress * scale));
            foreach (var module in this.TopLevelModules)
            {
                SetModuleRank(moduleRanks, module, 0);
                ProcessModule(moduleRanks, modulesToProcess, module, 0);
                Log.DetailProgress("{0,3}%", (int)((++progress) * scale));
            }
            // process all modules by initializing them to a best-guess rank
            // but then potentially moving them to a higher rank if they re-appear as dependencies
            while (modulesToProcess.Any())
            {
                var module = modulesToProcess.Dequeue();
                ProcessModule(moduleRanks, modulesToProcess, module, moduleRanks[module]);
                Log.DetailProgress("{0,3}%", (int)((++progress) * scale));
            }
            // moduleRanks[*].Value is now sparse - there may be gaps between successive ranks with modules
            // this needs to be collapsed so that the rank indices are contiguous (the order is correct, the indices are just wrong)

            // assign modules, for each rank index, into collections
            var contiguousRankIndex = 0;
            var lastRankIndex = 0;
            foreach (var nextModule in moduleRanks.OrderBy(item => item.Value))
            {
                if (lastRankIndex != nextModule.Value)
                {
                    lastRankIndex = nextModule.Value;
                    contiguousRankIndex++;
                }
                var rank = this.DependencyGraph[contiguousRankIndex];
                rank.Add(nextModule.Key);
                Log.DetailProgress("{0,3}%", (int)((++progress) * scale));
            }
            Module.CompleteModules();
        }

        private static void
        DumpModule(
            System.Text.StringBuilder builder,
            int depth,
            char? prefix,
            Module module,
            Array<Module> visited)
        {
            if (prefix.HasValue)
            {
                builder.AppendFormat("{0}{1}{2}", new string(' ', depth), prefix.Value, module.ToString());
            }
            else
            {
                builder.AppendFormat("{0}{1}", new string(' ', depth), module.ToString());
            }
            if (visited.Contains(module))
            {
                builder.AppendFormat("*");
                builder.AppendLine();
                return;
            }
            visited.Add(module);
            if (module is IInputPath inputPath)
            {
                builder.AppendLine($" {inputPath.InputPath.ToString()}");
            }
            foreach (var req in module.Requirements)
            {
                DumpModule(builder, depth + 1, '-', req, visited);
            }
            foreach (var dep in module.Dependents)
            {
                DumpModule(builder, depth + 1, '+', dep, visited);
            }
        }

        private void
        DumpModuleHierarchy()
        {
            Log.Message(this.VerbosityLevel, "Module hierarchy");
            var visited = new Array<Module>();
            foreach (var module in this.TopLevelModules)
            {
                var text = new System.Text.StringBuilder();
                DumpModule(text, 0, null, module, visited);
                Log.Message(this.VerbosityLevel, text.ToString());
            }
        }

        private void
        DumpRankHierarchy()
        {
            Log.Message(this.VerbosityLevel, "Rank hierarchy");
            foreach (var rank in this.DependencyGraph)
            {
                var text = new System.Text.StringBuilder();
                text.AppendFormat("{2}Rank {0}: {1} modules{2}", rank.Key, rank.Value.Count(), System.Environment.NewLine);
                text.AppendLine(new string('-', 80));
                foreach (var m in rank.Value)
                {
                    text.AppendLine(m.ToString());
                    if (m is IInputPath inputPath)
                    {
                        text.AppendFormat("\tInput: {0}{1}", inputPath.InputPath.ToString(), System.Environment.NewLine);
                    }
                    foreach (var s in m.GeneratedPaths)
                    {
                        text.AppendFormat("\t{0} : {1}{2}", s.Key, s.Value, System.Environment.NewLine);
                    }
                }
                Log.Message(this.VerbosityLevel, text.ToString());
            }
        }

        /// <summary>
        /// Dump a representation of the dependency graph to the console.
        /// Initially a representation of module hierarchies
        ///  depth of dependency is indicated by indentation
        ///  a direct dependency is a + prefix
        ///  an indirect dependency is a - prefix
        /// Then a representation of rank hierarchies, i.e. the order in which
        /// modules will be built.
        /// </summary>
        public void
        Dump()
        {
            Log.Message(this.VerbosityLevel, new string('*', 80));
            Log.Message(this.VerbosityLevel, "{0,50}", "DEPENDENCY GRAPH VIEW");
            Log.Message(this.VerbosityLevel, new string('*', 80));
            this.DumpModuleHierarchy();
            this.DumpRankHierarchy();
            Log.Message(this.VerbosityLevel, new string('*', 80));
            Log.Message(this.VerbosityLevel, "{0,50}", "END DEPENDENCY GRAPH VIEW");
            Log.Message(this.VerbosityLevel, new string('*', 80));
        }

        private void
        InternalValidateGraph(
            int parentRankIndex,
            System.Collections.ObjectModel.ReadOnlyCollection<Module> modules)
        {
            foreach (var c in modules)
            {
                var childCollection = c.OwningRank;
                if (null == childCollection)
                {
                    throw new Exception("Dependency has no rank");
                }
                try
                {
                    var childRank = this.DependencyGraph.First(item => item.Value == childCollection);
                    var childRankIndex = childRank.Key;
                    if (childRankIndex <= parentRankIndex)
                    {
                        throw new Exception(
                            $"Dependent module {c.ToString()} found at a lower rank {childRankIndex} than the dependee {parentRankIndex}"
                        );
                    }
                }
                catch (System.InvalidOperationException)
                {
                    throw new Exception("Module collection not found in graph");
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
            Log.DebugMessage("Used packages:");
            foreach (var package in this.PackageDefinitions.Where(item => item.CreatedModules().Any()))
            {
                Log.DebugMessage($"\t{package.Name}");
                foreach (var module in package.CreatedModules())
                {
                    Log.DebugMessage($"\t\t{module.ToString()}");
                }
            }
            Log.DebugMessage("Unused packages:");
            foreach (var package in this.PackageDefinitions.Where(item => !item.CreatedModules().Any()))
            {
                Log.DebugMessage($"\t{package.Name}");
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
        System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Determines whether the specified module is referenced or unreferenced.
        /// </summary>
        /// <returns><c>true</c> if this instance is referenced; otherwise, <c>false</c>.</returns>
        /// <param name="module">Module to check.</param>
        public bool
        IsReferencedModule(
            Module module) => this.ReferencedModules[module.BuildEnvironment].Contains(module);

        /// <summary>
        /// Returns a read only collection of all the named/referenced/encapsulating modules
        /// for the specified Environment.
        /// </summary>
        /// <returns>The collection of modules</returns>
        /// <param name="env">The Environment to query for named modules.</param>
        public System.Collections.ObjectModel.ReadOnlyCollection<Module>
        EncapsulatingModules(
            Environment env) => this.ReferencedModules[env].ToReadOnlyCollection();

        /// <summary>
        /// Obtain the list of build environments defined for this Graph.
        /// </summary>
        /// <value>The build environments.</value>
        public System.Collections.Generic.List<Environment> BuildEnvironments => this.Modules.Keys.ToList();

        private Array<PackageDefinition> PackageDefinitions { get; set; }

        /// <summary>
        /// Obtain the master package (the package in which Bam was invoked).
        /// </summary>
        /// <value>The master package.</value>
        public PackageDefinition MasterPackage => this.PackageDefinitions[0];

        /// <summary>
        /// Assign the array of package definitions to the Graph.
        /// Macros added to the Graph are:
        /// 'masterpackagename'
        /// Packages with external sources are fetched at this point.
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
                if (null == this.PackageDefinitions)
                {
                    throw new Exception("No packages were defined in the build");
                }
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
        public IBuildModeMetaData BuildModeMetaData { get; set; }

        static internal PackageMetaData
        InstantiatePackageMetaData(
            System.Type metaDataType)
        {
            try
            {
                return System.Activator.CreateInstance(metaDataType) as PackageMetaData;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            catch (System.Reflection.TargetInvocationException exception)
            {
                throw new Exception(exception, "Failed to create package metadata");
            }
        }

        static internal PackageMetaData
        InstantiatePackageMetaData<MetaDataType>() => InstantiatePackageMetaData(typeof(MetaDataType));

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
            var package = Bam.Core.Graph.Instance.Packages.FirstOrDefault(item => item.Name.Equals(packageName, System.StringComparison.Ordinal));
            if (null == package)
            {
                throw new Exception("Unable to locate package '{0}'", packageName);
            }
            if (null == package.MetaData)
            {
                package.MetaData = InstantiatePackageMetaData<MetaDataType>();
            }
            return package.MetaData as MetaDataType;
        }

        /// <summary>
        /// Get or set the build root to write all build output to.
        /// Macros added to the graph:
        /// 'buildroot'
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
                var absoluteBuildRootPath = RelativePathUtilities.ConvertRelativePathToAbsolute(
                    Graph.Instance.ProcessState.WorkingDirectory,
                    value
                );
                this.TheBuildRoot = absoluteBuildRootPath;
                this.Macros.AddVerbatim("buildroot", absoluteBuildRootPath);
            }
        }

        /// <summary>
        /// Get or set the logging verbosity level to use across the build.
        /// </summary>
        /// <value>The verbosity level.</value>
        public EVerboseLevel VerbosityLevel { get; set; }

        private Array<PackageRepository> InternalPackageRepositories { get; set; }

        /// <summary>
        /// Adds a new package repository, if not already added.
        /// </summary>
        /// <param name="repoPath">Path to the new package repository.</param>
        /// <param name="requiresSourceDownload">Requires a source download?</param>
        public void
        AddPackageRepository(
            string repoPath,
            bool requiresSourceDownload)
        {
            if (this.InternalPackageRepositories.Any(item => item.RootPath == repoPath))
            {
                return;
            }
            this.InternalPackageRepositories.Add(new PackageRepository(repoPath, requiresSourceDownload));
        }

        /// <summary>
        /// Enumerates the package repositories known about in the build.
        /// </summary>
        /// <value>Each package repository path.</value>
        public System.Collections.Generic.IEnumerable<PackageRepository> PackageRepositories
        {
            get
            {
                foreach (var pkgRepo in this.InternalPackageRepositories)
                {
                    yield return pkgRepo;
                }
            }
        }

        /// <summary>
        /// Determine whether package definition files are automatically updated after being read.
        /// </summary>
        /// <value><c>true</c> if force definition file update; otherwise, <c>false</c>.</value>
        public bool ForceDefinitionFileUpdate { get; private set; }

        /// <summary>
        /// Determine whether package definition files read have their BAM assembly versions updated
        /// to the current version of BAM running.
        /// Requires forced updates to definition files to be enabled.
        /// </summary>
        /// <value><c>true</c> to update bam assembly versions; otherwise, <c>false</c>.</value>
        public bool UpdateBamAssemblyVersions { get; private set; }

        /// <summary>
        /// Determine whether package assembly compilation occurs with debug symbol information.
        /// </summary>
        /// <value><c>true</c> if compile with debug symbols; otherwise, <c>false</c>.</value>
        public bool CompileWithDebugSymbols { get; set; }

        /// <summary>
        /// Get or set the path of the package script assembly.
        /// </summary>
        /// <value>The script assembly pathname.</value>
        public string ScriptAssemblyPathname { get; set; }

        /// <summary>
        /// Get or set the compiled package script assembly.
        /// </summary>
        /// <value>The script assembly.</value>
        public System.Reflection.Assembly ScriptAssembly { get; set; }

        /// <summary>
        /// Obtain the current state of Bam.
        /// </summary>
        /// <value>The state of the process.</value>
        public BamState ProcessState { get; private set; }

        /// <summary>
        /// Obtain the named referenced module for a given environment and the type of the module required.
        /// An exception will be thrown if the type does not refer to any referenced module in that environment.
        /// </summary>
        /// <param name="env">Environment in which the referenced module was created.</param>
        /// <param name="type">The type of the module requested.</param>
        /// <returns>The instance of the referenced module requested.</returns>
        public Module
        GetReferencedModule(
            Environment env,
            System.Type type)
        {
            try
            {
                return this.ReferencedModules[env].First(item => item.GetType() == type);
            }
            catch (System.InvalidOperationException)
            {
                Log.ErrorMessage($"Unable to locate a referenced module of type {type.ToString()}");
                throw;
            }
        }

        /// <summary>
        /// Obtain the IProductDefinition instance (if it exists) from the package assembly.
        /// </summary>
        public IProductDefinition ProductDefinition { get; set; }

        /// <summary>
        /// Obtain the IOverrideModuleConfiguration instance (if it exists) from the package assembly.
        /// </summary>
        public IOverrideModuleConfiguration OverrideModuleConfiguration { get; set; }
    }
}
