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
using VisualCCommon.Helpers;
using ClangCommon.Helpers;
using GccCommon.Helpers;
using MingwCommon.Helpers;
namespace NonPackageNamespaceTest1
{
    // Note: this is not sealed, so will not build by default
    class Library :
        C.DynamicLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(packagedir)/include/*.h");
            var source = this.CreateCSourceContainer("$(packagedir)/source/*.c");

            source.SetVisualCWarningLevel(level: 4);
            source.SetClangWarningOptions(allWarnings:true, extraWarnings: true, pedantic: true);
            source.SetGccWarningOptions(allWarnings: true, extraWarnings: true, pedantic: true);
            source.SetMingwWarningOptions(allWarnings: true, extraWarnings: true, pedantic: true);

            this.PublicPatch((settings, appliedTo) =>
            {
                if (settings is C.ICommonPreprocessorSettings preprocessor)
                {
                    preprocessor.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/include"));
                }
            });
        }
    }

    // special BAM sub-namespace, which is only inspected for Modules when -t or --tests is passed to BAM
    namespace tests
    {
        // not sealed
        class Test1 :
            C.ConsoleApplication
        {
            protected override void
            Init(
                Bam.Core.Module parent)
            {
                base.Init(parent);

                var source = this.CreateCSourceContainer("$(packagedir)/tests/test1.c");
                this.CompileAndLinkAgainst<Library>(source);

                source.SetVisualCWarningLevel(level: 4);
                source.SetClangWarningOptions(allWarnings: true, extraWarnings: true, pedantic: true);
                source.SetGccWarningOptions(allWarnings: true, extraWarnings: true, pedantic: true);
                source.SetMingwWarningOptions(allWarnings: true, extraWarnings: true, pedantic: true);

                this.FindSharedObjectsNextToExecutable();
            }
        }

        // not sealed
        class Test2 :
            C.ConsoleApplication
        {
            protected override void
            Init(
                Bam.Core.Module parent)
            {
                base.Init(parent);

                var source = this.CreateCSourceContainer("$(packagedir)/tests/test2.c");
                this.CompileAndLinkAgainst<Library>(source);

                source.SetVisualCWarningLevel(level: 4);
                source.SetClangWarningOptions(allWarnings: true, extraWarnings: true, pedantic: true);
                source.SetGccWarningOptions(allWarnings: true, extraWarnings: true, pedantic: true);
                source.SetMingwWarningOptions(allWarnings: true, extraWarnings: true, pedantic: true);

                this.FindSharedObjectsNextToExecutable();
            }
        }

        // publishing Module in the tests namespace is sealed, as the only discoverable auto-build
        // Module in this package
        sealed class TestCollection :
            Publisher.Collation
        {
            protected override void
            Init(
                Bam.Core.Module parent)
            {
                base.Init(parent);

                this.SetDefaultMacrosAndMappings(EPublishingType.ConsoleApplication);
                this.IncludeAllModulesInNamespace("NonPackageNamespaceTest1.tests", C.ConsoleApplication.ExecutableKey);
            }
        }
    }
}
