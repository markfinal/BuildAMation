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

            if (System.IO.Directory.Exists(publishRoot.ToString()))
            {
                // found the SDK directory on disk

                includeDir = this.CreateTokenizedString("$(0)/include", publishRoot);
                var sdkLibDir = this.CreateTokenizedString("$(0)/lib", publishRoot);
                var sdkBinDir = this.CreateTokenizedString("$(0)/bin", publishRoot);
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                {
                    libraryDirs.AddUnique(sdkLibDir);
                }
                else
                {
                    libraryDirs.AddUnique(sdkBinDir);
                }

                foreach (var libType in this.LibraryModuleTypes)
                {
                    // when using the SDK, we don't need its component Modules to be built
                    // but do need it in the graph for collation to find it as a dependent
                    var findFn = Bam.Core.Graph.Instance.GetType().GetMethod("FindReferencedModule", System.Type.EmptyTypes).MakeGenericMethod(libType);
                    var libraryModule = findFn.Invoke(Bam.Core.Graph.Instance, null) as Bam.Core.Module;
                    this.UsePublicPatches(libraryModule);
                    this.DependsOn(libraryModule);
                    libraryModule.Build = false;

                    // update the library so that its binaries refer to those in the SDK
                    if (libraryModule is IDynamicLibrary dynLibraryModule)
                    {
                        dynLibraryModule.ChangeExecutableRootPath(sdkBinDir);
                    }

                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        var importLibPath = libraryModule.GeneratedPaths[DynamicLibrary.ImportLibraryKey];
                        libs.AddUnique(this.CreateTokenizedString("@filename($(0))", importLibPath));
                    }
                    else
                    {
                        if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux) && libraryModule is IDynamicLibrary dynamicLib)
                        {
                            var linkNameSO = dynamicLib.LinkerNameSymbolicLink;
                            var linkNameSOPath = linkNameSO.GeneratedPaths[SharedObjectSymbolicLink.SOSymLinkKey];
                            libs.AddUnique(this.CreateTokenizedString("-l@trimstart(@basename($(0)),lib)", linkNameSOPath));
                        }
                        else
                        {
                            var dylib = libraryModule.GeneratedPaths[DynamicLibrary.ExecutableKey];
                            libs.AddUnique(this.CreateTokenizedString("-l@trimstart(@basename($(0)),lib)", dylib));
                        }
                    }
                }
            }
            else
            {
                // need to construct the SDK contents

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

                var isPrimaryOutput = true;
                foreach (var libType in this.LibraryModuleTypes)
                {
                    var findFn = Bam.Core.Graph.Instance.GetType().GetMethod("FindReferencedModule", System.Type.EmptyTypes).MakeGenericMethod(libType);
                    var libraryModule = findFn.Invoke(Bam.Core.Graph.Instance, null) as Bam.Core.Module;
                    this.UsePublicPatches(libraryModule);

                    var includeFn = this.GetType().GetMethod("Include").MakeGenericMethod(libType);
                    var copiedBin = includeFn.Invoke(this, new[] { DynamicLibrary.ExecutableKey, null }) as Publisher.ICollatedObject;
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        var copiedLib = includeFn.Invoke(this, new[] { DynamicLibrary.ImportLibraryKey, null }) as Publisher.ICollatedObject;
                        copiedLibs.Add(copiedLib);
                        libraryDirs.AddUnique((copiedLib as Publisher.CollatedObject).CreateTokenizedString("$(0)", this.ImportLibraryDir));
                        this.RegisterGeneratedFile(
                            DynamicLibrary.ImportLibraryKey,
                            (copiedLib as Publisher.CollatedObject).GeneratedPaths[Publisher.CollatedObject.CopiedFileKey],
                            isPrimaryOutput
                        );
                        libs.AddUnique((copiedLib as Publisher.CollatedObject).GeneratedPaths[Publisher.CollatedObject.CopiedFileKey]);
                    }
                    else
                    {
                        if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux) && libraryModule is IDynamicLibrary dynamicLib)
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
                            libs.AddUnique((copiedLinkName as Publisher.CollatedObject).GeneratedPaths[Publisher.CollatedObject.CopiedFileKey]);
                        }
                        else
                        {
                            copiedLibs.Add(copiedBin);
                            libraryDirs.AddUnique((copiedBin as Publisher.CollatedObject).CreateTokenizedString("$(0)", this.ExecutableDir));
                            this.RegisterGeneratedFile(
                                DynamicLibrary.ExecutableKey,
                                (copiedBin as Publisher.CollatedObject).GeneratedPaths[Publisher.CollatedObject.CopiedFileKey],
                                isPrimaryOutput
                            );
                            libs.AddUnique((copiedBin as Publisher.CollatedObject).GeneratedPaths[Publisher.CollatedObject.CopiedFileKey]);
                        }
                    }
                    isPrimaryOutput = false;
                }
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
    }
}
