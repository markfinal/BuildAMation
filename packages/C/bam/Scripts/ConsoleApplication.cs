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
using Bam.Core;
using System.Linq;
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
        /// <summary>
        /// list of source Modules
        /// </summary>
        protected Bam.Core.Array<Bam.Core.Module> sourceModules = new Bam.Core.Array<Bam.Core.Module>();
        private readonly Bam.Core.Array<Bam.Core.Module> linkedModules = new Bam.Core.Array<Bam.Core.Module>();

        /// <summary>
        /// Path key representing the executable
        /// </summary>
        public const string ExecutableKey = "Executable File";

        /// <summary>
        /// Path key representing the Windows import library for a DLL
        /// </summary>
        public const string ImportLibraryKey = "Windows Import Library File";

        /// <summary>
        /// Path key representing the Windows program database
        /// </summary>
        public const string PDBKey = "Windows Program DataBase File";

        /// <summary>
        /// Initialize the console application
        /// </summary>
        protected override void
        Init()
        {
            base.Init();
            this.RegisterGeneratedFile(
                ExecutableKey,
                this.CreateTokenizedString("$(packagebuilddir)/$(moduleoutputdir)/$(OutputName)$(exeext)")
            );
            this.Linker = DefaultToolchain.C_Linker(this.BitDepth);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                Bam.Core.Graph.Instance.Mode != "Xcode")
            {
                if (this.Linker.Macros.Contains("pdbext"))
                {
                    this.RegisterGeneratedFile(
                        PDBKey,
                        this.IsPrebuilt ?
                            null :
                            this.CreateTokenizedString("@changeextension($(0),$(pdbext))", this.GeneratedPaths[ExecutableKey])
                        );
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

        /// <summary>
        /// Override the name of the subdirectory containing executables
        /// </summary>
        public override string CustomOutputSubDirectory => "bin";

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
        /// Enumerate across all input Modules to this application
        /// </summary>
        public override System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>> InputModules
        {
            get
            {
                foreach (var obj in this.ObjectFiles.Where(item => (item as ObjectFileBase).PerformCompilation))
                {
                    yield return new System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>(C.ObjectFileBase.ObjectFileKey, obj);
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

        /// <summary>
        /// Private patch to mark console applications (as opposed to Windowed)
        /// </summary>
        protected Bam.Core.Module.PrivatePatchDelegate ConsolePreprocessor = settings =>
            {
                var preprocessor = settings as C.ICommonPreprocessorSettings;
                preprocessor.PreprocessorDefines.Add("_CONSOLE");
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
        /// <param name="additionalSources">Optional list of additional sources.</param>
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
                source?.UsePublicPatches(dependent);
            }
        }

        /// <summary>
        /// Add a module as a link dependency.
        /// </summary>
        /// <param name="dependent">Module to add</param>
        protected void
        AddLinkDependency(
            Bam.Core.Module dependent)
        {
            if (dependent is IDynamicLibrary dynamicLib)
            {
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

        /// <summary>
        /// Add a runtime dependency (order only). No link step.
        /// </summary>
        /// <param name="dependent">Module that is a runtime dependency.</param>
        protected void
        AddRuntimeDependency(
            Bam.Core.Module dependent)
        {
            if (dependent is IDynamicLibrary dynamicLib)
            {
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
            this.AddLinkDependency(dependent);
            this.AddRuntimeDependency(dependent);
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
                this.AddRuntimeDependency(dependent);
                foreach (var source in affectedSources)
                {
                    source?.UsePublicPatches(dependent);
                }
            }
            catch (Bam.Core.UnableToBuildModuleException exception)
            {
                Bam.Core.Log.Info(
                    $"Unable to build {typeof(DependentModule).ToString()} required by {this.GetType().ToString()} because {exception.Message}, but the build will continue"
                );
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
            this.AddLinkDependency(dependent);
            this.AddRuntimeDependency(dependent);
            var sources = new CModule[additionalSources.Length + 1];
            sources[0] = affectedSource;
            if (additionalSources.Length > 0)
            {
                additionalSources.CopyTo(sources, 1);
            }
            foreach (var source in sources)
            {
                source?.UsePublicPatches(dependent);
            }
            this.LinkAllForwardedDependenciesFromLibraries(dependent);
            this.UsePublicPatchesPrivately(dependent);
        }

        /// <summary>
        /// Add link dependencies to all forwarded modules.
        /// </summary>
        /// <param name="module">Module to find forwarded libraries from.</param>
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
                this.AddLinkDependency(forwarded);
                this.AddRuntimeDependency(forwarded);
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
                if (child is ObjectFileBase childAsObjectFile)
                {
                    childAsObjectFile.PerformCompilation = false;
                }
            }

            affectedSource.ExtendWith(dependent);
            affectedSource.UsePublicPatchesPrivately(dependent);
        }

        /// <summary>
        /// Get or set the linker tool
        /// </summary>
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

        /// <summary>
        /// Execute the link step.
        /// </summary>
        /// <param name="context">In this context.</param>
        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            switch (Bam.Core.Graph.Instance.Mode)
            {
#if D_PACKAGE_MAKEFILEBUILDER
                case "MakeFile":
                    {
                        if (this.IsPrebuilt)
                        {
                            return;
                        }
                        // any libraries added prior to here, need to be moved to the end
                        // they are external dependencies, and thus all built modules (to be added now) may have
                        // a dependency on them (and not vice versa)
                        var linker = this.Settings as C.ICommonLinkerSettings;
                        var externalLibs = linker.Libraries;
                        linker.Libraries = new Bam.Core.StringArray();
                        foreach (var library in this.Libraries)
                        {
                            (this.Tool as C.LinkerTool).ProcessLibraryDependency(this as CModule, library as CModule);
                        }
                        linker.Libraries.AddRange(externalLibs);

                        MakeFileBuilder.Support.Add(this);
                    }
                    break;
#endif

#if D_PACKAGE_NATIVEBUILDER
                case "Native":
                    {
                        // any libraries added prior to here, need to be moved to the end
                        // they are external dependencies, and thus all built modules (to be added now) may have
                        // a dependency on them (and not vice versa)
                        var linker = this.Settings as C.ICommonLinkerSettings;
                        var externalLibs = linker.Libraries;
                        linker.Libraries = new Bam.Core.StringArray();
                        foreach (var library in this.Libraries)
                        {
                            (this.Tool as C.LinkerTool).ProcessLibraryDependency(this as CModule, library as CModule);
                        }
                        linker.Libraries.AddRange(externalLibs);

                        NativeBuilder.Support.RunCommandLineTool(this, context);
                    }
                    break;
#endif

#if D_PACKAGE_VSSOLUTIONBUILDER
                case "VSSolution":
                    VSSolutionSupport.Link(this);
                    break;
#endif

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    XcodeSupport.Link(this);
                    break;
#endif

                default:
                    throw new System.NotImplementedException();
            }
        }

        // Determine if the application needs to be linked
        protected override void
        EvaluateInternal()
        {
            this.ReasonToExecute = null;
            if (this.IsPrebuilt) // never check, even if there are headers
            {
                return;
            }
            var binaryPath = this.GeneratedPaths[ExecutableKey].ToString();
            var exists = System.IO.File.Exists(binaryPath);
            if (!exists)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[ExecutableKey]);
                return;
            }
            var binaryWriteTime = System.IO.File.GetLastWriteTime(binaryPath);
            foreach (var source in this.linkedModules)
            {
                source.EvaluationTask?.Wait();
                if (null != source.ReasonToExecute)
                {
                    switch (source.ReasonToExecute.Reason)
                    {
                        case ExecuteReasoning.EReason.FileDoesNotExist:
                        case ExecuteReasoning.EReason.InputFileIsNewer:
                            this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                                this.GeneratedPaths[ExecutableKey],
                                source.ReasonToExecute.OutputFilePath
                            );
                            return;

                        default:
                            throw new Bam.Core.Exception($"Unknown reason, {source.ReasonToExecute.Reason.ToString()}");
                    }
                }
                // TODO: there could be a chance that a library is up-to-date from a previous build, and yet
                // it is not detected for a link step
                // however, the inputs here could be static libraries, or dynamic libraries, or import libraries
                // so there is more variety in the Generated key enumeration
            }
            foreach (var source in this.sourceModules)
            {
                source.EvaluationTask?.Wait();
                if (null != source.ReasonToExecute)
                {
                    switch (source.ReasonToExecute.Reason)
                    {
                        case ExecuteReasoning.EReason.FileDoesNotExist:
                        case ExecuteReasoning.EReason.InputFileIsNewer:
                            this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                                this.GeneratedPaths[ExecutableKey],
                                source.ReasonToExecute.OutputFilePath
                            );
                            return;

                        default:
                            throw new Bam.Core.Exception($"Unknown reason, {source.ReasonToExecute.Reason.ToString()}");
                    }
                }
                else
                {
                    // if an object file is built, but for some reason (e.g. previous build failure), not been linked
                    if (source is Bam.Core.IModuleGroup)
                    {
                        foreach (var objectFile in source.Children)
                        {
                            var objectFilePath = objectFile.GeneratedPaths[ObjectFile.ObjectFileKey].ToString();
                            var objectFileWriteTime = System.IO.File.GetLastWriteTime(objectFilePath);
                            if (objectFileWriteTime > binaryWriteTime)
                            {
                                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                                    this.GeneratedPaths[ExecutableKey],
                                    objectFile.GeneratedPaths[ObjectFile.ObjectFileKey]
                                );
                                return;
                            }
                        }
                    }
                    else
                    {
                        source.GeneratedPaths[ObjectFile.ObjectFileKey].Parse();
                        var objectFilePath = source.GeneratedPaths[ObjectFile.ObjectFileKey].ToString();
                        var objectFileWriteTime = System.IO.File.GetLastWriteTime(objectFilePath);
                        if (objectFileWriteTime > binaryWriteTime)
                        {
                            this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                                this.GeneratedPaths[ExecutableKey],
                                source.GeneratedPaths[ObjectFile.ObjectFileKey]
                            );
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reference to the module generated internally for Windows versioning of this binary.
        /// This can be used to attach local patches or dependencies, e.g. to satisfy header search paths.
        /// </summary>
        public WinResource WindowsVersionResource { get; private set; }
    }
}
