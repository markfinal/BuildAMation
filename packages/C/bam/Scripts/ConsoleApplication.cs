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
namespace C
{
    /// <summary>
    /// Derive from this class to generate a console application and link against the C runtime library.
    /// On Windows, a versioning resource file is generated and linked into this binary. If a macro 'Description'
    /// exists on this module, then it is used for the 'FileDescription' field in the resource file.
    /// </summary>
    public class ConsoleApplication :
        CModule
    {
        protected Bam.Core.Array<Bam.Core.Module> sourceModules = new Bam.Core.Array<Bam.Core.Module>();
        private Bam.Core.Array<Bam.Core.Module> linkedModules = new Bam.Core.Array<Bam.Core.Module>();
#if BAM_V2
#else
        private ILinkingPolicy Policy = null;
#endif

        static public Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("ExecutableFile");
        static public Bam.Core.PathKey ImportLibraryKey = Bam.Core.PathKey.Generate("Windows Import Library File");
        static public Bam.Core.PathKey PDBKey = Bam.Core.PathKey.Generate("Windows Program DataBase File");

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.RegisterGeneratedFile(Key, this.CreateTokenizedString("$(packagebuilddir)/$(moduleoutputdir)/$(OutputName)$(exeext)"));
            this.Linker = DefaultToolchain.C_Linker(this.BitDepth);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                Bam.Core.Graph.Instance.Mode != "Xcode")
            {
                if (this.Linker.Macros.Contains("pdbext"))
                {
                    this.RegisterGeneratedFile(PDBKey, this.IsPrebuilt ? null : this.CreateTokenizedString("@changeextension($(0),$(pdbext))", this.GeneratedPaths[Key]));
                }

                if (!this.IsPrebuilt)
                {
                    var rcContainer = this.CreateWinResourceContainer();
                    if (this.ThirdpartyWindowsVersionResourcePath != null)
                    {
                        var versionRC = rcContainer.AddFiles(this.ThirdpartyWindowsVersionResourcePath);
                        this.WindowsVersionResource = versionRC[0] as WinResource;
                    }
                    else
                    {
                        var versionSource = Bam.Core.Module.Create<WinVersionResource>(preInitCallback: module =>
                            {
                                module.BinaryModule = this;
                                module.InputPath = this.CreateTokenizedString("$(packagebuilddir)/$(config)/$(OutputName)_version.rc");
                            });
                        var versionRC = rcContainer.AddFile(versionSource);
                        this.WindowsVersionResource = versionRC;
                    }
                }
            }
            this.PrivatePatch(settings =>
                {
                    var linker = settings as C.ICommonLinkerSettings;
                    linker.OutputType = ELinkerOutput.Executable;
                });
        }

        public override string CustomOutputSubDirectory
        {
            get
            {
                return "bin";
            }
        }

        /// <summary>
        /// Access the headers files associated with this executable.
        /// </summary>
        public System.Collections.Generic.IEnumerable<Bam.Core.Module>
        HeaderFiles
        {
            get
            {
                var module_list = FlattenHierarchicalFileList(this.headerModules);
                foreach (var module in module_list)
                {
                    yield return module;
                }
            }
        }

        /// <summary>
        /// Access the object files required to create this executable.
        /// </summary>
        public System.Collections.Generic.IEnumerable<Bam.Core.Module>
        ObjectFiles
        {
            get
            {
                var module_list = FlattenHierarchicalFileList(this.sourceModules);
                foreach (var module in module_list)
                {
                    yield return module;
                }
            }
        }

        /// <summary>
        /// Access the built libraries required to create this executable.
        /// </summary>
        public System.Collections.Generic.IEnumerable<Bam.Core.Module>
        Libraries
        {
            get
            {
                // some linkers require a specific order of libraries in order to resolve symbols
                // so that if an existing library is later referenced, it needs to be moved later
                var module_list = OrderLibrariesWithDecreasingDependencies(this.linkedModules);
                foreach (var module in module_list)
                {
                    yield return module;
                }
            }
        }

        protected Bam.Core.Module.PrivatePatchDelegate ConsolePreprocessor = settings =>
            {
                var compiler = settings as C.ICommonCompilerSettings;
                compiler.PreprocessorDefines.Add("_CONSOLE");
            };

        /// <summary>
        /// Create a container for matching source files, for preprocessed assembly.
        /// </summary>
        /// <returns>The assembled source container.</returns>
        /// <param name="wildcardPath">Wildcard path.</param>
        /// <param name="macroModuleOverride">Macro module override.</param>
        /// <param name="filter">Filter.</param>
        public virtual AssembledObjectFileCollection
        CreateAssemblerSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var source = this.InternalCreateContainer<AssembledObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter, null);
            this.sourceModules.Add(source);
            return source;
        }

        /// <summary>
        /// Create a container for matching source files, to compile as C.
        /// </summary>
        /// <returns>The C source container.</returns>
        /// <param name="wildcardPath">Wildcard path.</param>
        /// <param name="macroModuleOverride">Macro module override.</param>
        /// <param name="filter">Filter.</param>
        public virtual CObjectFileCollection
        CreateCSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var applicationPreprocessor = this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) ? this.ConsolePreprocessor : null;
            var source = this.InternalCreateContainer<CObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter, applicationPreprocessor);
            this.sourceModules.Add(source);
            return source;
        }

        /// <summary>
        /// Create a container for matching source files, to compile as ObjectiveC.
        /// </summary>
        /// <returns>The objective C source container.</returns>
        /// <param name="wildcardPath">Wildcard path.</param>
        /// <param name="macroModuleOverride">Macro module override.</param>
        /// <param name="filter">Filter.</param>
        public C.ObjC.ObjectFileCollection
        CreateObjectiveCSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var applicationPreprocessor = this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) ? this.ConsolePreprocessor : null;
            var source = this.InternalCreateContainer<C.ObjC.ObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter, applicationPreprocessor);
            this.sourceModules.Add(source);
            return source;
        }

        /// <summary>
        /// Create a container for matching Windows resource files.
        /// </summary>
        /// <returns>The window resource container.</returns>
        /// <param name="wildcardPath">Wildcard path.</param>
        /// <param name="macroModuleOverride">Macro module override.</param>
        /// <param name="filter">Filter.</param>
        public virtual WinResourceCollection
        CreateWinResourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var source = this.InternalCreateContainer<WinResourceCollection>(false, wildcardPath, macroModuleOverride, filter);
            this.sourceModules.Add(source);
            return source;
        }

        /// <summary>
        /// Specified source modules are compiled against the DependentModule type, with any public patches
        /// of that type applied to each source.
        /// </summary>
        /// <param name="affectedSource">Required source module.</param>
        /// <param name="affectedSources">Optional list of additional sources.</param>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public void
        CompileAgainst<DependentModule>(
            CModule affectedSource,
            params CModule[] additionalSources) where DependentModule : HeaderLibrary, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            this.Requires(dependent);
            var sources = new CModule[additionalSources.Length + 1];
            sources[0] = affectedSource;
            if (additionalSources.Length > 0)
            {
                additionalSources.CopyTo(sources, 1);
            }
            foreach (var source in sources)
            {
                if (null == source)
                {
                    continue;
                }
                source.UsePublicPatches(dependent);
            }
        }

        protected void
        addLinkDependency(
            Bam.Core.Module dependent)
        {
            if (dependent is IDynamicLibrary)
            {
                var dynamicLib = dependent as IDynamicLibrary;
                if (dynamicLib.LinkerNameSymbolicLink != null)
                {
                    this.DependsOn(dynamicLib.LinkerNameSymbolicLink);
                }
                // else is non-Linux platforms
            }
            else
            {
                this.DependsOn(dependent);
            }
        }

        protected void
        addRuntimeDependency(
            Bam.Core.Module dependent)
        {
            if (dependent is IDynamicLibrary)
            {
                var dynamicLib = dependent as IDynamicLibrary;
                if (dynamicLib.SONameSymbolicLink != null)
                {
                    this.Requires(dynamicLib.SONameSymbolicLink);
                    return;
                }
            }
            this.Requires(dependent);
        }

        /// <summary>
        /// Application is linked against the DependentModule type.
        /// Any public patches of DependentModule are applied privately to the Application.
        /// </summary>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public void
        LinkAgainst<DependentModule>() where DependentModule : CModule, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            this.addLinkDependency(dependent);
            this.addRuntimeDependency(dependent);
            this.LinkAllForwardedDependenciesFromLibraries(dependent);
            this.UsePublicPatchesPrivately(dependent);
        }

        /// <summary>
        /// Application requires the DependentModule type to exist.
        /// The affected sources list for applying the dependent's public patch to is optional.
        /// </summary>
        /// <param name="affectedSources">Optional list of additional sources.</param>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public void
        RequiredToExist<DependentModule>(
            params CModule[] affectedSources) where DependentModule : CModule, new()
        {
            try
            {
                var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
                if (null == dependent)
                {
                    return;
                }
                this.addRuntimeDependency(dependent);
                foreach (var source in affectedSources)
                {
                    if (null == source)
                    {
                        continue;
                    }
                    source.UsePublicPatches(dependent);
                }
            }
            catch (Bam.Core.UnableToBuildModuleException exception)
            {
                Bam.Core.Log.Info("Unable to build {0} required by {1} because {2}, but the build will continue",
                    typeof(DependentModule).ToString(),
                    this.GetType().ToString(),
                    exception.Message);
            }
        }

        /// <summary>
        /// Specified sources and the application compiles and links against the DependentModule.
        /// </summary>
        /// <param name="affectedSource">Required source module.</param>
        /// <param name="additionalSources">Optional list of additional sources.</param>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public void
        CompileAndLinkAgainst<DependentModule>(
            CModule affectedSource,
            params CModule[] additionalSources) where DependentModule : CModule, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            this.addLinkDependency(dependent);
            this.addRuntimeDependency(dependent);
            var sources = new CModule[additionalSources.Length + 1];
            sources[0] = affectedSource;
            if (additionalSources.Length > 0)
            {
                additionalSources.CopyTo(sources, 1);
            }
            foreach (var source in sources)
            {
                if (null == source)
                {
                    continue;
                }
                source.UsePublicPatches(dependent);
            }
            this.LinkAllForwardedDependenciesFromLibraries(dependent);
            this.UsePublicPatchesPrivately(dependent);
        }

        protected void
        LinkAllForwardedDependenciesFromLibraries(
            Bam.Core.Module module)
        {
            this.linkedModules.AddUnique(module);
            var withForwarded = module as IForwardedLibraries;
            if (null == withForwarded)
            {
                return;
            }

            // recursive
            foreach (var forwarded in withForwarded.ForwardedLibraries)
            {
                this.addLinkDependency(forwarded);
                this.addRuntimeDependency(forwarded);
                this.linkedModules.AddUnique(forwarded);
                this.LinkAllForwardedDependenciesFromLibraries(forwarded);
            }
        }

        /// <summary>
        /// Extend a container of C object files with another, potentially from another module. Note that module types must match.
        /// Private patches are inherited.
        /// Public patches are used internally to compile against, but are not exposed further.
        /// Note that the referenced module in DependentModule will have its source files marked as PerformCompilation=false
        /// so that it is not attempted to be built standalone.
        /// </summary>
        /// <typeparam name="DependentModule">Container module type to embed into the specified container.</typeparam>
        /// <param name="affectedSource">Container to be extended.</param>
        public void
        ExtendSource<DependentModule>(
            CModuleContainer<ObjectFile> affectedSource)
            where DependentModule : CModuleContainer<ObjectFile>, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }

            // as the referenced container of source is to be shoehorned into
            // this module, make sure that the external container isn't built standalone
            // note that the referenced container will float to the top of the dependency
            // graph, but just won't do anything
            foreach (var child in dependent.Children)
            {
                var childAsObjectFile = child as ObjectFileBase;
                if (null == childAsObjectFile)
                {
                    continue;
                }
                childAsObjectFile.PerformCompilation = false;
            }

            affectedSource.ExtendWith(dependent);
            affectedSource.UsePublicPatchesPrivately(dependent);
        }

        public LinkerTool Linker
        {
            get
            {
                return this.Tool as LinkerTool;
            }
            set
            {
                this.Tool = value;
            }
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
#if BAM_V2
            switch (Bam.Core.Graph.Instance.Mode)
            {
#if D_PACKAGE_MAKEFILEBUILDER
                case "MakeFile":
                    MakeFileSupport.Link(this);
                    break;
#endif

#if D_PACKAGE_NATIVEBUILDER
                case "Native":
                    NativeSupport.Link(this, context);
                    break;
#endif

#if D_PACKAGE_VSSOLUTIONBUILDER
                case "VSSolution":
                    VSSolutionSupport.Link(this);
                    break;
#endif

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    //XcodeSupport.Link(this);
                    break;
#endif

                default:
                    throw new System.NotImplementedException();
            }
#else
            if (this.IsPrebuilt &&
                !((this.headerModules.Count > 0) && Bam.Core.Graph.Instance.BuildModeMetaData.CanCreatePrebuiltProjectForAssociatedFiles))
            {
                return;
            }
            var source = FlattenHierarchicalFileList(this.sourceModules).ToReadOnlyCollection();
            var headers = FlattenHierarchicalFileList(this.headerModules).ToReadOnlyCollection();
            // some linkers require a specific order of libraries in order to resolve symbols
            // so that if an existing library is later referenced, it needs to be moved later
            var linked = OrderLibrariesWithDecreasingDependencies(this.linkedModules);
            var executable = this.GeneratedPaths[Key];
            this.Policy.Link(this, context, executable, source, headers, linked);
#endif
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
#if BAM_V2
#else
            if (this.IsPrebuilt &&
                !((this.headerModules.Count > 0) && Bam.Core.Graph.Instance.BuildModeMetaData.CanCreatePrebuiltProjectForAssociatedFiles))
            {
                return;
            }
            var className = "C." + mode + "Linker";
            this.Policy = Bam.Core.ExecutionPolicyUtilities<ILinkingPolicy>.Create(className);
#endif
        }

        protected override void
        EvaluateInternal()
        {
            this.ReasonToExecute = null;
            if (this.IsPrebuilt) // never check, even if there are headers
            {
                return;
            }
            var binaryPath = this.GeneratedPaths[Key].ToString();
            var exists = System.IO.File.Exists(binaryPath);
            if (!exists)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
                return;
            }
            var binaryWriteTime = System.IO.File.GetLastWriteTime(binaryPath);
            foreach (var source in this.linkedModules)
            {
                if (null != source.EvaluationTask)
                {
                    source.EvaluationTask.Wait();
                }
                if (null != source.ReasonToExecute)
                {
                    switch (source.ReasonToExecute.Reason)
                    {
                        case ExecuteReasoning.EReason.FileDoesNotExist:
                        case ExecuteReasoning.EReason.InputFileIsNewer:
                            this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], source.ReasonToExecute.OutputFilePath);
                            return;

                        default:
                            throw new Bam.Core.Exception("Unknown reason, {0}", source.ReasonToExecute.Reason.ToString());
                    }
                }
                // TODO: there could be a chance that a library is up-to-date from a previous build, and yet
                // it is not detected for a link step
                // however, the inputs here could be static libraries, or dynamic libraries, or import libraries
                // so there is more variety in the Generated key enumeration
            }
            foreach (var source in this.sourceModules)
            {
                if (null != source.EvaluationTask)
                {
                    source.EvaluationTask.Wait();
                }
                if (null != source.ReasonToExecute)
                {
                    switch (source.ReasonToExecute.Reason)
                    {
                        case ExecuteReasoning.EReason.FileDoesNotExist:
                        case ExecuteReasoning.EReason.InputFileIsNewer:
                            this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], source.ReasonToExecute.OutputFilePath);
                            return;

                        default:
                            throw new Bam.Core.Exception("Unknown reason, {0}", source.ReasonToExecute.Reason.ToString());
                    }
                }
                else
                {
                    // if an object file is built, but for some reason (e.g. previous build failure), not been linked
                    if (source is Bam.Core.IModuleGroup)
                    {
                        foreach (var objectFile in source.Children)
                        {
                            var objectFilePath = objectFile.GeneratedPaths[ObjectFile.Key].ToString();
                            var objectFileWriteTime = System.IO.File.GetLastWriteTime(objectFilePath);
                            if (objectFileWriteTime > binaryWriteTime)
                            {
                                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], objectFile.GeneratedPaths[ObjectFile.Key]);
                                return;
                            }
                        }
                    }
                    else
                    {
                        source.GeneratedPaths[ObjectFile.Key].Parse();
                        var objectFilePath = source.GeneratedPaths[ObjectFile.Key].ToString();
                        var objectFileWriteTime = System.IO.File.GetLastWriteTime(objectFilePath);
                        if (objectFileWriteTime > binaryWriteTime)
                        {
                            this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], source.GeneratedPaths[ObjectFile.Key]);
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Define the working directory to use in IDE projects for debugging (if supported).
        /// </summary>
        virtual public Bam.Core.TokenizedString WorkingDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Reference to the module generated internally for Windows versioning of this binary.
        /// This can be used to attach local patches or dependencies, e.g. to satisfy header search paths.
        /// </summary>
        public WinResource WindowsVersionResource
        {
            get;
            private set;
        }
    }
}
