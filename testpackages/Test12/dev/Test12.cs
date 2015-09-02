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
namespace Test12
{
    sealed class MyOpenGLApplicationV2 :
        C.V2.ConsoleApplication // TODO: windowed application
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCxxSourceContainer();
            source.AddFile("$(pkgroot)/source/main.cpp");
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                source.AddFile("$(pkgroot)/source/win/win.cpp");
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Unix))
            {
                source.AddFile("$(pkgroot)/source/unix/unix.cpp");
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                source.AddFile("$(pkgroot)/source/osx/osx.cpp");
            }

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.CompilePubliclyAndLinkAgainst<WindowsSDK.WindowsSDKV2>(source);

                this.PrivatePatch(settings =>
                    {
                        var linker = settings as C.V2.ICommonLinkerOptions;
                        linker.Libraries.Add("USER32.lib");
                    });
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Unix))
            {
                this.PrivatePatch(settings =>
                    {
                        var linker = settings as C.V2.ICommonLinkerOptions;
                        linker.Libraries.Add("-lX11");
                    });
            }
        }
    }

    // Define module classes here
    class MyOpenGLApplication :
        C.WindowsApplication
    {
        class CommonSourceFiles :
            C.Cxx.ObjectFileCollection
        {
            public
            CommonSourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "main.cpp");
            }
        }

        class WindowsSourceFiles :
            C.Cxx.ObjectFileCollection
        {
            public
            WindowsSourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                var winSourceDir = sourceDir.SubDirectory("win");
                this.Include(winSourceDir, "win.cpp");
            }
        }

        class UnixSourceFiles :
            C.Cxx.ObjectFileCollection
        {
            public
            UnixSourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                var unixSourceDir = sourceDir.SubDirectory("unix");
                this.Include(unixSourceDir, "unix.cpp");
            }
        }

        class OSXSourceFiles :
            C.Cxx.ObjectFileCollection
        {
            public
            OSXSourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                var osxSourceDir = sourceDir.SubDirectory("osx");
                this.Include(osxSourceDir, "osx.cpp");
            }
        }

        [Bam.Core.SourceFiles]
        CommonSourceFiles commonSourceFiles = new CommonSourceFiles();
        [Bam.Core.SourceFiles(Platform=Bam.Core.EPlatform.Windows)]
        WindowsSourceFiles windowsSourceFiles = new WindowsSourceFiles();
        [Bam.Core.SourceFiles(Platform=Bam.Core.EPlatform.Unix)]
        UnixSourceFiles unixSourceFiles = new UnixSourceFiles();
        [Bam.Core.SourceFiles(Platform=Bam.Core.EPlatform.OSX)]
        OSXSourceFiles osxSourceFiles = new OSXSourceFiles();

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray windowsVCDependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray windowsVCLibraries = new Bam.Core.StringArray(
            "KERNEL32.lib",
            "USER32.lib"
        );

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Unix)]
        Bam.Core.StringArray unixLibraries = new Bam.Core.StringArray(
            "-lX11"
        );
    }
}
