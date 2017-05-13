#region License
// Copyright (c) 2010-2017, Mark Final
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
namespace DeltaSettingsTest2
{
    sealed class Test :
        C.ConsoleApplication
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer("$(packagedir)/source/*.c");
            source.PrivatePatch(settings =>
            {
                var cCompiler = settings as C.ICOnlyCompilerSettings;
                cCompiler.LanguageStandard = C.ELanguageStandard.C89;

                var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                if (null != vcCompiler)
                {
                    vcCompiler.WarningLevel = VisualCCommon.EWarningLevel.Level4;
                }

                var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                if (null != gccCompiler)
                {
                    gccCompiler.AllWarnings = true;
                    gccCompiler.ExtraWarnings = true;
                    gccCompiler.Pedantic = true;

                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("long-long"); // DeltaSettingsTest2/source/main.c:37:10: error: ISO C90 does not support ‘long long’ [-Werror=long-long]
                }

                var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                if (null != clangCompiler)
                {
                    clangCompiler.AllWarnings = true;
                    clangCompiler.ExtraWarnings = true;
                    clangCompiler.Pedantic = true;

                    // this is intentionally common to all source, as it's part of pedantic, which is disabled
                    // for a specific file
                    // the issue arises for the other file and the order of this warning suppression
                    // and enabling 'pedantic' on the Xcode generated command line
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("long-long"); // DeltaSettingsTest2/source/main.c:37:5: error: 'long long' is an extension when C99 mode is not enabled [-Werror,-Wlong-long]
                }
            });

            source["literal"].ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                        if (null != clangCompiler)
                        {
                            clangCompiler.Pedantic = false; // DeltaSettingsTest2/source/literal.c:35:9: error: string literal of length 510 exceeds maximum length 509 that C90 compilers are required to support [-Werror,-Woverlength-strings]
                        }
                        var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                        if (null != gccCompiler)
                        {
                            gccCompiler.Pedantic = false; // DeltaSettingsTest2/source/literal.c:35:9: error: string length ‘510’ is greater than the length ‘509’ ISO C90 compilers are required to support [-Werror=overlength-strings]
                        }
                    }));

            if (this.Linker is VisualCCommon.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
            }
        }
    }
}
