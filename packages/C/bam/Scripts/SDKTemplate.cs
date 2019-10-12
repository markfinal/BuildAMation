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
    abstract class SDKTemplate :
        Publisher.Collation
    {
        private readonly Bam.Core.Array<Publisher.ICollatedObject> copiedHeaders = new Bam.Core.Array<Publisher.ICollatedObject>();
        private readonly Bam.Core.Array<Publisher.ICollatedObject> copiedLibs = new Bam.Core.Array<Publisher.ICollatedObject>();

        protected abstract Bam.Core.TokenizedStringArray HeaderFiles { get; }
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

            foreach (var type in this.LibraryModuleTypes)
            {
                var includeFn = this.GetType().GetMethod("Include").MakeGenericMethod(type);
                includeFn.Invoke(this, new[] { DynamicLibrary.ExecutableKey, null });
                var copiedLib = includeFn.Invoke(this, new[] { DynamicLibrary.ImportLibraryKey, null }) as Publisher.ICollatedObject;
                copiedLibs.Add(copiedLib);
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
                        linker.LibraryPaths.AddUnique((lib as Publisher.CollatedObject).CreateTokenizedString("$(0)", this.ImportLibraryDir));
                        var libPath = (appliedTo.Tool as LinkerTool).GetLibraryPath(lib.SourceModule as CModule);
                        if (!libPath.IsParsed)
                        {
                            libPath.Parse();
                        }
                        linker.Libraries.Add(libPath.ToString());
                    }
                }
            });
        }
    }
}
