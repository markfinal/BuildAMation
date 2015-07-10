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
namespace Test4
{
    sealed class MyDynamicLibV2 :
        C.V2.DynamicLibrary
    {
        public MyDynamicLibV2()
        {
            this.LinkAgainst<MyStaticLibV2>();

            var source = this.CreateCSourceContainer();
            source.AddFile("$(pkgroot)/source/dynamiclibrary.c");

            source.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.V2.ICommonCompilerOptions;
                    compiler.IncludePaths.Add(Bam.Core.V2.TokenizedString.Create("$(pkgroot)/include", this));
                });

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.Linker)
            {
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
        public MyStaticLibV2()
        {
            var source = this.CreateCSourceContainer();
            source.AddFile("$(pkgroot)/source/staticlibrary.c");

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
            if (Bam.Core.OSUtilities.IsUnixHosting)
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
