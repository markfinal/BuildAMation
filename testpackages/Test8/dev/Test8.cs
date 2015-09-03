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
using Bam.Core.V2; // for EPlatform.PlatformExtensions
namespace Test8
{
    [Bam.Core.V2.PlatformFilter(Bam.Core.EPlatform.Windows)]
    sealed class ApplicationTestV2 :
        C.V2.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer("$(pkgroot)/source/main.c");

            var dynamicLib = Bam.Core.V2.Graph.Instance.FindReferencedModule<Test7.ExplicitDynamicLibraryV2>();
            this.Requires(dynamicLib);
            source.UsePublicPatches(dynamicLib);

            if (this.Linker is VisualC.V2.LinkerBase)
            {
                this.CompilePubliclyAndLinkAgainst<WindowsSDK.WindowsSDKV2>(source);

                this.PrivatePatch(settings =>
                    {
                        var linker = settings as C.V2.ICommonLinkerOptions;
                        linker.Libraries.Add("dbghelp.lib");
                    });
            }
        }
    }

    [Bam.Core.V2.PlatformFilter(Bam.Core.EPlatform.Windows)]
    sealed class RuntimePackage :
        Publisher.V2.Package
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var app = this.Include<ApplicationTestV2>(C.V2.ConsoleApplication.Key, EPublishingType.ConsoleApplication);
            this.Include<Test7.ExplicitDynamicLibraryV2>(C.V2.DynamicLibrary.Key, ".", app);
        }
    }

    // Define module classes here

    [Bam.Core.ModuleTargets(Platform=Bam.Core.EPlatform.Windows)]
    class ApplicationTest :
        C.Application
    {
        public
        ApplicationTest()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.sourceFile.Include(sourceDir, "main.c");
        }

        [Bam.Core.SourceFiles]
        C.ObjectFile sourceFile = new C.ObjectFile();

        [Bam.Core.RequiredModules]
        Bam.Core.TypeArray requiredModules = new Bam.Core.TypeArray(
            typeof(Test7.ExplicitDynamicLibrary)
        );

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray winVCDependents = new Bam.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray libraries = new Bam.Core.StringArray(
            "KERNEL32.lib",
            "dbghelp.lib"
        );

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations]
        Bam.Core.Array<Publisher.PublishDependency> publishKeys = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.Application.OutputFile));
#endif
    }

#if D_PACKAGE_PUBLISHER_DEV
    [Bam.Core.ModuleTargets(Platform=Bam.Core.EPlatform.Windows)]
    class Publish :
        Publisher.ProductModule
    {
        [Publisher.PrimaryTarget]
        System.Type primary = typeof(ApplicationTest);
    }
#endif
}
