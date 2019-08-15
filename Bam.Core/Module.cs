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
    /// Abstract concept of a module, the base class for all buildables in BAM. A hierarchy of classes in modules
    /// allows all modules to share similar features, and build specifics with each sub-class.
    /// </summary>
    public abstract class Module :
        IModuleExecution
    {
        /// <summary>
        /// Static cache of all modules created.
        /// </summary>
        static protected System.Collections.Generic.List<Module> AllModules;

        /// <summary>
        /// Reset all static state of the Module class.
        /// This function is only really useful in unit tests.
        /// </summary>
        public static void
        Reset() => AllModules = new System.Collections.Generic.List<Module>();

        static Module() => Reset();

        /// <summary>
        /// Protected constructor (use Init function in general use to configure a module) for a new module. Use Module.Create
        /// to create new instances of a module.
        /// This defines the standard macros for all modules:
        /// 'bampackagedir' - the directory in which the 'bam' directory resides
        /// 'packagedir' - same as 'bampackagedir' unless a Bam.Core.PackageDirectoryRedirect attribute is specified
        /// 'packagename' - name of the package
        /// 'packagebuilddir' - equivalent to $(buildroot)/$(packagename)
        /// 'modulename' - name of the module
        /// 'OutputName' - the 'main' part of the filename for the output of the module, but may have a module specific prefix and/or suffix
        /// 'config' - the textual name of the configuration for the module
        /// Note that the EncapsulatingModule property is not available until after the Module's preInit callback has been invoked.
        /// But the EncapsulatingType is available after the constructor has completed.
        /// </summary>
        protected Module()
        {
            var graph = Graph.Instance;
            if (null == graph.BuildEnvironment)
            {
                throw new Exception($"No build environment for module {this.GetType().ToString()}");
            }

            graph.AddModule(this);
            this.TokenizedStringCacheMap = new System.Collections.Generic.Dictionary<System.Int64, TokenizedString>();
            this.Macros = new MacroList(this.GetType().FullName);
            // TODO: Can this be generalized to be a collection of files?
            this._GeneratedPaths = new System.Collections.Generic.Dictionary<string, TokenizedString>();

            // capture the type of the encapsulating module
            this.EncapsulatingType = graph.CommonModuleType.Peek();

            // add the package root
            try
            {
                var packageNameSpace = this.EncapsulatingType.Namespace;
                var packageDefinition = graph.Packages.FirstOrDefault(item => item.Name.Equals(packageNameSpace, System.StringComparison.Ordinal));
                if (null == packageDefinition)
                {
                    var includeTests = CommandLineProcessor.Evaluate(new Options.UseTests());
                    if (includeTests && packageNameSpace.EndsWith(".tests", System.StringComparison.Ordinal))
                    {
                        packageNameSpace = packageNameSpace.Replace(".tests", string.Empty);
                        packageDefinition = graph.Packages.FirstOrDefault(item => item.Name.Equals(packageNameSpace, System.StringComparison.Ordinal));
                    }

                    if (null == packageDefinition)
                    {
                        throw new Exception($"Unable to locate package for namespace '{packageNameSpace}'");
                    }
                }
                this.PackageDefinition = packageDefinition;
                this.Macros.AddVerbatim(ModuleMacroNames.BamPackageDirectory, packageDefinition.GetPackageDirectory());
                this.AddRedirectedPackageDirectory(this);
                this.Macros.AddVerbatim(ModuleMacroNames.PackageName, packageDefinition.Name);
                this.Macros.AddVerbatim(ModuleMacroNames.PackageBuildDirectory, packageDefinition.GetBuildDirectory());
                Graph.Instance.Macros.Add($"{packageDefinition.Name}.packagedir", this.Macros[ModuleMacroNames.PackageDirectory]);
            }
            catch (System.NullReferenceException)
            {
                // graph.Packages can be null during unittests
            }
            this.Macros.AddVerbatim(ModuleMacroNames.ModuleName, this.GetType().Name);
            this.Macros.Add(ModuleMacroNames.OutputName, this.Macros[ModuleMacroNames.ModuleName]);

            this.OwningRank = null;
            this.Tool = null;
            this.MetaData = null;
            this.BuildEnvironment = graph.BuildEnvironment;
            this.Macros.AddVerbatim(ModuleMacroNames.ConfigurationName, this.BuildEnvironment.Configuration.ToString());
            this.ReasonToExecute = ExecuteReasoning.Undefined();
            this.ExecutionTask = null;
            this.EvaluationTask = null;

            this.PackageDefinition.AddCreatedModule(this.GetType().Name);

            // download the primary package source archive if needed
            if (null != this.PackageDefinition.Sources)
            {
                foreach (var source in this.PackageDefinition.Sources)
                {
                    source.Fetch();
                }
            }

            // there may also be derived modules from other packages, so download their source archives too
            var parentType = this.GetType().BaseType;
            while (parentType != null)
            {
                if (parentType == typeof(Module))
                {
                    break;
                }
                var packageDefinition = graph.Packages.FirstOrDefault(item => item.Name.Equals(parentType.Namespace, System.StringComparison.Ordinal));
                if (null == packageDefinition)
                {
                    break;
                }
                if (null != packageDefinition.Sources)
                {
                    foreach (var source in packageDefinition.Sources)
                    {
                        source.Fetch();
                    }
                }
                parentType = parentType.BaseType;
            }
        }

        private void
        AddRedirectedPackageDirectory(
            Module moduleWithAttributes)
        {
            // if there are multiple sources, they all extract to the same place
            var downloadedPackageSource = this.PackageDefinition.Sources?.First().ExtractedPackageDir;
            if (null != downloadedPackageSource)
            {
                this.Macros.AddVerbatim(ModuleMacroNames.PackageDirectory, downloadedPackageSource);
                return;
            }

            var allModulePackageDirRedirection = moduleWithAttributes.GetType().GetCustomAttributes(typeof(ModulePackageDirectoryRedirectAttribute), false);
            if (allModulePackageDirRedirection.Length > 0)
            {
                if (allModulePackageDirRedirection.Length > 1)
                {
                    throw new Exception(
                        $"Cannot be more than one module packagedir redirection attribute on module {moduleWithAttributes.GetType().FullName}"
                    );
                }
                var attr = allModulePackageDirRedirection[0] as ModulePackageDirectoryRedirectAttribute;
                var redirectedNamespace = attr.SourceModuleType.Namespace;
                var redirectedPackageDefinition = Graph.Instance.Packages.FirstOrDefault(item => item.Name.Equals(redirectedNamespace, System.StringComparison.Ordinal));
                if (null == redirectedNamespace)
                {
                    throw new Exception($"Unable to find package definition for module type {attr.SourceModuleType.FullName}");
                }

                var downloadedRedirectedPackageSource = redirectedPackageDefinition.Sources?.First().ExtractedPackageDir;
                if (null != downloadedRedirectedPackageSource)
                {
                    this.Macros.AddVerbatim(ModuleMacroNames.PackageDirectory, downloadedRedirectedPackageSource);
                }
                else
                {
                    this.Macros.AddVerbatim(ModuleMacroNames.PackageDirectory, redirectedPackageDefinition.GetPackageDirectory());
                }
                return;
            }

            var allPackageDirRedirection = Graph.Instance.ScriptAssembly.GetCustomAttributes(typeof(PackageDirectoryRedirectAttribute), false);
            if (allPackageDirRedirection.Length > 0)
            {
                foreach (PackageDirectoryRedirectAttribute packageDirRedirect in allPackageDirRedirection)
                {
                    if (packageDirRedirect.Name.Equals(PackageDefinition.Name, System.StringComparison.Ordinal))
                    {
                        if (null != packageDirRedirect.Version)
                        {
                            if (!packageDirRedirect.Version.Equals(PackageDefinition.Version, System.StringComparison.Ordinal))
                            {
                                continue;
                            }
                        }

                        if (RelativePathUtilities.IsPathAbsolute(packageDirRedirect.RedirectedPath))
                        {
                            this.Macros.AddVerbatim(ModuleMacroNames.PackageDirectory, packageDirRedirect.RedirectedPath);
                        }
                        else
                        {
                            this.Macros.Add(
                                ModuleMacroNames.PackageDirectory,
                                this.CreateTokenizedString(
                                    "@normalize($(bampackagedir)/$(0))",
                                    Bam.Core.TokenizedString.CreateVerbatim(packageDirRedirect.RedirectedPath)
                                )
                            );
                        }
                        return;
                    }
                }
            }
            this.Macros.Add(
                ModuleMacroNames.PackageDirectory,
                this.CreateTokenizedString($"$({ModuleMacroNames.BamPackageDirectory})")
            );
        }

        /// <summary>
        /// Initialize the module. The base implementation does nothing, but subsequent sub-classing
        /// adds more specific details. Always invoke the base.Init.
        /// </summary>
        protected virtual void
        Init()
        { }

        /// <summary>
        /// Utillity function to determine whether a specific module type can be created. Does it satisfy all requirements,
        /// such as platform and configuration filters.
        /// </summary>
        /// <returns><c>true</c> if can create the specified moduleType; otherwise, <c>false</c>.</returns>
        /// <param name="moduleType">Module type.</param>
        public static bool
        CanCreate(
            System.Type moduleType)
        {
            var platformFilters = moduleType.GetCustomAttributes(typeof(PlatformFilterAttribute), true) as PlatformFilterAttribute[];
            var configurationFilters = moduleType.GetCustomAttributes(typeof(ConfigurationFilterAttribute), true) as ConfigurationFilterAttribute[];
            if (platformFilters.Length > 0 && !platformFilters[0].Includes(Graph.Instance.BuildEnvironment.Platform))
            {
                Log.DebugMessage($"Cannot create module of type {moduleType.ToString()} as it does not satisfy the platform filter");
                return false;
            }
            if (configurationFilters.Length > 0 && !configurationFilters[0].Includes(Graph.Instance.BuildEnvironment.Configuration))
            {
                Log.DebugMessage($"Cannot create module of type {moduleType.ToString()} as it does not satisfy the configuration filter");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Define the delegate that can be invoked after module construction but before Init has been called.
        /// </summary>
        /// <typeparam name="T">Type of the module that has been created.</typeparam>
        /// <param name="module">Module that has been created, but Init has yet to be called.</param>
        public delegate void PreInitDelegate<T>(T module);

        /// <summary>
        /// Define the delegate that can be invoked after module construction and Init have both been called.
        /// </summary>
        /// <param name="module">The module that has just been created and initialized.</param>
        public delegate void PostInitDelegate(Module module);

        private void
        ConfigureEncapsulatingModule()
        {
            var graph = Graph.Instance;
            this.EncapsulatingModule = graph.GetReferencedModule(this.BuildEnvironment, this.EncapsulatingType);

            // TODO: there may have to be a more general module type for something that is not built, as this affects modules referred to prebuilts too
            // note that this cannot be a class, as modules already are derived from another base class (generally)
            if (this.EncapsulatingModule is PreBuiltTool)
            {
                return;
            }
            this.Macros.Add(ModuleMacroNames.ModuleOutputDirectory, graph.BuildModeMetaData.ModuleOutputDirectory(this, this.EncapsulatingModule));
        }

        /// <summary>
        /// Create the specified module type T, given an optional parent module (for collections), pre-init and post-init callbacks.
        /// </summary>
        /// <param name="parent">Parent module for which this new module is intended as a child.</param>
        /// <param name="preInitCallback">Callback invoked after module creation, but prior to Init.</param>
        /// <param name="postInitCallback">Callback invoked after Init.</param>
        /// <typeparam name="T">The type of the module to create.</typeparam>
        public static T
        Create<T>(
            Module parent = null,
            PreInitDelegate<T> preInitCallback = null,
            PostInitDelegate postInitCallback = null) where T : Module, new()
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                if (!CanCreate(typeof(T)))
                {
                    return null;
                }

                var module = new T();
                if (null != parent)
                {
                    // children of Modules that have been redirect, also need to inherit the
                    // redirected package directory
                    module.AddRedirectedPackageDirectory(parent);
                }
                if (preInitCallback != null)
                {
                    preInitCallback(module);
                }
                // required to run after the preInitCallback for referenced modules
                {
                    module.ConfigureEncapsulatingModule();
                    module.InitializeModuleConfiguration();
                }
                module.Init();
                if (postInitCallback != null)
                {
                    postInitCallback(module);
                }
                AllModules.Add(module);
                stopwatch.Stop();
                module.CreationTime = new System.TimeSpan(stopwatch.ElapsedTicks);
                return module;
            }
            catch (UnableToBuildModuleException exception)
            {
                if (null == exception.ModuleType)
                {
                    exception.ModuleType = typeof(T);
                    throw exception;
                }
                throw;
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

        /// <summary>
        /// Clone an existing module, and copy its private patches into the clone. If the source
        /// module has a parent, private patches from the parent are also copied.
        /// </summary>
        /// <typeparam name="T">Type of the module to clone.</typeparam>
        /// <param name="source">Module to be cloned.</param>
        /// <param name="parent">Optional parent module for the clone, should it be a child of a container.</param>
        /// <param name="preInitCallback">Optional pre-Init callback to be invoked during creation.</param>
        /// <param name="postInitCallback">Optional post-Init callback to be invoked after Init has returned.</param>
        /// <returns>The cloned module</returns>
        public static T
        CloneWithPrivatePatches<T>(
            T source,
            Module parent = null,
            PreInitDelegate<T> preInitCallback = null,
            PostInitDelegate postInitCallback = null) where T : Module, new()
        {
            var clone = Create<T>(parent, preInitCallback, postInitCallback);
            if (null == clone)
            {
                throw new Exception(
                    "Making a clone has failed, even though the source module exists. This is unexpected behaviour. Please report it with details for reproduction."
                );
            }
            clone.PrivatePatches.AddRange(source.PrivatePatches);
            if (source is IChildModule sourceAsChild)
            {
                clone.PrivatePatches.AddRange(sourceAsChild.Parent.PrivatePatches);
            }
            return clone;
        }

        /// <summary>
        /// Register a path against a particular key for the module. Useful for output paths that are referenced in dependents.
        /// Will throw an exception if the key has already been registered and parsed, otherwise it can be replaced, for example
        /// in Module subclasses that share a path key.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="path">Path.</param>
        protected void
        RegisterGeneratedFile(
            string key,
            TokenizedString path)
        {
            if (this._GeneratedPaths.ContainsKey(key))
            {
                if (this._GeneratedPaths[key].IsParsed)
                {
                    throw new Exception(
                        $"Key '{key}' has already been registered as a generated file for module {this.ToString()}"
                    );
                }
                if (this.OutputDirs.ContainsKey(key))
                {
                    this.OutputDirs.Remove(key);
                }
                this._GeneratedPaths[key] = path;
            }
            else
            {
                this._GeneratedPaths.Add(key, path);
            }
            if (null != path)
            {
                this.OutputDirs.Add(
                    key,
                    this.CreateTokenizedString("@dir($(0))", path)
                );
            }
        }

        /// <summary>
        /// Register an empty path against a given key.
        /// </summary>
        /// <param name="key">Key.</param>
        private void
        RegisterGeneratedFile(
            string key) => this.RegisterGeneratedFile(key, null);

        private void
        InternalDependsOn(
            Module module)
        {
            if (this == module)
            {
                throw new Exception(
                    $"Circular reference. Module {this.ToString()} cannot depend on itself"
                );
            }
            if (this.DependentsList.Contains(module))
            {
                return;
            }
            if (this.DependeesList.Contains(module))
            {
                throw new Exception(
                    $"Cyclic dependency found between {this.ToString()} and {module.ToString()}"
                );
            }
            this.DependentsList.Add(module);
            module.DependeesList.Add(this);
        }

        /// <summary>
        /// An axiom of Bam. If a module depends on another, that other must have completely been brought up-to-date
        /// before the first module can begin to build.
        /// </summary>
        /// <param name="module">Module to depend upon.</param>
        /// <param name="moreModules">A zero-or-longer list or further modules to depend upon</param>
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

        /// <summary>
        /// An axiom of Bam. Depend upon a list of modules.
        /// </summary>
        /// <param name="modules">Modules.</param>
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
            if (this == module)
            {
                throw new Exception(
                    $"Circular reference. Module {this.ToString()} cannot require itself"
                );
            }
            if (this.RequiredDependentsList.Contains(module))
            {
                return;
            }
            this.RequiredDependentsList.Add(module);
            module.RequiredDependeesList.Add(this);
        }

        /// <summary>
        /// An axiom of Bam. A module requires another module, ensures that both modules will be brought up-to-date.
        /// </summary>
        /// <param name="module">Module.</param>
        /// <param name="moreModules">A zero-or-longer list or further modules to depend upon</param>
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

        /// <summary>
        /// An axiom of Bam. Require a list of modules to exist.
        /// </summary>
        /// <param name="modules">Modules.</param>
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

        /// <summary>
        /// Get or set the Settings instance associated with the Tool for this Module. Can be null.
        /// </summary>
        /// <value>The settings.</value>
        public Settings Settings { get; set; }

        /// <summary>
        /// Get the package definition containing this module.
        /// </summary>
        /// <value>The package definition.</value>
        public PackageDefinition PackageDefinition { get; private set; }

        /// <summary>
        /// Delegate used for private-scope patching of Settings.
        /// </summary>
        public delegate void PrivatePatchDelegate(Settings settings);

        /// <summary>
        /// Add a private patch to the current module. Usually this takes the form of a lambda function.
        /// </summary>
        /// <param name="dlg">The delegate to execute privately on the module.</param>
        public void
        PrivatePatch(
            PrivatePatchDelegate dlg) => this.PrivatePatches.Add(dlg);

        /// <summary>
        /// Add a closing patch to the current module, using the same delegate as a private patch.
        /// There can only be one closing patch on a module.
        /// It is always executed after all other patches, and thus can assume that the module's Settings object has all of its
        /// properties in their final state just prior to execution.
        /// </summary>
        /// <param name="dlg">The delegate to execute as a closing patch on the module.</param>
        public void
        ClosingPatch(
            PrivatePatchDelegate dlg)
        {
            if (null != this.TheClosingPatch)
            {
                throw new Exception($"Module {this.ToString()} already has a closing patch");
            }
            this.TheClosingPatch = dlg;
        }

        /// <summary>
        /// Delegate used for public-scope patching of Settings. Note that appliedTo is the module on which
        /// this delegate is being applied.
        /// </summary>
        public delegate void PublicPatchDelegate(Settings settings, Module appliedTo);

        /// <summary>
        /// Add a public patch to the current module. Usually this takes the form of a lambda function.
        /// </summary>
        /// <param name="dlg">The delegate to execute on the module, and on its dependees.</param>
        public void
        PublicPatch(
            PublicPatchDelegate dlg) => this.PublicPatches.Add(dlg);

        /// <summary>
        /// Instruct this module to use the public patches, and any inherited patches, from the dependent module.
        /// All inherited patches will be forwarded onto any uses of this module.
        /// </summary>
        /// <param name="module">Dependent module containing patches to use.</param>
        public void
        UsePublicPatches(
            Module module)
        {
            this.PublicInheritedPatches.Add(module.PublicPatches);
            this.PublicInheritedPatches.AddRange(module.PublicInheritedPatches);
        }

        /// <summary>
        /// Instruct this module to use the public patches from the dependent module.
        /// All patches will be used privately to this module.
        /// </summary>
        /// <param name="module">Dependent module containing patches to use.</param>
        public void
        UsePublicPatchesPrivately(
            Module module)
        {
            this.PrivateInheritedPatches.Add(module.PublicPatches);
            this.PrivateInheritedPatches.AddRange(module.PublicInheritedPatches);
        }

        /// <summary>
        /// Determine whether a module has any patches to be applied.
        /// </summary>
        /// <value><c>true</c> if this instance has patches; otherwise, <c>false</c>.</value>
        public bool HasPatches
        {
            get
            {
                return this.PrivatePatches.Any() ||
                       this.PublicPatches.Any() ||
                       this.PublicInheritedPatches.Any() ||
                       (null != this.TheClosingPatch);
            }
        }

        /// <summary>
        /// Obtain a read-only list of modules it depends on.
        /// </summary>
        /// <value>The dependents.</value>
        public System.Collections.ObjectModel.ReadOnlyCollection<Module> Dependents => this.DependentsList.ToReadOnlyCollection();

        /// <summary>
        /// Obtain a read-only list of modules that depend on it.
        /// </summary>
        /// <value>The dependees.</value>
        public System.Collections.ObjectModel.ReadOnlyCollection<Module> Dependees => this.DependeesList.ToReadOnlyCollection();

        /// <summary>
        /// Obtain a read-only list of modules it requires to be up-to-date.
        /// </summary>
        /// <value>The requirements.</value>
        public System.Collections.ObjectModel.ReadOnlyCollection<Module> Requirements => this.RequiredDependentsList.ToReadOnlyCollection();

        /// <summary>
        /// Obtain a read-only list of modules that require it.
        /// </summary>
        /// <value>The requirees.</value>
        public System.Collections.ObjectModel.ReadOnlyCollection<Module> Requirees => this.RequiredDependeesList.ToReadOnlyCollection();

        /// <summary>
        /// Obtain a read-only list of dependents that are children of this module.
        /// </summary>
        /// <value>The children.</value>
        public System.Collections.ObjectModel.ReadOnlyCollection<Module> Children => new System.Collections.ObjectModel.ReadOnlyCollection<Module>(this.DependentsList.Where(item => (item is IChildModule child) && (child.Parent == this)).ToList());

        private readonly Array<Module> DependentsList = new Array<Module>();
        private readonly Array<Module> DependeesList = new Array<Module>();

        private readonly Array<Module> RequiredDependentsList = new Array<Module>();
        private readonly Array<Module> RequiredDependeesList = new Array<Module>();

        private System.Collections.Generic.List<PrivatePatchDelegate> PrivatePatches = new System.Collections.Generic.List<PrivatePatchDelegate>();
        private System.Collections.Generic.List<PublicPatchDelegate> PublicPatches = new System.Collections.Generic.List<PublicPatchDelegate>();
        private System.Collections.Generic.List<System.Collections.Generic.List<PublicPatchDelegate>> PublicInheritedPatches = new System.Collections.Generic.List<System.Collections.Generic.List<PublicPatchDelegate>>();
        private System.Collections.Generic.List<System.Collections.Generic.List<PublicPatchDelegate>> PrivateInheritedPatches = new System.Collections.Generic.List<System.Collections.Generic.List<PublicPatchDelegate>>();
        private PrivatePatchDelegate TheClosingPatch = null;

        private System.Collections.Generic.Dictionary<string, TokenizedString> _GeneratedPaths { get; set; }

        /// <summary>
        /// Get the dictionary of keys and strings for all registered generated paths with the module.
        /// </summary>
        /// <value>The generated paths.</value>
        public System.Collections.Generic.IReadOnlyDictionary<string, TokenizedString> GeneratedPaths => this._GeneratedPaths;

        private readonly System.Collections.Generic.Dictionary<string, TokenizedString> OutputDirs = new System.Collections.Generic.Dictionary<string, TokenizedString>();

        /// <summary>
        /// Return output directories required to exist for this module.
        /// </summary>
        public System.Collections.Generic.IEnumerable<TokenizedString> OutputDirectories
        {
            get
            {
                foreach (var dir in this.OutputDirs.Values.Where(item => !System.String.IsNullOrEmpty(item.ToString())))
                {
                    yield return dir;
                }
            }
        }

        /// <summary>
        /// Gets or sets meta data on the module, which build mode packages use to associated extra data for builds.
        /// </summary>
        /// <value>The meta data.</value>
        public object MetaData { get; set; }

        /// <summary>
        /// Internal module execution function, invoked from IModuleExecution.
        /// </summary>
        /// <param name="context">Context.</param>
        protected abstract void
        ExecuteInternal(
            ExecutionContext context);

        void
        IModuleExecution.Execute(
            ExecutionContext context)
        {
            if (context.Evaluate)
            {
                this.EvaluationTask?.Wait();
                if (null == this.ReasonToExecute)
                {
                    Log.Message(
                        context.ExplainLoggingLevel,
                        $"Module {this.ToString()} is up-to-date"
                    );
                    this.Executed = true;
                    return;
                }
                Log.Message(
                    context.ExplainLoggingLevel,
                    $"Module {this.ToString()} will change because {this.ReasonToExecute.ToString()}."
                );
            }
            this.ExecuteInternal(context);
            this.Executed = true;
        }

        /// <summary>
        /// Implementation of IModuleExecution.Executed
        /// </summary>
        public bool Executed { get; private set; }

        /// <summary>
        /// Determine if the module is a top-level module, i.e. is from the package in which Bam was invoked,
        /// and nothing depends on it.
        /// </summary>
        /// <value><c>true</c> if top level; otherwise, <c>false</c>.</value>
        public bool TopLevel
        {
            get
            {
                var isTopLevel = !this.DependeesList.Any() &&
                    !this.RequiredDependeesList.Any() &&
                    (this.PackageDefinition == Graph.Instance.MasterPackage);
                return isTopLevel;
            }
        }

        /// <summary>
        /// Gets the macros associated with this Module.
        /// </summary>
        /// <value>The macros.</value>
        public MacroList Macros { get; private set; }

        /// <summary>
        /// Gets or sets the ModuleCollection, which is associated with a rank in the DependencyGraph.
        /// </summary>
        /// <value>The owning rank.</value>
        public ModuleCollection OwningRank { get; set; }

        private Module TheTool;
        /// <summary>
        /// Get or set the tool associated with the module.
        /// </summary>
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
                    throw new Exception($"Tool {value.GetType().ToString()} does not implement {typeof(ITool).ToString()}");
                }
                this.TheTool = value;
            }
        }

        /// <summary>
        /// Apply any patches set on the module with the settings for its tool.
        /// </summary>
        public void
        ApplySettingsPatches()
        {
            this.ApplySettingsPatches(this.Settings, true);
        }

        /// <summary>
        /// Apply any patches set on the module with the settings for its tool.
        /// If a module has no Settings instance, then no patches are executed. This is so that modules can have
        /// public patches to affect those that depend on them. However, if only private patches exist, but there is no
        /// Settings instance, an exception is thrown.
        /// Order of evaluation is:
        /// 1. If this is a child module, and honourParents is true, apply private patches from the parent.
        /// 2. Apply private patches of this.
        /// 3. If this is a child module, and honourParents is true, apply inherited private patches from the parent.
        /// 4. Apply inherited private patches of this.
        /// 5. If this is a child module, and honourParents is true, apply public patches from the parent.
        /// 6. Apply public patches of this.
        /// 7. If this is a child module, and honourParents is true, apply any inherited patches from the parent.
        /// 8. Apply inherited public patches of this.
        /// Once all patches have been evaluated, if the module has a closing patch, this is now evaluated. If the module's
        /// parent also has a closing patch, this is also evaluated.
        /// Inherited patches are the mechanism for transient dependencies, where dependencies filter up the module hierarchy.
        /// See UsePublicPatches and UsePublicPatchesPrivately.
        /// </summary>
        /// <param name="settings">Settings object to apply patches to.</param>
        /// <param name="honourParents">If set to <c>true</c>, honourParents takes patches from any parent module
        /// and also invokes those if this module is a child.</param>
        public void
        ApplySettingsPatches(
            Settings settings,
            bool honourParents)
        {
            if (null == settings)
            {
                if (!this.PublicPatches.Any() && this.PrivatePatches.Any())
                {
                    throw new Exception(
                        $"Module {this.ToString()} only has private patches, but has no settings on the module to apply them to"
                    );
                }
                return;
            }
            // Note: first private patches, followed by public patches
            // TODO: they could override each other - anyway to check?
            var parentModule = (this is IChildModule child) && honourParents ? child.Parent : null;
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
                foreach (var patchList in parentModule.PrivateInheritedPatches)
                {
                    foreach (var patch in patchList)
                    {
                        patch(settings, this);
                    }
                }
            }
            foreach (var patchList in this.PrivateInheritedPatches)
            {
                foreach (var patch in patchList)
                {
                    patch(settings, this);
                }
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
                foreach (var patchList in parentModule.PublicInheritedPatches)
                {
                    foreach (var patch in patchList)
                    {
                        patch(settings, this);
                    }
                }
            }
            foreach (var patchList in this.PublicInheritedPatches)
            {
                foreach (var patch in patchList)
                {
                    patch(settings, this);
                }
            }
            if (null != TheClosingPatch)
            {
                TheClosingPatch(settings);
            }
            if (null != parentModule && null != parentModule.TheClosingPatch)
            {
                parentModule.TheClosingPatch(settings);
            }

            // now validate the Settings properties
            try
            {
                this.Settings.Validate();
            }
            catch (Bam.Core.Exception ex)
            {
                throw new Bam.Core.Exception(ex, $"Settings validation failed for module {this.ToString()} because:");
            }
        }

        /// <summary>
        /// Determine the reason why the module should (re)build.
        /// </summary>
        /// <value>The reason to execute.</value>
        public ExecuteReasoning ReasonToExecute { get; protected set; }

        /// <summary>
        /// Get or set the async Task for execution.
        /// </summary>
        /// <value>The execution task.</value>
        public System.Threading.Tasks.Task ExecutionTask { get; set; }

        /// <summary>
        /// Get the async Task for evaluating the module for whether it is up-to-date.
        /// </summary>
        /// <value>The evaluation task.</value>
        public System.Threading.Tasks.Task EvaluationTask { get; private set; }

        /// <summary>
        /// Evaluate the module to determine whether it requires a (re)build.
        /// </summary>
        protected abstract void
        EvaluateInternal();

        /// <summary>
        /// Asynchronously run the module evaluation step.
        /// </summary>
        /// <param name="factory">TaskFactory to create evaluation tasks from.</param>
        public void
        EvaluateAsync(
            System.Threading.Tasks.TaskFactory factory)
        {
            this.EvaluationTask = factory.StartNew(
                () => this.EvaluateInternal()
            );
        }

        /// <summary>
        /// Immediately run the module evaluation step.
        /// </summary>
        public void
        EvaluateImmediate() => this.EvaluateInternal();

        /// <summary>
        /// Get the Environment associated with this module. The same module in different environments will be different
        /// instances of a Module.
        /// </summary>
        /// <value>The build environment.</value>
        public Environment BuildEnvironment { get; private set; }

        private System.Type EncapsulatingType { get; set; }

        /// <summary>
        /// A referenced module is an encapsulating module, and can be considered to be uniquely identifiable by name.
        /// An unreferenced module belongs, in some part, to another module and perhaps many of them exist within the
        /// dependency graph.
        /// For identification, a stack of referenced module type was recorded during dependency graph population, and as
        /// all unreferenced modules belonging to that are created within it's Init function, the encapsulating module type
        /// is at the top of the stack.
        /// Knowledge of the encapsulating module is useful for logical grouping, such as build sub-folder names.
        /// </summary>
        public Module EncapsulatingModule { get; private set; }

        private void
        Complete()
        {
            // modules that are encapsulated, that have settings, and aren't a child (as their parent is also encapsulated,
            // and thus gets this too), inherit the public patches from the encapsulating module,
            // since this is identical behavior to 'using public patches'
            if (this.EncapsulatingModule == this)
            {
                return;
            }
            if (null == this.Settings)
            {
                return;
            }
            if (this is IChildModule)
            {
                return;
            }
            this.UsePublicPatches(this.EncapsulatingModule);
        }

        /// <summary>
        /// Ensures all modules have been 'completed', which ensures that everything is in place, ready for validation.
        /// </summary>
        static public void
        CompleteModules()
        {
            var totalProgress = 3 * Module.Count;
            var scale = 100.0f / totalProgress;
            var progress = 2 * Module.Count;
            foreach (var module in AllModules.Reverse<Module>())
            {
                module.Complete();
                Log.DetailProgress("{0,3}%", (int)((++progress) * scale));
            }
        }

        /// <summary>
        /// Create a TokenizedString associated with this module, using the MacroList in the module.
        /// </summary>
        /// <returns>The tokenized string.</returns>
        /// <param name="format">Format.</param>
        /// <param name="argv">Argv.</param>
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

        /// <summary>
        /// Static utility method to count all modules created. Useful for profiling.
        /// </summary>
        /// <value>The count.</value>
        public static int Count => AllModules.Count;

        /// <summary>
        /// Virtual string property specifying a subdirectory name appended to the macro 'moduleoutputdir'
        /// that can be used to further improve the uniqueness of where built files are written.
        /// Default is null to indicate no subdirectory.
        /// </summary>
        public virtual string CustomOutputSubDirectory => null;

        /// <summary>
        /// Extract the time taken to create the instance of this module.
        /// </summary>
        /// <value>The init time.</value>
        public System.TimeSpan CreationTime { get; private set; }

        /// <summary>
        /// Cache of TokenizedStrings that use macros from this module.
        /// </summary>
        public System.Collections.Generic.Dictionary<System.Int64, TokenizedString> TokenizedStringCacheMap { get; private set; }

        /// <summary>
        /// For the given module type, remove all module instances that are encapsulated by it.
        /// This is used in the case where a module is unable to build.
        /// </summary>
        /// <param name="encapsulatingType">Type of the module that is the encapsulating module type to remove.</param>
        static public void
        RemoveEncapsulatedModules(
            System.Type encapsulatingType)
        {
            var modulesToRemove = AllModules.Where(item => item.EncapsulatingType == encapsulatingType);
            foreach (var i in modulesToRemove.ToList())
            {
                Log.DebugMessage(
                    $"Removing module {i.ToString()} from {encapsulatingType.ToString()}"
                );
                AllModules.Remove(i);
            }
        }

        /// <summary>
        /// Query if a module is valid (exists in internal lists).
        /// </summary>
        /// <param name="module">Module instance to query.</param>
        /// <returns></returns>
        static public bool
        IsValid(
            Module module) => AllModules.Contains(module);

        private void
        InitializeModuleConfiguration()
        {
            this.Configuration = null;
            if (!Graph.Instance.IsReferencedModule(this))
            {
                return;
            }
            var accessConfig = this as IHasModuleConfiguration;
            if (null == accessConfig)
            {
                return;
            }
            var writeType = accessConfig.WriteableClassType;
            if (!writeType.IsClass)
            {
                throw new Exception($"Module configuration writeable type {writeType.ToString()} must be a class");
            }
            if (writeType.IsAbstract)
            {
                throw new Exception($"Module configuration writeable type {writeType.ToString()} must not be abstract");
            }
            if (!typeof(IModuleConfiguration).IsAssignableFrom(writeType))
            {
                throw new Exception(
                    $"Module configuration writeable type {writeType.ToString()} must implement {typeof(IModuleConfiguration).ToString()}"
                );
            }
            if (null == writeType.GetConstructor(new[] { typeof(Environment) }))
            {
                throw new Exception(
                    $"Module configuration writeable type {writeType.ToString()} must define a public constructor accepting a {typeof(Environment).ToString()}"
                );
            }
            var readType = accessConfig.ReadOnlyInterfaceType;
            if (!readType.IsAssignableFrom(writeType))
            {
                throw new Exception(
                    $"Module configuration writeable type {writeType.ToString()} does not implement the readable type {readType.ToString()}"
                );
            }
            this.Configuration = System.Activator.CreateInstance(writeType, new[] { this.BuildEnvironment }) as IModuleConfiguration;
            if (Graph.Instance.OverrideModuleConfiguration != null)
            {
                Graph.Instance.OverrideModuleConfiguration.execute(this.Configuration, this.BuildEnvironment);
            }
        }

        /// <summary>
        /// If a Module's configuration can be overridden by the user, the instance of the class for the Module allowing
        /// that configuration to be updated.
        /// </summary>
        public IModuleConfiguration Configuration { get; set; }

        /// <summary>
        /// Enumerable of Modules that are considered inputs to this Module, as in, they need
        /// to be operated on in order to generate the output(s) of this Module.
        /// By default, this is the list of Dependents (not Required), although subclasses can
        /// override this property to give a more precise meaning.
        /// This default implementation is not aware of path keys of derived Module types
        /// so if a caller requires knowledge of that, it is expected the Module types of interest
        /// will override this property to provide the necessary path keys.
        /// </summary>
        public virtual System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string,Module>> InputModules
        {
            get
            {
                foreach (var module in this.DependentsList)
                {
                    yield return new System.Collections.Generic.KeyValuePair<string, Module>(
                        $"Input module path key not overridden in module {this.ToString()} from dependent {module.ToString()}",
                        module
                    );
                }
            }
        }

        /// <summary>
        /// Create an instance of the Settings class used for the Module.
        /// By default, this will come from the Tool (if defined).
        /// Overriding this function allows Modules to use different Settings classes,
        /// although they must implement the appropriate interfaces.
        /// </summary>
        /// <returns>Instance of the Settings class for this Module.</returns>
        public virtual Settings
        MakeSettings()
        {
            System.Diagnostics.Debug.Assert(null != this.Tool);
            return (this.Tool as ITool).CreateDefaultSettings(this);
        }

        /// <summary>
        /// Allow a Module to specify the working directory in which its Tool is executed.
        /// By default, this is null, meaning that either no working directory is needed
        /// or the call site for Tool execution can specify it.
        /// </summary>
        public virtual TokenizedString WorkingDirectory => null;
    }
}
