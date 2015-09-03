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

            var source = this.CreateCSourceContainer("$(pkgroot)/source/main.c");
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
