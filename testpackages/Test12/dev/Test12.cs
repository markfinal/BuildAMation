#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
using Bam.Core.V2; // for EPlatform.PlatformExtensions
namespace Test12
{
    sealed class MyOpenGLApplicationV2 :
        C.V2.ConsoleApplication // TODO: windowed application
    {
        public MyOpenGLApplicationV2()
        {
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
                this.Linker is VisualC.V2.Linker)
            {
                var windowsSDK = Bam.Core.V2.Graph.Instance.FindReferencedModule<WindowsSDK.WindowsSDKV2>();
                this.Requires(windowsSDK);
                source.UsePublicPatches(windowsSDK); // compiling
                this.UsePublicPatches(windowsSDK); // linking

                this.PrivatePatch((settings, appliedTo) =>
                    {
                        var linker = settings as C.V2.ICommonLinkerOptions;
                        linker.Libraries.Add("USER32.lib");
                    });
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Unix))
            {
                this.PrivatePatch((settings, appliedTo) =>
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
