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
namespace Test4
{
    sealed class MyDynamicLibV2 :
        C.V2.DynamicLibrary
    {
        private Bam.Core.V2.Module.PublicPatchDelegate includePaths = (settings, appliedTo) =>
            {
                var compiler = settings as C.V2.ICommonCompilerOptions;
                if (null != compiler)
                {
                    compiler.IncludePaths.Add(Bam.Core.V2.TokenizedString.Create("$(pkgroot)/include", appliedTo));
                }
            };

        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.LinkAgainst<MyStaticLibV2>();

            this.CreateHeaderContainer("$(pkgroot)/include/dynamiclibrary.h");

            var source = this.CreateCSourceContainer("$(pkgroot)/source/dynamiclibrary.c");

            source.PrivatePatch(settings => this.includePaths(settings, this));
            this.PublicPatch((settings, appliedTo) => this.includePaths(settings, this));

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                // TODO: simplify
                var windowsSDK = Bam.Core.V2.Graph.Instance.FindReferencedModule<WindowsSDK.WindowsSDKV2>();
                this.Requires(windowsSDK);
                source.UsePublicPatches(windowsSDK); // compiling
                this.UsePublicPatches(windowsSDK); // linking
            }
        }
    }

    sealed class MyStaticLibV2 :
        C.V2.StaticLibrary
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(pkgroot)/include/staticlibrary.h");

            var source = this.CreateCSourceContainer("$(pkgroot)/source/staticlibrary.c");

            source.PublicPatch((settings, appliedTo) =>
            {
                var compiler = settings as C.V2.ICommonCompilerOptions;
                compiler.IncludePaths.Add(Bam.Core.V2.TokenizedString.Create("$(pkgroot)/include", this));
            });
        }
    }

    // Define module classes here
    class MyDynamicLib :
        C.DynamicLibrary
    {
        public
        MyDynamicLib(
            Bam.Core.Target target)
        {
            var includeDir = this.PackageLocation.SubDirectory("include");
            this.headerFiles.Include(includeDir, "dynamiclibrary.h");

#if D_PACKAGE_PUBLISHER_DEV
            // TODO: can this be automated?
            if (Bam.Core.OSUtilities.IsLinuxHosting)
            {
                this.publish.AddUnique(new Publisher.PublishDependency(C.PosixSharedLibrarySymlinks.MajorVersionSymlink));
                this.publish.AddUnique(new Publisher.PublishDependency(C.PosixSharedLibrarySymlinks.MinorVersionSymlink));
                this.publish.AddUnique(new Publisher.PublishDependency(C.PosixSharedLibrarySymlinks.LinkerSymlink));
            }
#endif
        }

        class SourceFiles :
            C.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "dynamiclibrary.c");
                this.UpdateOptions += SetIncludePaths;
                this.UpdateOptions += SetRuntimeLibrary;
            }

            [C.ExportCompilerOptionsDelegate]
            private void
            SetIncludePaths(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Include(this.PackageLocation.SubDirectory("include"));
            }

            [C.ExportCompilerOptionsDelegate]
            private static void
            SetRuntimeLibrary(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var vcCompilerOptions = module.Options as VisualCCommon.ICCompilerOptions;
                if (vcCompilerOptions != null)
                {
                    vcCompilerOptions.RuntimeLibrary = VisualCCommon.ERuntimeLibrary.MultiThreadedDebugDLL;
                }
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [C.HeaderFiles]
        Bam.Core.FileCollection headerFiles = new Bam.Core.FileCollection();

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(typeof(MyStaticLib));

        [Bam.Core.DependentModules(Platform=Bam.Core.EPlatform.Windows, ToolsetTypes=new[]{typeof(VisualC.Toolset)})]
        Bam.Core.TypeArray winVCDependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray libraries = new Bam.Core.StringArray("KERNEL32.lib");

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations]
        Bam.Core.Array<Publisher.PublishDependency> publish = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.DynamicLibrary.OutputFile)
            );
#endif
    }

    class MyStaticLib :
        C.StaticLibrary
    {
        public
        MyStaticLib()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.sourceFile.Include(sourceDir, "staticlibrary.c");
            this.sourceFile.UpdateOptions += SetIncludePaths;

            var includeDir = this.PackageLocation.SubDirectory("include");
            this.headerFiles.Include(includeDir, "staticlibrary.h");
        }

        [Bam.Core.SourceFiles]
        C.ObjectFile sourceFile = new C.ObjectFile();

        [C.HeaderFiles]
        Bam.Core.FileCollection headerFiles = new Bam.Core.FileCollection();

        [C.ExportCompilerOptionsDelegate]
        private void SetIncludePaths(Bam.Core.IModule module, Bam.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Include(this.PackageLocation.SubDirectory("include"));
        }
    }
}
