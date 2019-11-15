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
        /// Return a list of Module types that are the libraries to include in the SDK.
        /// </summary>
        protected abstract Bam.Core.TypeArray LibraryModuleTypes { get; }

        protected override void
        Init()
        {
            base.Init();

            this.SetDefaultMacrosAndMappings(EPublishingType.Library);

            foreach (var header in this.HeaderFiles)
            {
                copiedHeaders.AddRange(this.IncludeFiles(header, this.HeaderDir, null));
            }

            var isPrimaryOutput = true;
            foreach (var type in this.LibraryModuleTypes)
            {
                var includeFn = this.GetType().GetMethod("Include").MakeGenericMethod(type);
                var copiedBin = includeFn.Invoke(this, new[] { DynamicLibrary.ExecutableKey, null }) as Publisher.ICollatedObject;
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                {
                    var copiedLib = includeFn.Invoke(this, new[] { DynamicLibrary.ImportLibraryKey, null }) as Publisher.ICollatedObject;
                    copiedLibs.Add(copiedLib);
                    this.RegisterGeneratedFile(
                        DynamicLibrary.ImportLibraryKey,
                        (copiedLib as Publisher.CollatedObject).GeneratedPaths[Publisher.CollatedObject.CopiedFileKey],
                        isPrimaryOutput
                    );
                }
                else
                {
                    var typeModule = Bam.Core.Graph.Instance.GetReferencedModule(this.BuildEnvironment, type);
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux) && typeModule is IDynamicLibrary dynamicLib)
                    {
                        var copiedSOName = this.IncludeModule(dynamicLib.SONameSymbolicLink, SharedObjectSymbolicLink.SOSymLinkKey, null);
                        var copiedLinkName = this.IncludeModule(dynamicLib.LinkerNameSymbolicLink, SharedObjectSymbolicLink.SOSymLinkKey, null);
                        copiedLibs.Add(copiedLinkName);
                        this.RegisterGeneratedFile(
                            SharedObjectSymbolicLink.SOSymLinkKey,
                            (copiedLinkName as Publisher.CollatedObject).GeneratedPaths[Publisher.CollatedObject.CopiedFileKey],
                            isPrimaryOutput
                        );
                    }
                    else
                    {
                        copiedLibs.Add(copiedBin);
                        this.RegisterGeneratedFile(
                            DynamicLibrary.ExecutableKey,
                            (copiedBin as Publisher.CollatedObject).GeneratedPaths[Publisher.CollatedObject.CopiedFileKey],
                            isPrimaryOutput
                        );
                    }
                }
                isPrimaryOutput = false;
            }

            this.PublicPatch((settings, appliedTo) =>
            {
                if (settings is ICommonPreprocessorSettings preprocessor)
                {
                    preprocessor.IncludePaths.AddUnique((this.copiedHeaders.First() as Publisher.CollatedObject).CreateTokenizedString("$(0)", this.HeaderDir));
                }
                else if (settings is ICommonLinkerSettings linker)
                {
                    foreach (var lib in this.copiedLibs)
                    {
                        if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                        {
                            linker.LibraryPaths.AddUnique((lib as Publisher.CollatedObject).CreateTokenizedString("$(0)", this.ImportLibraryDir));
                        }
                        else
                        {
                            linker.LibraryPaths.AddUnique((lib as Publisher.CollatedObject).CreateTokenizedString("$(0)", this.ExecutableDir));
                        }
                        var outputPath = (lib as Bam.Core.Module).GeneratedPaths[Publisher.CollatedFile.CopiedFileKey];
                        if (!outputPath.IsParsed)
                        {
                            outputPath.Parse();
                        }
                        linker.Libraries.Add(outputPath.ToString());
                    }
                }
            });
        }
    }
}
