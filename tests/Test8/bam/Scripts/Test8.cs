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
using Bam.Core;
namespace Test8
{
    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Windows)]
    sealed class ApplicationTest :
        C.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer("$(pkgroot)/source/main.c");

            // TODO: required to exist?
            var dynamicLib = Bam.Core.Graph.Instance.FindReferencedModule<Test7.ExplicitDynamicLibrary>();
            this.Requires(dynamicLib);
            source.UsePublicPatches(dynamicLib);

            if (this.Linker is VisualC.LinkerBase)
            {
                this.CompilePubliclyAndLinkAgainst<WindowsSDK.WindowsSDK>(source);

                this.PrivatePatch(settings =>
                    {
                        var linker = settings as C.ICommonLinkerOptions;
                        linker.Libraries.Add("dbghelp.lib");
                    });
            }
        }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Windows)]
    sealed class RuntimePackage :
        Publisher.Package
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
