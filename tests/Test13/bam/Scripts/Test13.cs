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
namespace Test13
{
    class DynamicLibraryA :
        C.DynamicLibrary
    {
        protected override void
        Init()
        {
            base.Init();

            this.SetSemanticVersion(Bam.Core.Graph.Instance.ProcessState as Bam.Core.ISemanticVersion);
            this.Macros["Description"] = Bam.Core.TokenizedString.CreateVerbatim("Test13: Example dynamic library A");

            this.CreateHeaderCollection("$(packagedir)/include/dynamicLibraryA.h");
            this.CreateCSourceCollection("$(packagedir)/source/dynamicLibraryA.c");
            this.PublicPatch((settings, appliedTo) =>
                {
                    if (settings is C.ICommonPreprocessorSettings preprocessor)
                    {
                        preprocessor.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/include"));
                    }
                });
        }
    }

    class DynamicLibraryB :
        C.DynamicLibrary
    {
        protected override void
        Init()
        {
            base.Init();

            this.SetSemanticVersion(Bam.Core.Graph.Instance.ProcessState as Bam.Core.ISemanticVersion);
            this.Macros["Description"] = Bam.Core.TokenizedString.CreateVerbatim("Test13: Example dynamic library B");

            this.CreateHeaderCollection("$(packagedir)/include/dynamicLibraryB.h");
            this.CreateCSourceCollection("$(packagedir)/source/dynamicLibraryB.c");
            this.PublicPatch((settings, appliedTo) =>
                {
                    if (settings is C.ICommonPreprocessorSettings preprocessor)
                    {
                        preprocessor.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/include"));
                    }
                });

            this.LinkAgainst<DynamicLibraryA>();
        }
    }

    class Application :
        C.ConsoleApplication
    {
        protected override void
        Init()
        {
            base.Init();

            var source = this.CreateCSourceCollection("$(packagedir)/source/main.c");

            this.PrivatePatch(settings =>
                {
                    if (settings is GccCommon.ICommonLinkerSettings gccLinker)
                    {
                        gccLinker.CanUseOrigin = true;
                        gccLinker.RPath.AddUnique("$ORIGIN");
                    }
                });

            this.CompileAndLinkAgainst<DynamicLibraryA>(source);
            this.CompileAndLinkAgainst<DynamicLibraryB>(source);
        }
    }

    sealed class RuntimePackage :
        Publisher.Collation
    {
        protected override void
        Init()
        {
            base.Init();

            this.SetDefaultMacrosAndMappings(EPublishingType.ConsoleApplication);
            this.Include<Application>(C.ConsoleApplication.ExecutableKey);
        }
    }
}
