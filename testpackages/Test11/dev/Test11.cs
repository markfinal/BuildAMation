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
namespace Test11
{
    sealed class CrossPlatformApplicationV2 :
        C.V2.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer();
            source.AddFile("$(pkgroot)/source/main.c");
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                source.AddFile("$(pkgroot)/source/win/win.c");
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Unix))
            {
                source.AddFile("$(pkgroot)/source/unix/unix.c");
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                source.AddFile("$(pkgroot)/source/osx/osx.c");
            }

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDKV2>();
            }
        }
    }

    // Define module classes here
    class CrossPlatformApplication :
        C.Application
    {
        public
        CrossPlatformApplication()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.commonSourceFile.Include(sourceDir, "main.c");
            var winDir = sourceDir.SubDirectory("win");
            var unixDir = sourceDir.SubDirectory("unix");
            var osxDir = sourceDir.SubDirectory("osx");
            this.winSourceFile.Include(winDir, "win.c");
            this.unixSourceFile.Include(unixDir, "unix.c");
            this.osxSourceFile.Include(osxDir, "osx.c");
        }

        [Bam.Core.SourceFiles]
        C.ObjectFile commonSourceFile = new C.ObjectFile();

        [Bam.Core.SourceFiles(Platform=Bam.Core.EPlatform.Windows)]
        C.ObjectFile winSourceFile = new C.ObjectFile();

        [Bam.Core.SourceFiles(Platform=Bam.Core.EPlatform.Unix)]
        C.ObjectFile unixSourceFile = new C.ObjectFile();

        [Bam.Core.SourceFiles(Platform=Bam.Core.EPlatform.OSX)]
        C.ObjectFile osxSourceFile = new C.ObjectFile();

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray WinVCDependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray WinVCLibraries = new Bam.Core.StringArray("KERNEL32.lib");
    }
}
