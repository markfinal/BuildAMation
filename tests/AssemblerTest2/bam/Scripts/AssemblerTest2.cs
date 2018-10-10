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
namespace AssemblerTest2
{
    sealed class TestProg :
        C.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateCSourceContainer("$(packagedir)/source/*.c");
            this.CreateHeaderContainer("$(packagedir)/source/*.h");

            var asmSource = this.CreateAssemblerSourceContainer();
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                if (this.Linker is MingwCommon.LinkerBase)
                {
                    asmSource.AddFiles("$(packagedir)/source/mingw/*32.S");
                }
                else
                {
                    asmSource.AddFiles("$(packagedir)/source/*.asm");
                    if (this.BitDepth == C.EBit.ThirtyTwo)
                    {
                        asmSource["helloworld64.asm"].ForEach(item => (item as C.ObjectFileBase).PerformCompilation = false);
                    }
                    else
                    {
                        asmSource["helloworld32.asm"].ForEach(item => (item as C.ObjectFileBase).PerformCompilation = false);
                    }
                }
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                asmSource.AddFiles("$(packagedir)/source/clang/*.s");
                if (this.BitDepth == C.EBit.ThirtyTwo)
                {
                    asmSource["helloworld64.s"].ForEach(item => (item as C.ObjectFileBase).PerformCompilation = false);
                }
                else
                {
                    asmSource["helloworld32.s"].ForEach(item => (item as C.ObjectFileBase).PerformCompilation = false);
                }
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                var gccMeta = Bam.Core.Graph.Instance.PackageMetaData<Gcc.MetaData>("Gcc");
                if (gccMeta.CompilerMajorVersion < 8)
                {
                    asmSource.AddFiles("$(packagedir)/source/gcc/7/*.S");
                }
                else if (gccMeta.CompilerMajorVersion < 6)
                {
                    asmSource.AddFiles("$(packagedir)/source/gcc/5/*.S");
                }
                else if (gccMeta.CompilerMajorVersion < 5)
                {
                    asmSource.AddFiles("$(packagedir)/source/gcc/4/*.S");
                }
                else
                {
                    throw new Bam.Core.Exception("No assembly code found for GCC version {0}", gccMeta.CompilerMajorVersion);
                }
                if (this.BitDepth == C.EBit.ThirtyTwo)
                {
                    asmSource["helloworld64.S"].ForEach(item => (item as C.ObjectFileBase).PerformCompilation = false);
                }
                else
                {
                    asmSource["helloworld32.S"].ForEach(item => (item as C.ObjectFileBase).PerformCompilation = false);
                }
            }

            if (this.Linker is VisualCCommon.LinkerBase)
            {
                this.PrivatePatch(settings =>
                    {
                        var linker = settings as C.ICommonLinkerSettings;
                        linker.Libraries.AddUnique("kernel32.lib");
                        linker.Libraries.AddUnique("ucrt.lib");

                        var additional = settings as C.IAdditionalSettings;
                        additional.AdditionalSettings.AddUnique("-entry:main");
                    });
            }
        }
    }
}
