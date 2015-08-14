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
namespace Test5
{
    sealed class MyDynamicLibTestAppV2 :
        C.V2.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.PrivatePatch(settings =>
                {
                    var gccCommon = settings as GccCommon.V2.ICommonLinkerOptions;
                    if (null != gccCommon)
                    {
                        gccCommon.CanUseOrigin = true;
                        gccCommon.RPath.AddUnique("$ORIGIN");
                    }
                });

            var source = this.CreateCSourceContainer();
            source.AddFile("$(pkgroot)/source/dynamicmain.c");

            this.LinkAgainst<Test4.MyStaticLibV2>();
            this.CompileAndLinkAgainst<Test4.MyDynamicLibV2>(source);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDKV2>();
            }
        }
    }

    sealed class RuntimePackage :
        Publisher.V2.Package
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var app = this.Include<MyDynamicLibTestAppV2>(C.V2.ConsoleApplication.Key);
            this.Include<Test4.MyDynamicLibV2>(C.V2.DynamicLibrary.Key, ".", app);
        }
    }

    sealed class SDKPackage :
        Publisher.V2.Package
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var dll = this.Include<Test4.MyDynamicLibV2>(C.V2.DynamicLibrary.Key, "bin");
            if (Bam.Core.OSUtilities.IsWindowsHosting)
            {
                this.Include<Test4.MyDynamicLibV2>(C.V2.DynamicLibrary.ImportLibraryKey, "../lib", dll);
            }

            this.IncludeFiles<Test4.MyDynamicLibV2>("$(pkgroot)/include/dynamiclibrary.h", "../include", dll);
        }
    }

    sealed class WinInstallerInno :
        Publisher.V2.InnoSetupInstaller
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.SourceFolder<RuntimePackage>(Publisher.V2.Package.PackageRoot);
        }
    }

    sealed class WinInstallerNSIS :
        Publisher.V2.NSISInstaller
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.SourceFolder<RuntimePackage>(Publisher.V2.Package.PackageRoot);
        }
    }

    sealed class TarBallInstaller :
        Publisher.V2.TarBall
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.Include<MyDynamicLibTestAppV2>(C.V2.ConsoleApplication.Key);
        }
    }

    sealed class DiskImageInstaller :
        Publisher.V2.DiskImage
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.SourceFolder<RuntimePackage>(Publisher.V2.Package.PackageRoot);
        }
    }

    // Define module classes here
    class MyDynamicLibTestApp :
        C.Application
    {
        public
        MyDynamicLibTestApp()
        {
            this.UpdateOptions += delegate(Bam.Core.IModule module, Bam.Core.Target target)
            {
                var linkerOptions = module.Options as GccCommon.ILinkerOptions;
                if (null != linkerOptions)
                {
                    linkerOptions.CanUseOrigin = true;
                    linkerOptions.RPath.Add("$ORIGIN");
                }
            };
        }

        class SourceFiles :
            C.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "dynamicmain.c");
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(
            typeof(Test4.MyDynamicLib),
            typeof(Test4.MyStaticLib)
        );

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray winVCDependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray libraries = new Bam.Core.StringArray("KERNEL32.lib");

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations]
        Bam.Core.Array<Publisher.PublishDependency> publish = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.Application.OutputFile)
            );
#endif
    }

#if D_PACKAGE_PUBLISHER_DEV
    class Publish :
        Publisher.ProductModule
    {
        [Publisher.PrimaryTarget]
        System.Type primary = typeof(MyDynamicLibTestApp);
    }
#endif
}
