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
namespace AssemblerTest1
{
    sealed class TestProg :
        C.Cxx.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var source = this.CreateAssemblerSourceContainer();
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                if (this.Linker is MingwCommon.LinkerBase)
                {
                    source.AddFiles("$(packagedir)/source/mingw/*32.S");
                }
                else
                {
                    source.AddFiles("$(packagedir)/source/*.asm");
                    if (this.BitDepth == C.EBit.ThirtyTwo)
                    {
                        source["helloworld64.asm"].ForEach(item => (item as C.ObjectFileBase).PerformCompilation = false);
                    }
                    else
                    {
                        source["helloworld32.asm"].ForEach(item => (item as C.ObjectFileBase).PerformCompilation = false);
                    }
                }
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                source.AddFiles("$(packagedir)/source/clang/*.s");
                if (this.BitDepth == C.EBit.ThirtyTwo)
                {
                    source["helloworld64.s"].ForEach(item => (item as C.ObjectFileBase).PerformCompilation = false);
                }
                else
                {
                    source["helloworld32.s"].ForEach(item => (item as C.ObjectFileBase).PerformCompilation = false);
                }
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                source.AddFiles("$(packagedir)/source/gcc/*.S");
                if (this.BitDepth == C.EBit.ThirtyTwo)
                {
                    source["helloworld64.S"].ForEach(item => (item as C.ObjectFileBase).PerformCompilation = false);
                }
                else
                {
                    source["helloworld32.S"].ForEach(item => (item as C.ObjectFileBase).PerformCompilation = false);
                }
            }

            if (this.Linker is VisualCCommon.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
                this.PrivatePatch(settings =>
                    {
                        var linker = settings as C.ICommonLinkerSettings;
                        linker.Libraries.AddUnique("kernel32.lib");

                        var additional = settings as C.IAdditionalSettings;
                        additional.AdditionalSettings.AddUnique("-entry:main");
                    });
            }
        }
    }

    sealed class TestLibrary :
        C.StaticLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var source = this.CreateAssemblerSourceContainer();
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                if (this.Librarian is MingwCommon.Librarian)
                {
                    source.AddFiles("$(packagedir)/source/mingw/*32.S");
                }
                else
                {
                    source.AddFiles("$(packagedir)/source/*.asm");
                    if (this.BitDepth == C.EBit.ThirtyTwo)
                    {
                        source["helloworld64.asm"].ForEach(item => (item as C.ObjectFileBase).PerformCompilation = false);
                    }
                    else
                    {
                        source["helloworld32.asm"].ForEach(item => (item as C.ObjectFileBase).PerformCompilation = false);
                    }
                }
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                source.AddFiles("$(packagedir)/source/clang/*.s");
                if (this.BitDepth == C.EBit.ThirtyTwo)
                {
                    source["helloworld64.s"].ForEach(item => (item as C.ObjectFileBase).PerformCompilation = false);
                }
                else
                {
                    source["helloworld32.s"].ForEach(item => (item as C.ObjectFileBase).PerformCompilation = false);
                }
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                source.AddFiles("$(packagedir)/source/gcc/*.S");
                if (this.BitDepth == C.EBit.ThirtyTwo)
                {
                    source["helloworld64.S"].ForEach(item => (item as C.ObjectFileBase).PerformCompilation = false);
                }
                else
                {
                    source["helloworld32.S"].ForEach(item => (item as C.ObjectFileBase).PerformCompilation = false);
                }
            }
        }
    }
}
