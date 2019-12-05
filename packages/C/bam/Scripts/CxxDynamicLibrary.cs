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
namespace C.Cxx
{
    /// <summary>
    /// Derive from this module to create a C++ dynamic library, linking against the C++ runtime.
    /// </summary>
    class DynamicLibrary :
        ConsoleApplication,
        IDynamicLibrary,
        IForwardedLibraries
    {
        private readonly Bam.Core.Array<Bam.Core.Module> forwardedDeps = new Bam.Core.Array<Bam.Core.Module>();

        /// <summary>
        /// Initialize the dynamic library
        /// </summary>
        protected override void
        Init()
        {
            base.Init();
            this.Linker = C.DefaultToolchain.Cxx_Linker(this.BitDepth);

            this.RegisterGeneratedFile(
                ExecutableKey,
                this.CreateTokenizedString(
                    "$(packagebuilddir)/$(moduleoutputdir)/$(dynamicprefix)$(OutputName)$(dynamicext)"
                ),
                true
            );
            this.Macros.Add("LinkOutput", this.GeneratedPaths[ExecutableKey]);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Macros.Add("ImportLibraryName", Bam.Core.TokenizedString.Create("$(OutputName)", null));
                this.RegisterGeneratedFile(ImportLibraryKey, this.CreateTokenizedString("$(packagebuilddir)/$(moduleoutputdir)/$(libprefix)$(ImportLibraryName)$(libext)"), false);
            }

            this.PrivatePatch(settings =>
            {
                if (settings is C.ICommonLinkerSettings linker)
                {
                    linker.OutputType = ELinkerOutput.DynamicLibrary;
                }
            });
        }

        /// <summary>
        /// Create a collection whose matching source compiles against C.
        /// </summary>
        /// <returns>The C source collection.</returns>
        /// <param name="wildcardPath">Wildcard path.</param>
        /// <param name="macroModuleOverride">Macro module override.</param>
        /// <param name="filter">Filter.</param>
        [System.Obsolete("Please use CreateCSourceCollection instead", true)]
        public sealed override CObjectFileCollection
        CreateCSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            return this.CreateCSourceCollection(wildcardPath, macroModuleOverride, filter);
        }

        /// <summary>
        /// Create a collection whose matching source compiles against C.
        /// </summary>
        /// <returns>The C source collection.</returns>
        /// <param name="wildcardPath">Wildcard path.</param>
        /// <param name="macroModuleOverride">Macro module override.</param>
        /// <param name="filter">Filter.</param>
        public sealed override CObjectFileCollection
        CreateCSourceCollection(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var collection = base.CreateCSourceCollection(wildcardPath, macroModuleOverride, filter);
            collection.PrivatePatch(settings =>
            {
                var preprocessor = settings as C.ICommonPreprocessorSettings;
                preprocessor.PreprocessorDefines.Add("D_BAM_DYNAMICLIBRARY_BUILD");
                (collection.Tool as C.CompilerTool).CompileAsShared(settings);
            });
            return collection;
        }

        /// <summary>
        /// Create a collection whose matching source compiles against C++.
        /// </summary>
        /// <returns>The cxx source collection.</returns>
        /// <param name="wildcardPath">Wildcard path.</param>
        /// <param name="macroModuleOverride">Macro module override.</param>
        /// <param name="filter">Filter.</param>
        [System.Obsolete("Please use CreateCxxSourceCollection instead", true)]
        public sealed override Cxx.ObjectFileCollection
        CreateCxxSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            return this.CreateCxxSourceCollection(wildcardPath, macroModuleOverride, filter);
        }

        /// <summary>
        /// Create a collection whose matching source compiles against C++.
        /// </summary>
        /// <returns>The cxx source collection.</returns>
        /// <param name="wildcardPath">Wildcard path.</param>
        /// <param name="macroModuleOverride">Macro module override.</param>
        /// <param name="filter">Filter.</param>
        public sealed override Cxx.ObjectFileCollection
        CreateCxxSourceCollection(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var collection = base.CreateCxxSourceCollection(wildcardPath, macroModuleOverride, filter);
            collection.PrivatePatch(settings =>
            {
                var preprocessor = settings as C.ICommonPreprocessorSettings;
                preprocessor.PreprocessorDefines.Add("D_BAM_DYNAMICLIBRARY_BUILD");
                (collection.Tool as C.CompilerTool).CompileAsShared(settings);
            });
            return collection;
        }

        /// <summary>
        /// Specified sources compile against DependentModule, and re-exports the public patches
        /// from the dependent, e.g. if the headers of the dynamic library require include paths from
        /// the dependent.
        /// </summary>
        /// <param name="affectedSource">Required source module.</param>
        /// <param name="additionalSources">Optional list of additional sources.</param>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public void
        CompileAgainstPublicly<DependentModule>(
            CModule affectedSource,
            params CModule[] additionalSources) where DependentModule : CModule, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            this.DependsOn(dependent);
            System.Collections.Generic.IEnumerable<CModule>
            EnumerateSources(
                CModule affectedSourceLocal,
                params CModule[] additionalSourcesLocal)
            {
                yield return affectedSourceLocal;
                foreach (var source in additionalSourcesLocal)
                {
                    yield return source;
                }
            }
            foreach (var source in EnumerateSources(affectedSource, additionalSources))
            {
                source?.UsePublicPatches(dependent);
            }
            this.UsePublicPatches(dependent);
        }

        /// <summary>
        /// Specified sources and the application compiles and links against the DependentModule, and the
        /// application uses patches from the dependent.
        /// </summary>
        /// <param name="affectedSource">Required source module.</param>
        /// <param name="additionalSources">Optional list of additional sources.</param>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public void
        CompilePubliclyAndLinkAgainst<DependentModule>(
            CModule affectedSource,
            params CModule[] additionalSources) where DependentModule : CModule, IExportableCModule, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            if (dependent is C.DynamicLibrary || dependent is C.Cxx.DynamicLibrary)
            {
                this.forwardedDeps.AddUnique(dependent);
            }
            this.CompileAndLinkAgainst<DependentModule>(affectedSource, additionalSources);
            this.UsePublicPatches(dependent);
        }

        /// <summary>
        /// DynamicLibrary is linked against the DependentModule type.
        /// Any public patches of DependentModule are applied publicly to the DynamicLibrary, and will be inherited
        /// by any module depending on the DynamicLibrary.
        /// </summary>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public void
        LinkPubliclyAgainst<DependentModule>() where DependentModule : CModule, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            this.AddLinkDependency(dependent);
            this.AddRuntimeDependency(dependent);
            if (dependent is C.DynamicLibrary || dependent is C.Cxx.DynamicLibrary)
            {
                this.forwardedDeps.AddUnique(dependent);
            }
            this.LinkAllForwardedDependenciesFromLibraries(dependent);
            this.UsePublicPatches(dependent);
        }

        /// <summary>
        /// Extend a collection of C++ object files with another, potentially from another module. Note that module types must match.
        /// Private patches are inherited.
        /// Public patches are both used internally, and also forwarded onto any callers of this module.
        /// </summary>
        /// <typeparam name="DependentModule">Collection module type to embed into the specified collection.</typeparam>
        /// <param name="affectedSource">Collection to be extended.</param>
        public void
        ExtendSourcePublicly<DependentModule>(
            ObjectFileCollection affectedSource) where DependentModule : ObjectFileCollection, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            affectedSource.ExtendWith(dependent);
            this.UsePublicPatches(dependent);
        }

        /// <summary>
        /// Execute the link of the dynamic library
        /// </summary>
        /// <param name="context"></param>
        protected sealed override void
        ExecuteInternal(
            ExecutionContext context)
        {
            if (this.IsPrebuilt &&
                !((this.headerModules.Count > 0) && Bam.Core.Graph.Instance.BuildModeMetaData.CanCreatePrebuiltProjectForAssociatedFiles))
            {
                return;
            }
            base.ExecuteInternal(context);
        }

        System.Collections.Generic.IEnumerable<Bam.Core.Module> IForwardedLibraries.ForwardedLibraries => this.forwardedDeps;

        void
        IDynamicLibrary.ChangeRootPaths(
            Bam.Core.TokenizedString binDirectory,
            Bam.Core.TokenizedString libDirectory
        )
        {
            this.RegisterGeneratedFile(
                ExecutableKey,
                this.CreateTokenizedString(
                    "$(0)/$(dynamicprefix)$(OutputName)$(dynamicext)",
                    binDirectory
                ),
                true
            );
            if (this.GeneratedPaths.ContainsKey(ImportLibraryKey))
            {
                this.RegisterGeneratedFile(
                    ImportLibraryKey,
                    this.CreateTokenizedString(
                        "$(0)/$(libprefix)$(ImportLibraryName)$(libext)",
                        libDirectory
                    ),
                    false
                );
            }
        }

        /// <summary>
        /// /copydoc Bam.Core.Module.NoBuildDependentsFilter
        /// </summary>
        protected override System.Type[] NoBuildDependentsFilter
        {
            get
            {
                return new System.Type[] { typeof(SDKTemplate), typeof(HeaderFileCollection) };
            }
        }
    }
}
