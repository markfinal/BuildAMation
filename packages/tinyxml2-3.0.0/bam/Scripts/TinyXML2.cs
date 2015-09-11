#region License
// Copyright (c) 2010-2015, Mark Final
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
namespace TinyXML2
{
    [Bam.Core.ModuleGroup("Thirdparty")]
    sealed class TinyXML2StaticV2 :
        C.V2.StaticLibrary
    {
        private Bam.Core.V2.Module.PublicPatchDelegate includePaths = (settings, appliedTo) =>
        {
            var compiler = settings as C.V2.ICommonCompilerOptions;
            if (null != compiler)
            {
                compiler.IncludePaths.AddUnique(Bam.Core.V2.TokenizedString.Create("$(pkgroot)", appliedTo));
            }
        };

        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCxxSourceContainer("$(pkgroot)/tinyxml2.cpp");
            source.PrivatePatch(settings => this.includePaths(settings, this));

            this.PublicPatch((settings, appliedTo) => this.includePaths(settings, this));
        }
    }

    [Bam.Core.ModuleGroup("Thirdparty")]
    class TinyXML2Static :
        C.StaticLibrary
    {
        public
        TinyXML2Static()
        {
            var sourceDir = this.PackageLocation.SubDirectory("tinyxml2-2.2.0");
            this.headers.Include(sourceDir, "*.h");
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(TinyXML2Static_ExportIncludePath);
        }

        [C.ExportCompilerOptionsDelegate]
        void
        TinyXML2Static_ExportIncludePath(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            if (module.Options is C.ICCompilerOptions)
            {
                (module.Options as C.ICCompilerOptions).IncludePaths.Include(this.PackageLocation, "tinyxml2-2.2.0");
            }
        }

        class SourceFiles :
            C.Cxx.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("tinyxml2-2.2.0");
                this.Include(sourceDir, "tinyxml2.cpp");
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles source = new SourceFiles();

        [C.HeaderFiles]
        Bam.Core.FileCollection headers = new Bam.Core.FileCollection();
    }
}
