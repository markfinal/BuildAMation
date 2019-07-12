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
namespace C
{
    /// <summary>
    /// Derive from this module to create a static library of object files, in any language.
    /// </summary>
    public class StaticLibrary :
        CModule,
        IForwardedLibraries
    {
        private readonly Bam.Core.Array<Bam.Core.Module> sourceModules = new Bam.Core.Array<Bam.Core.Module>();
        private readonly Bam.Core.Array<Bam.Core.Module> forwardedDeps = new Bam.Core.Array<Bam.Core.Module>();
        public const string LibraryKey = "Static Library File";

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.Librarian = DefaultToolchain.Librarian(this.BitDepth);
            this.RegisterGeneratedFile(
                LibraryKey,
                this.CreateTokenizedString("$(packagebuilddir)/$(moduleoutputdir)/$(libprefix)$(OutputName)$(libext)")
            );
        }

        public override string CustomOutputSubDirectory => "lib";

        /// <summary>
        /// Access the object files required to create this library.
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
        /// Access the headers files associated with this library.
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

        System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> IForwardedLibraries.ForwardedLibraries => this.forwardedDeps.ToReadOnlyCollection();

        public AssembledObjectFileCollection
        CreateAssemblerSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var source = this.InternalCreateContainer<AssembledObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter);
            this.sourceModules.Add(source);
            return source;
        }

        public CObjectFileCollection
        CreateCSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var source = this.InternalCreateContainer<CObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter);
            this.sourceModules.Add(source);
            return source;
        }

        public Cxx.ObjectFileCollection
        CreateCxxSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var source = this.InternalCreateContainer<Cxx.ObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter);
            this.sourceModules.Add(source);
            return source;
        }

        public C.ObjC.ObjectFileCollection
        CreateObjectiveCSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var source = this.InternalCreateContainer<C.ObjC.ObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter);
            this.sourceModules.Add(source);
            return source;
        }

        public C.ObjCxx.ObjectFileCollection
        CreateObjectiveCxxSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var source = this.InternalCreateContainer<C.ObjCxx.ObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter);
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
            params CModule[] additionalSources) where DependentModule : CModule, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
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
            if (dependent is HeaderLibrary)
            {
                this.Requires(dependent);
            }
            // this delays the dependency until a link
            // (and recursively checks the dependent for more forwarded dependencies)
            // because there is no explicit DependsOn call, perform a cyclic dependency check here too
            if (dependent is IForwardedLibraries forwardedDep)
            {
                if (forwardedDep.ForwardedLibraries.Contains(this))
                {
                    throw new Bam.Core.Exception(
                        $"Cyclic dependency found between {this.ToString()} and {dependent.ToString()}"
                    );
                }
            }
            this.forwardedDeps.AddUnique(dependent);
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
            this.CompileAgainst<DependentModule>(affectedSource, additionalSources);
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            this.UsePublicPatches(dependent);
        }

        public LibrarianTool Librarian
        {
            get
            {
                return this.Tool as LibrarianTool;
            }
            set
            {
                this.Tool = value;
            }
        }

        protected sealed override void
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
                        MakeFileBuilder.Support.Add(this);
                    }
                    break;
#endif

#if D_PACKAGE_NATIVEBUILDER
                case "Native":
                    NativeBuilder.Support.RunCommandLineTool(this, context);
                    break;
#endif

#if D_PACKAGE_VSSOLUTIONBUILDER
                case "VSSolution":
                    VSSolutionSupport.Archive(this);
                    break;
#endif

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    XcodeSupport.Archive(this);
                    break;
#endif

                default:
                    throw new System.NotImplementedException();
            }
        }

        protected sealed override void
        EvaluateInternal()
        {
            this.ReasonToExecute = null;
            if (this.IsPrebuilt) // never check
            {
                return;
            }
            var libraryPath = this.GeneratedPaths[LibraryKey].ToString();
            var exists = System.IO.File.Exists(libraryPath);
            if (!exists)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[LibraryKey]);
                return;
            }
            var libraryWriteTime = System.IO.File.GetLastWriteTime(libraryPath);
            foreach (var source in this.sourceModules)
            {
                source.EvaluationTask?.Wait();
                if (null != source.ReasonToExecute)
                {
                    switch (source.ReasonToExecute.Reason)
                    {
                        case Bam.Core.ExecuteReasoning.EReason.FileDoesNotExist:
                        case Bam.Core.ExecuteReasoning.EReason.InputFileIsNewer:
                            this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                                this.GeneratedPaths[LibraryKey],
                                source.ReasonToExecute.OutputFilePath
                            );
                            return;

                        default:
                            throw new Bam.Core.Exception($"Unknown reason, {source.ReasonToExecute.Reason.ToString()}");
                    }
                }
                else
                {
                    // if an object file is built, but for some reason (e.g. previous build failure), not been archived
                    if (source is Bam.Core.IModuleGroup)
                    {
                        foreach (var objectFile in source.Children)
                        {
                            var objectFilePath = objectFile.GeneratedPaths[ObjectFile.ObjectFileKey].ToString();
                            var objectFileWriteTime = System.IO.File.GetLastWriteTime(objectFilePath);
                            if (objectFileWriteTime > libraryWriteTime)
                            {
                                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                                    this.GeneratedPaths[LibraryKey],
                                    objectFile.GeneratedPaths[ObjectFile.ObjectFileKey]
                                );
                                return;
                            }
                        }
                    }
                    else
                    {
                        var objectFilePath = source.GeneratedPaths[ObjectFile.ObjectFileKey].ToString();
                        var objectFileWriteTime = System.IO.File.GetLastWriteTime(objectFilePath);
                        if (objectFileWriteTime > libraryWriteTime)
                        {
                            this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                                this.GeneratedPaths[LibraryKey],
                                source.GeneratedPaths[ObjectFile.ObjectFileKey]
                            );
                            return;
                        }
                    }
                }
            }
        }
    }
}
