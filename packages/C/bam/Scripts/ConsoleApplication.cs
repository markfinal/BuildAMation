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
using Bam.Core;
namespace C
{
    /// <summary>
    /// Derive from this class to generate a console application and link against the C runtime library.
    /// </summary>
    public class ConsoleApplication :
        CModule
    {
        protected Bam.Core.Array<Bam.Core.Module> sourceModules = new Bam.Core.Array<Bam.Core.Module>();
        private Bam.Core.Array<Bam.Core.Module> linkedModules = new Bam.Core.Array<Bam.Core.Module>();
        private ILinkingPolicy Policy = null;

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
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                if (this.Linker.Macros.Contains("pdbext"))
                {
                    this.RegisterGeneratedFile(PDBKey, this.IsPrebuilt ? null : this.CreateTokenizedString("@changeextension($(0),$(pdbext))", this.GeneratedPaths[Key]));
                }
            }
            this.PrivatePatch(settings =>
            {
                var linker = settings as C.ICommonLinkerSettings;
                linker.OutputType = ELinkerOutput.Executable;
            });
        }

        protected Bam.Core.Module.PrivatePatchDelegate ConsolePreprocessor = settings =>
            {
                var compiler = settings as C.ICommonCompilerSettings;
                compiler.PreprocessorDefines.Add("_CONSOLE");
            };

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

        /// <summary>
        /// Application is linked against the DependentModule type.
        /// </summary>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public void
        LinkAgainst<DependentModule>() where DependentModule : CModule, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            this.DependsOn(dependent);
            this.linkedModules.Add(dependent);
            this.UsePublicPatches(dependent);
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
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            this.Requires(dependent);
            foreach (var source in affectedSources)
            {
                if (null == source)
                {
                    continue;
                }
                source.UsePublicPatches(dependent);
            }
        }

        /// <summary>
        /// Specified sources and the application compiles and links against the DependentModule.
        /// </summary>
        /// <param name="affectedSource">Required source module.</param>
        /// <param name="affectedSources">Optional list of additional sources.</param>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public void
        CompileAndLinkAgainst<DependentModule>(
            CModule affectedSource,
            params CModule[] additionalSources) where DependentModule : CModule, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            this.DependsOn(dependent);
            this.linkedModules.Add(dependent);
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
        }

        /// <summary>
        /// Specified sources and the application compiles and links against the DependentModule, and the
        /// application uses patches from the dependent.
        /// </summary>
        /// <param name="affectedSource">Required source module.</param>
        /// <param name="affectedSources">Optional list of additional sources.</param>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public void
        CompilePubliclyAndLinkAgainst<DependentModule>(
            CModule affectedSource,
            params CModule[] additionalSources) where DependentModule : CModule, new()
        {
            this.CompileAndLinkAgainst<DependentModule>(affectedSource, additionalSources);
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            this.UsePublicPatches(dependent);
        }

        private void
        LinkAllForwardedDependenciesFromLibraries(
            Bam.Core.Module module)
        {
            var withForwarded = module as IForwardedLibraries;
            if (null == withForwarded)
            {
                return;
            }

            // recursive
            foreach (var forwarded in withForwarded.ForwardedLibraries)
            {
                this.DependsOn(forwarded);
                // some linkers require a specific order of libraries in order to resolve symbols
                // so that if an existing library is later referenced, it needs to be moved later
                if (this.linkedModules.Contains(forwarded))
                {
                    this.linkedModules.Remove(forwarded);
                }
                this.linkedModules.Add(forwarded);
                this.LinkAllForwardedDependenciesFromLibraries(forwarded);
            }
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
            if (this.IsPrebuilt &&
                !((this.headerModules.Count > 0) && Bam.Core.Graph.Instance.BuildModeMetaData.CanCreatePrebuiltProjectForAssociatedFiles))
            {
                return;
            }
            var source = FlattenHierarchicalFileList(this.sourceModules).ToReadOnlyCollection();
            var headers = FlattenHierarchicalFileList(this.headerModules).ToReadOnlyCollection();
            var linked = this.linkedModules.ToReadOnlyCollection();
            var executable = this.GeneratedPaths[Key];
            this.Policy.Link(this, context, executable, source, headers, linked);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            if (this.IsPrebuilt &&
                !((this.headerModules.Count > 0) && Bam.Core.Graph.Instance.BuildModeMetaData.CanCreatePrebuiltProjectForAssociatedFiles))
            {
                return;
            }
            var className = "C." + mode + "Linker";
            this.Policy = Bam.Core.ExecutionPolicyUtilities<ILinkingPolicy>.Create(className);
        }

        public sealed override void
        Evaluate()
        {
            this.ReasonToExecute = null;
            if (this.IsPrebuilt) // never check, even if there are headers
            {
                return;
            }
            var exists = System.IO.File.Exists(this.GeneratedPaths[Key].ToString());
            if (!exists)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
                return;
            }
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
                        case ExecuteReasoning.EReason.InputFileIsNewer:
                            this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], source.ReasonToExecute.OutputFilePath);
                            return;

                        case ExecuteReasoning.EReason.DeferredEvaluation:
                            this.ReasonToExecute = Bam.Core.ExecuteReasoning.DeferredUntilBuild(this.GeneratedPaths[Key]);
                            return;
                    }
                }
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
                        case ExecuteReasoning.EReason.InputFileIsNewer:
                            this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], source.ReasonToExecute.OutputFilePath);
                            return;

                        case ExecuteReasoning.EReason.DeferredEvaluation:
                            this.ReasonToExecute = Bam.Core.ExecuteReasoning.DeferredUntilBuild(this.GeneratedPaths[Key]);
                            return;
                    }
                }
            }
        }
    }
}
