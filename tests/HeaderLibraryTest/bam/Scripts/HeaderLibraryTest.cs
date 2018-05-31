#region License
// Copyright (c) 2010-2018, Mark Final
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
namespace HeaderLibraryTest
{
    class GeneratedHeader :
        C.ProceduralHeaderFile
    {
        protected override TokenizedString OutputPath
        {
            get
            {
                return this.CreateTokenizedString("$(packagebuilddir)/$(moduleoutputdir)/genheader.h");
            }
        }

        protected override string Contents
        {
            get
            {
                return "#define GENERATED_HEADER";
            }
        }
    }

    public sealed class HeaderLibrary :
        C.HeaderLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var headers = this.CreateHeaderContainer("$(packagedir)/include/unusedmacros.h");

            var genHeader = Bam.Core.Graph.Instance.FindReferencedModule<GeneratedHeader>();
            headers.AddFile(genHeader);
            this.UsePublicPatches(genHeader);

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/include"));
                    }
                });
        }
    }

    public sealed class Application :
        C.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer("$(packagedir)/source/main.c");

            this.CompileAgainst<HeaderLibrary>(source);
        }
    }

    // not sealed, to avoid being considered as a top-level module
    class StringifyMacro :
        C.HeaderLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(packagedir)/include/lib1/stringifymacro.h");

            this.PublicPatch((settings, appliedTo) =>
            {
                var compiler = settings as C.ICommonCompilerSettings;
                if (null != compiler)
                {
                    compiler.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/include/lib1"));
                }
            });
        }
    }

    // not sealed, to avoid being considered as a top-level module
    class StringifyMacroValue :
        C.HeaderLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(packagedir)/include/lib2/stringifymacrovalue.h");

            this.CompileAgainst<StringifyMacro>();

            this.PublicPatch((settings, appliedTo) =>
            {
                var compiler = settings as C.ICommonCompilerSettings;
                if (null != compiler)
                {
                    compiler.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/include/lib2"));
                }
            });
        }
    }

    sealed class LibUsingStringifyMacros :
        C.StaticLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer("$(packagedir)/source/lib.c");

            this.CompileAgainst<StringifyMacroValue>(source);
        }
    }

    // not sealed, to avoid being considered as a top-level module
    class DeepHeaderLib :
        C.HeaderLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(packagedir)/include/level1/**.h");

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/include/level1"));
                    }
                });
        }
    }

    sealed class LibUsingDeepHeaderLibrary :
        C.StaticLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer("$(packagedir)/source/deeplib.c");

            this.CompileAgainst<DeepHeaderLib>(source);
        }
    }
}
