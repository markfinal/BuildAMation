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
using Bam.Core;
namespace C
{
    /// <summary>
    /// Module representing a template to follow for creating SDKs.
    /// </summary>
    abstract class SDKTemplate :
        Publisher.Collation,
        IForwardedLibraries
    {
        private readonly Bam.Core.Array<Publisher.ICollatedObject> copiedHeaders = new Bam.Core.Array<Publisher.ICollatedObject>();
        private readonly Bam.Core.Array<Publisher.ICollatedObject> copiedLibs = new Bam.Core.Array<Publisher.ICollatedObject>();
        private bool useExistingSDK;

        /// <summary>
        /// List of the actual Modules used to populate the SDK
        /// </summary>
        protected readonly Bam.Core.Array<Bam.Core.Module> realLibraryModules = new Bam.Core.Array<Bam.Core.Module>();

        /// <summary>
        /// Return a list of partial-paths (relative to the package dir) to extra header files (not associated with SDK libraries) to include in the SDK.
        /// </summary>
        protected virtual Bam.Core.StringArray ExtraHeaderFiles { get; }

        /// <summary>
        /// Whether the header file directory structure is honoured upon copy to the SDK.
        /// </summary>
        protected virtual bool HonourHeaderFileLayout { get; } = true;

        /// <summary>
        /// Return a list of Module types that are the generated headers to include in the SDK.
        /// </summary>
        protected virtual Bam.Core.TypeArray GeneratedHeaderTypes { get; } = null;

        /// <summary>
        /// Return a list of Module types that are the libraries to include in the SDK.
        /// </summary>
        public abstract Bam.Core.TypeArray LibraryModuleTypes { get; }

        protected override void
        Init()
        {
            base.Init();

            this.SetDefaultMacrosAndMappings(EPublishingType.Library);

            var SDKname = this.GetType().Namespace;
            this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim(SDKname);

            Bam.Core.TokenizedString includeDir = null;
            var libraryDirs = new Bam.Core.TokenizedStringArray();
            var libs = new Bam.Core.TokenizedStringArray();

            // TODO: would be nice to re-use publishroot, but it doesn't exist
            // this is not the definition of where to write files - it's in Publisher's Collation.cs
            var bitDepth = Bam.Core.CommandLineProcessor.Evaluate(new C.Options.DefaultBitDepth());
            var publishRoot = this.CreateTokenizedString($"$(prebuiltsdksroot)/$(OutputName)/$(config){bitDepth}");
            if (!publishRoot.IsParsed)
            {
                publishRoot.Parse();
            }

            var prebuiltExists = System.IO.Directory.Exists(publishRoot.ToString());
            var sdkBuildOptions = Bam.Core.CommandLineProcessor.Evaluate(new Options.SDKBuild());
            this.useExistingSDK = prebuiltExists &&
                !sdkBuildOptions.Any(item => item.Contains("*", System.StringComparer.Ordinal) || item.Contains(SDKname, System.StringComparer.OrdinalIgnoreCase));
            if (this.useExistingSDK)
            {
                this.UsePrebuiltSDK(publishRoot, out includeDir, libs, libraryDirs);
            }
            else
            {
                this.GenerateSDK(out includeDir, libs, libraryDirs);
            }

            this.PublicPatch((settings, appliedTo) =>
            {
                if (settings is ICommonPreprocessorSettings preprocessor)
                {
                    if (null != includeDir)
                    {
                        preprocessor.IncludePaths.AddUnique(includeDir);
                    }
                }
            });
        }

        public override void
        PostInit()
        {
            base.PostInit();

            // generate Linux linkername and SOname symbolic links when making the SDK
            foreach (var libraryModule in this.realLibraryModules)
            {
                if (libraryModule.Settings is C.ICommonLinkerSettingsLinux linuxLinker)
                {
                    if (linuxLinker.SharedObjectName != null)
                    {
                        var graph = Bam.Core.Graph.Instance;
                        SharedObjectSymbolicLink linkerName;
                        SharedObjectSymbolicLink soName;
                        graph.CommonModuleType.Push(libraryModule.GetType());
                        try
                        {
                            linkerName = Bam.Core.Module.Create<LinkerNameSymbolicLink>(preInitCallback: module =>
                            {
                                module.Macros.Add("SymlinkFilename", libraryModule.CreateTokenizedString("$(dynamicprefix)$(OutputName)$(linkernameext)"));
                                module.SharedObject = libraryModule as C.ConsoleApplication;
                            });
                            linkerName.Build = !this.useExistingSDK;
                            soName = Bam.Core.Module.Create<SONameSymbolicLink>(preInitCallback: module =>
                            {
                                module.Macros.Add("SymlinkFilename", libraryModule.CreateTokenizedString("$(dynamicprefix)$(OutputName)$(sonameext)"));
                                module.SharedObject = libraryModule as C.ConsoleApplication;
                            });
                            soName.Build = !this.useExistingSDK;
                        }
                        finally
                        {
                            graph.CommonModuleType.Pop();
                        }
                        graph.CommonModuleType.Push(this.GetType());
                        try
                        {
                            var copiedLinkerName = this.IncludeModule(linkerName, SharedObjectSymbolicLink.SOSymLinkKey);
                            (copiedLinkerName as Bam.Core.Module).Build = !this.useExistingSDK;
                            var copiedSOName = this.IncludeModule(soName, SharedObjectSymbolicLink.SOSymLinkKey);
                            (copiedSOName as Bam.Core.Module).Build = !this.useExistingSDK;
                            this.copiedLibs.Add(copiedLinkerName);
                            this.copiedLibs.Add(copiedSOName);
                        }
                        finally
                        {
                            graph.CommonModuleType.Pop();
                        }
                    }
                }
            }
        }

        System.Collections.Generic.IEnumerable<Module> IForwardedLibraries.ForwardedLibraries => this.realLibraryModules.Where(item => !(item is HeaderLibrary));

        private void
        GatherAllLibrariesForSDK()
        {
            // make modules for each of those types explicitly requested
            foreach (var libType in this.LibraryModuleTypes)
            {
                var findFn = Bam.Core.Graph.Instance.GetType().GetMethod("FindReferencedModule", System.Type.EmptyTypes).MakeGenericMethod(libType);
                var libraryModule = findFn.Invoke(Bam.Core.Graph.Instance, null) as Bam.Core.Module;
                this.realLibraryModules.Add(libraryModule);
            }

            var allLibrariesInSDK = new Bam.Core.Array<Bam.Core.Module>();
            void recurse(
                Bam.Core.Module module)
            {
                if (!(module is CModule))
                {
                    return;
                }
                if (module.GetType().Namespace != this.GetType().Namespace)
                {
                    return;
                }
                allLibrariesInSDK.AddUnique(module);
                foreach (var dep in module.Dependents)
                {
                    recurse(dep);
                }
                foreach (var req in module.Requirements)
                {
                    recurse(req);
                }
            }
            foreach (var lib in this.realLibraryModules)
            {
                recurse(lib);
            }
            this.realLibraryModules.AddRangeUnique(allLibrariesInSDK);
        }

        private void
        UsePrebuiltSDK(
            Bam.Core.TokenizedString publishRoot,
            out Bam.Core.TokenizedString includeDir,
            Bam.Core.TokenizedStringArray libs,
            Bam.Core.TokenizedStringArray libraryDirs
        )
        {
            includeDir = this.CreateTokenizedString("$(0)/include", publishRoot);
            var sdkBinDir = this.CreateTokenizedString("$(0)/bin", publishRoot);
            var sdkLibDir = this.CreateTokenizedString("$(0)/lib", publishRoot);

            this.GatherAllLibrariesForSDK();

            foreach (var libraryModule in this.realLibraryModules)
            {
                // when using the SDK, we don't need its component Modules to be built
                // but do need it in the graph for collation to find it as a dependent
                this.UsePublicPatches(libraryModule);
                this.DependsOn(libraryModule);
                libraryModule.Build = false;

                if (libraryModule is HeaderLibrary)
                {
                    continue;
                }

                // update the library so that its binaries refer to those in the SDK
                if (libraryModule is IDynamicLibrary dynLibraryModule)
                {
                    dynLibraryModule.ChangeRootPaths(sdkBinDir, sdkLibDir);
                }
                else if (libraryModule is StaticLibrary staticLib)
                {
                    staticLib.ChangeLibraryRootPath(sdkLibDir);
                }

                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                {
                    if (libraryModule is IDynamicLibrary)
                    {
                        var importLibPath = libraryModule.GeneratedPaths[DynamicLibrary.ImportLibraryKey];
                        libs.AddUnique(this.CreateTokenizedString("@filename($(0))", importLibPath));
                        libraryDirs.AddUnique(this.CreateTokenizedString("@dir($(0))", importLibPath));
                    }
                    else
                    {
                        var libPath = libraryModule.GeneratedPaths[libraryModule.PrimaryOutputPathKey];
                        libs.AddUnique(this.CreateTokenizedString("@filename($(0))", libPath));
                        libraryDirs.AddUnique(this.CreateTokenizedString("@dir($(0))", libPath));
                    }
                }
                else
                {
                    if (libraryModule is IDynamicLibrary dynamicLib)
                    {
                        var dylib = libraryModule.GeneratedPaths[DynamicLibrary.ExecutableKey];
                        libraryDirs.AddUnique(this.CreateTokenizedString("@dir($(0))", dylib));
                        if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                        {
                            libs.AddUnique(this.CreateTokenizedString("-l$(0)", libraryModule.Macros[Bam.Core.ModuleMacroNames.OutputName]));
                        }
                        else
                        {
                            libs.AddUnique(this.CreateTokenizedString("-l@trimstart(@basename($(0)),lib)", dylib));
                        }
                    }
                    else
                    {
                        var libPath = libraryModule.GeneratedPaths[libraryModule.PrimaryOutputPathKey];
                        libs.AddUnique(this.CreateTokenizedString("-l@trimstart(@basename($(0)),lib)", libPath));
                        libraryDirs.AddUnique(this.CreateTokenizedString("@dir($(0))", libPath));
                    }
                }
            }
        }

        // requires this.realLibraryModules to have been populated
        private void
        GenerateSDKHeaders(
            out Bam.Core.TokenizedString includeDir
        )
        {
            void publishHeaders(
                Bam.Core.StringArray paths)
            {
                var packageDir = this.Macros[Bam.Core.ModuleMacroNames.PackageDirectory].ToString();
                foreach (var headerPath in paths)
                {
                    var src = System.IO.Path.Combine(packageDir, headerPath);
                    var option = src.Contains("**") ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;
                    var files = System.IO.Directory.GetFiles(
                        System.IO.Path.GetDirectoryName(src),
                        System.IO.Path.GetFileName(src),
                        option
                    );
                    if (0 == files.Length)
                    {
                        throw new Bam.Core.Exception(
                            $"No matches found for the header path '{src}'"
                        );
                    }
                    foreach (var file in files)
                    {
                        var relativePath = System.IO.Path.GetRelativePath(packageDir, file);
                        var dst = this.HonourHeaderFileLayout ?
                            this.CreateTokenizedString($"$(0)/{System.IO.Path.GetDirectoryName(relativePath)}", this.HeaderDir) :
                            this.CreateTokenizedString($"$(0)", this.HeaderDir);
                        copiedHeaders.AddRange(this.IncludeFiles(Bam.Core.TokenizedString.CreateVerbatim(file), dst, null));
                    }
                }
            }
            if (!this.Macros[Bam.Core.ModuleMacroNames.PackageDirectory].IsParsed)
            {
                this.Macros[Bam.Core.ModuleMacroNames.PackageDirectory].Parse();
            }
            if (null != this.ExtraHeaderFiles)
            {
                publishHeaders(this.ExtraHeaderFiles);
            }
            if ((null != this.GeneratedHeaderTypes) && this.GeneratedHeaderTypes.Any())
            {
                foreach (var genHeaderType in this.GeneratedHeaderTypes)
                {
                    var includeFn = this.GetType().GetMethod("Include").MakeGenericMethod(genHeaderType);
                    var copiedHeader = includeFn.Invoke(this, new[] { HeaderFile.HeaderFileKey, null }) as Publisher.ICollatedObject;
                    copiedHeaders.Add(copiedHeader);
                }
            }
            foreach (var libraryModule in this.realLibraryModules)
            {
                if (libraryModule is IPublicHeaders publicHeaders)
                {
                    publishHeaders(publicHeaders.PublicHeaders);
                }
            }
            if (this.copiedHeaders.Any())
            {
                // can't juse be this.HeaderDir, as it needs to resolve publishdir
                includeDir = (this.copiedHeaders.First() as Publisher.CollatedObject).CreateTokenizedString("$(0)", this.HeaderDir);
            }
            else
            {
                includeDir = null;
            }
        }

        private void
        GenerateSDK(
            out Bam.Core.TokenizedString includeDir,
            Bam.Core.TokenizedStringArray libs,
            Bam.Core.TokenizedStringArray libraryDirs
        )
        {
            this.GatherAllLibrariesForSDK();

            var isPrimaryOutput = true;
            foreach (var libraryModule in this.realLibraryModules)
            {
                this.UsePublicPatches(libraryModule);

                if (libraryModule is HeaderLibrary)
                {
                    continue;
                }

                var copiedBin = this.IncludeModule(libraryModule, libraryModule.PrimaryOutputPathKey);
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                {
                    if (libraryModule is IDynamicLibrary)
                    {
                        var copiedLib = this.IncludeModule(libraryModule, DynamicLibrary.ImportLibraryKey);
                        copiedLibs.Add(copiedLib);
                        libraryDirs.AddUnique((copiedLib as Publisher.CollatedObject).CreateTokenizedString("$(0)", this.ImportLibraryDir));
                        this.RegisterGeneratedFile(
                            DynamicLibrary.ImportLibraryKey,
                            (copiedLib as Publisher.CollatedObject).GeneratedPaths[Publisher.CollatedObject.CopiedFileKey],
                            isPrimaryOutput
                        );
                        libs.AddUnique(this.CreateTokenizedString("@filename($(0))", (copiedLib as Publisher.CollatedObject).GeneratedPaths[Publisher.CollatedObject.CopiedFileKey]));
                    }
                    else
                    {
                        copiedLibs.Add(copiedBin);
                        libraryDirs.AddUnique((copiedBin as Publisher.CollatedObject).CreateTokenizedString("$(0)", this.ImportLibraryDir));
                        this.RegisterGeneratedFile(
                            libraryModule.PrimaryOutputPathKey,
                            (copiedBin as Publisher.CollatedObject).GeneratedPaths[Publisher.CollatedObject.CopiedFileKey],
                            isPrimaryOutput
                        );
                        libs.AddUnique(this.CreateTokenizedString("@filename($(0))", (copiedBin as Publisher.CollatedObject).GeneratedPaths[Publisher.CollatedObject.CopiedFileKey]));
                    }
                }
                else
                {
                    if (libraryModule is IDynamicLibrary dynamicLib)
                    {
                        copiedLibs.Add(copiedBin);
                        libraryDirs.AddUnique((copiedBin as Publisher.CollatedObject).CreateTokenizedString("$(0)", this.ExecutableDir));
                        this.RegisterGeneratedFile(
                            libraryModule.PrimaryOutputPathKey,
                            (copiedBin as Publisher.CollatedObject).GeneratedPaths[Publisher.CollatedObject.CopiedFileKey],
                            isPrimaryOutput
                        );
                        if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                        {
                            libs.AddUnique(this.CreateTokenizedString("-l$(0)", libraryModule.Macros[Bam.Core.ModuleMacroNames.OutputName]));
                        }
                        else
                        {
                            libs.AddUnique(this.CreateTokenizedString("-l@trimstart(@basename($(0)),lib)", (copiedBin as Publisher.CollatedObject).GeneratedPaths[Publisher.CollatedObject.CopiedFileKey]));
                        }
                    }
                    else
                    {
                        libraryDirs.AddUnique((copiedBin as Publisher.CollatedObject).CreateTokenizedString("$(0)", this.StaticLibraryDir));
                        this.RegisterGeneratedFile(
                            libraryModule.PrimaryOutputPathKey,
                            (copiedBin as Publisher.CollatedObject).GeneratedPaths[Publisher.CollatedObject.CopiedFileKey],
                            isPrimaryOutput
                        );
                        libs.AddUnique(this.CreateTokenizedString("-l@trimstart(@basename($(0)),lib)", (copiedBin as Publisher.CollatedObject).GeneratedPaths[Publisher.CollatedObject.CopiedFileKey]));
                    }
                }
                isPrimaryOutput = false;
            }
            this.GenerateSDKHeaders(out includeDir);
        }
    }
}
