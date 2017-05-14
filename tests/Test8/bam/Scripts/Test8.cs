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
namespace Test8
{
    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Windows)]
    sealed class ApplicationTest :
        C.ConsoleApplication
    {
        private static void
        SuppressC4091(
            Bam.Core.Settings settings)
        {
            // warning suppression only required for VS2015 and above
            // this is a less common example of a static private patch, so it has no access
            // to the variables generally available to patches inlined in Init()
            // so settings.Module is just a Bam.Core.Module, so utility functions from the C package
            // are provided to get access to properties on the compilable module
            if (settings is VisualCCommon.ICommonCompilerSettings)
            {
                var compilerUsed = C.PatchUtilities.GetCompiler<C.ObjectFile>(settings.Module);
                if (compilerUsed.IsAtLeast(19))
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.DisableWarnings.AddUnique("4091"); // C:\Program Files (x86)\Windows Kits\8.1\Include\um\DbgHelp.h(1544): warning C4091: 'typedef ': ignored on left of '' when no variable is declared
                }
            }
        }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer("$(packagedir)/source/main.c");
            this.RequiredToExist<Test7.ExplicitDynamicLibrary>(source);

            source["main.c"].ForEach(item => item.PrivatePatch(SuppressC4091));

            if (this.Linker is VisualCCommon.LinkerBase)
            {
                this.CompileAndLinkAgainst<WindowsSDK.WindowsSDK>(source);

                this.PrivatePatch(settings =>
                    {
                        var linker = settings as C.ICommonLinkerSettings;
                        linker.Libraries.Add("dbghelp.lib");
                    });
            }
        }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Windows)]
    sealed class RuntimePackage :
        Publisher.Collation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var app = this.Include<ApplicationTest>(C.ConsoleApplication.Key, EPublishingType.ConsoleApplication);
            this.Include<Test7.ExplicitDynamicLibrary>(C.DynamicLibrary.Key, ".", app);
        }
    }
}
