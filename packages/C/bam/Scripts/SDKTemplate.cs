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
        Publisher.Collation
    {
        private readonly Bam.Core.Array<Publisher.ICollatedObject> copiedHeaders = new Bam.Core.Array<Publisher.ICollatedObject>();
        private readonly Bam.Core.Array<Publisher.ICollatedObject> copiedLibs = new Bam.Core.Array<Publisher.ICollatedObject>();
        private readonly Bam.Core.Array<Bam.Core.Module> realLibraryModules = new Bam.Core.Array<Bam.Core.Module>();

        /// <summary>
        /// Return a list of paths to header files to include in the SDK.
        /// </summary>
        protected abstract Bam.Core.TokenizedStringArray HeaderFiles { get; }

        /// <summary>
        /// Return a list of Module types that are the generated headers to include in the SDK.
        /// </summary>
        protected virtual Bam.Core.TypeArray GeneratedHeaderTypes { get; } = null;

        /// <summary>
        /// Return a list of Module types that are the libraries to include in the SDK.
        /// </summary>
        protected abstract Bam.Core.TypeArray LibraryModuleTypes { get; }

        protected override void
        Init()
        {
            base.Init();

            this.SetDefaultMacrosAndMappings(EPublishingType.Library);

            this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim(this.GetType().Namespace);

            Bam.Core.TokenizedString includeDir = null;
            var libraryDirs = new Bam.Core.TokenizedStringArray();
            var libs = new Bam.Core.TokenizedStringArray();

            // TODO: would be nice to re-use publishroot, but it doesn't exist
            var publishRoot = this.CreateTokenizedString("$(prebuiltsdksroot)/$(OutputName)");
            if (!publishRoot.IsParsed)
            {
                publishRoot.Parse();
            }

            var useExistingSDK = System.IO.Directory.Exists(publishRoot.ToString());
            if (useExistingSDK)
            {
                UsePrebuiltSDK(publishRoot, out includeDir, libs, libraryDirs);
            }
            else
            {
                GenerateSDK(publishRoot, out includeDir, libs, libraryDirs);
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
                else if (settings is ICommonLinkerSettings linker)
                {
                    foreach (var libDir in libraryDirs)
                    {
                        linker.LibraryPaths.AddUnique(libDir);
                    }
                    foreach (var lib in libs)
                    {
                        if (!lib.IsParsed)
                        {
                            lib.Parse();
                        }
                        linker.Libraries.Add(lib.ToString());
                    }
                }
            });
        }

        public override void
        PostInit()
        {
            base.PostInit();

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
                            soName = Bam.Core.Module.Create<SONameSymbolicLink>(preInitCallback: module =>
                            {
                                module.Macros.Add("SymlinkFilename", libraryModule.CreateTokenizedString("$(dynamicprefix)$(OutputName)$(sonameext)"));
                                module.SharedObject = libraryModule as C.ConsoleApplication;
                            });
                        }
                        finally
                        {
                            graph.CommonModuleType.Pop();
                        }
                        graph.CommonModuleType.Push(this.GetType());
                        try
                        {
                            this.copiedLibs.Add(this.IncludeModule(linkerName, SharedObjectSymbolicLink.SOSymLinkKey));
                            this.copiedLibs.Add(this.IncludeModule(soName, SharedObjectSymbolicLink.SOSymLinkKey));
                        }
                        finally
                        {
                            graph.CommonModuleType.Pop();
                        }
                    }
                }
            }
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

            foreach (var libType in this.LibraryModuleTypes)
            {
                // when using the SDK, we don't need its component Modules to be built
                // but do need it in the graph for collation to find it as a dependent
                var findFn = Bam.Core.Graph.Instance.GetType().GetMethod("FindReferencedModule", System.Type.EmptyTypes).MakeGenericMethod(libType);
                var libraryModule = findFn.Invoke(Bam.Core.Graph.Instance, null) as Bam.Core.Module;
                this.realLibraryModules.Add(libraryModule);
                var originalLibraryModule = libraryModule;

                this.UsePublicPatches(libraryModule);
                this.DependsOn(libraryModule);
                libraryModule.Build = false;

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
#if false
                            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                            {
                                var linkNameSO = dynamicLib.LinkerNameSymbolicLink;
                                var linkNameSOPath = linkNameSO.GeneratedPaths[SharedObjectSymbolicLink.SOSymLinkKey];
                                libs.AddUnique(this.CreateTokenizedString("-l@trimstart(@basename($(0)),lib)", linkNameSOPath));
                                libraryDirs.AddUnique(this.CreateTokenizedString("@dir($(0))", linkNameSOPath));
                            }
                            else
#endif
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

        private void
        GenerateSDK(
            Bam.Core.TokenizedString publishRoot,
            out Bam.Core.TokenizedString includeDir,
            Bam.Core.TokenizedStringArray libs,
            Bam.Core.TokenizedStringArray libraryDirs
        )
        {
            foreach (var header in this.HeaderFiles)
            {
                copiedHeaders.AddRange(this.IncludeFiles(header, this.HeaderDir, null));
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
            if (this.copiedHeaders.Any())
            {
                includeDir = (this.copiedHeaders.First() as Publisher.CollatedObject).CreateTokenizedString("$(0)", this.HeaderDir);
            }
            else
            {
                includeDir = null;
            }

            var isPrimaryOutput = true;
            foreach (var libType in this.LibraryModuleTypes)
            {
                var findFn = Bam.Core.Graph.Instance.GetType().GetMethod("FindReferencedModule", System.Type.EmptyTypes).MakeGenericMethod(libType);
                var libraryModule = findFn.Invoke(Bam.Core.Graph.Instance, null) as Bam.Core.Module;
                this.realLibraryModules.Add(libraryModule);
                this.UsePublicPatches(libraryModule);

                var includeFn = this.GetType().GetMethod("Include").MakeGenericMethod(libType);
                var copiedBin = includeFn.Invoke(this, new[] { libraryModule.PrimaryOutputPathKey, null }) as Publisher.ICollatedObject;
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                {
                    if (libraryModule is IDynamicLibrary)
                    {
                        var copiedLib = includeFn.Invoke(this, new[] { DynamicLibrary.ImportLibraryKey, null }) as Publisher.ICollatedObject;
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
#if false
                            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                            {
                                var copiedSOName = this.IncludeModule(dynamicLib.SONameSymbolicLink, SharedObjectSymbolicLink.SOSymLinkKey, null);
                                var copiedLinkName = this.IncludeModule(dynamicLib.LinkerNameSymbolicLink, SharedObjectSymbolicLink.SOSymLinkKey, null);
                                copiedLibs.Add(copiedLinkName);
                                libraryDirs.AddUnique((copiedLinkName as Publisher.CollatedObject).CreateTokenizedString("$(0)", this.ExecutableDir));
                                this.RegisterGeneratedFile(
                                    SharedObjectSymbolicLink.SOSymLinkKey,
                                    (copiedLinkName as Publisher.CollatedObject).GeneratedPaths[Publisher.CollatedObject.CopiedFileKey],
                                    isPrimaryOutput
                                );
                                libs.AddUnique(this.CreateTokenizedString("-l@trimstart(@basename($(0)),lib)", (copiedLinkName as Publisher.CollatedObject).GeneratedPaths[Publisher.CollatedObject.CopiedFileKey]));
                            }
                            else
#endif
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
        }
    }
}
