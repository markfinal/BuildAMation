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
namespace Test14
{
    public sealed class DynamicLibraryAV2 :
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

            var headers = this.CreateHeaderContainer();
            headers.AddFile("$(pkgroot)/include/dynamicLibraryA.h");

            var source = this.CreateCSourceContainer();
            source.AddFile("$(pkgroot)/source/dynamicLibraryA.c");
            source.PrivatePatch(settings => this.includePaths(settings, this));

            this.PublicPatch((settings, appliedTo) => this.includePaths(settings, appliedTo));

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDKV2>();
            }
        }
    }

    public sealed class DynamicLibraryBV2 :
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

            var headers = this.CreateHeaderContainer();
            headers.AddFile("$(pkgroot)/include/dynamicLibraryB.h");

            var source = this.CreateCSourceContainer();
            source.AddFile("$(pkgroot)/source/dynamicLibraryB.c");
            source.PrivatePatch(settings => this.includePaths(settings, this));

            this.PublicPatch((settings, appliedTo) => this.includePaths(settings, appliedTo));

            this.LinkAgainst<DynamicLibraryAV2>();

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDKV2>();
            }
        }
    }

    public sealed class ApplicationV2 :
        C.V2.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer();
            source.AddFile("$(pkgroot)/source/main.c");

            this.PrivatePatch(settings =>
                {
                    var gccLinker = settings as GccCommon.V2.ICommonLinkerOptions;
                    if (null != gccLinker)
                    {
                        gccLinker.CanUseOrigin = true;
                        gccLinker.RPath.Add("$ORIGIN");
                    }
                });

            this.CompileAndLinkAgainst<DynamicLibraryAV2>(source);
            this.CompileAndLinkAgainst<DynamicLibraryBV2>(source);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDKV2>();
            }
        }
    }

    public sealed class RuntimePackage :
        Publisher.V2.Package
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.Include<ApplicationV2>(C.V2.ConsoleApplication.Key, ".");
            this.Include<DynamicLibraryAV2>(C.V2.DynamicLibrary.Key, ".");
            this.Include<DynamicLibraryBV2>(C.V2.DynamicLibrary.Key, ".");
        }
    }

    // Define module classes here
    class DynamicLibraryA :
        C.DynamicLibrary
    {
        public
        DynamicLibraryA(
            Bam.Core.Target target)
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.source.Include(sourceDir, "dynamicLibraryA.c");
            this.source.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(DynamicLibraryA_IncludePaths);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(DynamicLibraryA_UpdateOptions);

            var includeDir = this.PackageLocation.SubDirectory("include");
            this.header.Include(includeDir, "dynamicLibraryA.h");

#if D_PACKAGE_PUBLISHER_DEV
            // TODO: can this be automated?
            if (target.HasPlatform(Bam.Core.EPlatform.Unix))
            {
                this.publishKeys.Add(new Publisher.PublishDependency(C.PosixSharedLibrarySymlinks.MajorVersionSymlink));
                this.publishKeys.Add(new Publisher.PublishDependency(C.PosixSharedLibrarySymlinks.MinorVersionSymlink));
                this.publishKeys.Add(new Publisher.PublishDependency(C.PosixSharedLibrarySymlinks.LinkerSymlink));
            }
#endif
        }

        [C.ExportCompilerOptionsDelegate]
        void
        DynamicLibraryA_IncludePaths(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Include(this.PackageLocation, "include");
        }

        void
        DynamicLibraryA_UpdateOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var linkerOptions = module.Options as C.ILinkerOptions;
            linkerOptions.DoNotAutoIncludeStandardLibraries = false;
        }

        [Bam.Core.SourceFiles]
        C.ObjectFile source = new C.ObjectFile();

        [C.HeaderFiles]
        Bam.Core.FileCollection header = new Bam.Core.FileCollection();

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray vcDependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations]
        Bam.Core.Array<Publisher.PublishDependency> publishKeys = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.DynamicLibrary.OutputFile));
#endif
    }

    class DynamicLibraryB :
        C.DynamicLibrary
    {
        public
        DynamicLibraryB(
            Bam.Core.Target target)
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.source.Include(sourceDir, "dynamicLibraryB.c");
            this.source.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(DynamicLibraryB_IncludePaths);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(DynamicLibraryB_UpdateOptions);

            var includeDir = this.PackageLocation.SubDirectory("include");
            this.header.Include(includeDir, "dynamicLibraryB.h");

#if D_PACKAGE_PUBLISHER_DEV
            // TODO: can this be automated?
            if (target.HasPlatform(Bam.Core.EPlatform.Unix))
            {
                this.publishKeys.Add(new Publisher.PublishDependency(C.PosixSharedLibrarySymlinks.MajorVersionSymlink));
                this.publishKeys.Add(new Publisher.PublishDependency(C.PosixSharedLibrarySymlinks.MinorVersionSymlink));
                this.publishKeys.Add(new Publisher.PublishDependency(C.PosixSharedLibrarySymlinks.LinkerSymlink));
            }
#endif
        }

        void
        DynamicLibraryB_IncludePaths(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Include(this.PackageLocation, "include");
        }

        void
        DynamicLibraryB_UpdateOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var linkerOptions = module.Options as C.ILinkerOptions;
            linkerOptions.DoNotAutoIncludeStandardLibraries = false;
        }

        [Bam.Core.SourceFiles]
        C.ObjectFile source = new C.ObjectFile();

        [C.HeaderFiles]
        Bam.Core.FileCollection header = new Bam.Core.FileCollection();

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(typeof(DynamicLibraryA));

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray vcDependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations]
        Bam.Core.Array<Publisher.PublishDependency> publishKeys = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.DynamicLibrary.OutputFile));
#endif
    }

    class Application :
        C.Application
    {
        public
        Application()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.source.Include(sourceDir, "main.c");
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(Application_UpdateOptions);
        }

        void
        Application_UpdateOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var linkerOptions = module.Options as C.ILinkerOptions;
            if (null != linkerOptions)
            {
                linkerOptions.DoNotAutoIncludeStandardLibraries = false;
            }
            var gccLink = module.Options as GccCommon.ILinkerOptions;
            if (null != gccLink)
            {
                gccLink.CanUseOrigin = true;
                gccLink.RPath.Add("$ORIGIN");
            }
        }

        [Bam.Core.SourceFiles]
        C.ObjectFile source = new C.ObjectFile();

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(
            typeof(DynamicLibraryA),
            typeof(DynamicLibraryB)
        );

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations]
        Bam.Core.Array<Publisher.PublishDependency> publishKeys = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.Application.OutputFile));
#endif
    }

#if D_PACKAGE_PUBLISHER_DEV
    class Publish :
        Publisher.ProductModule
    {
        [Publisher.PrimaryTarget]
        System.Type primary = typeof(Application);
    }
#endif
}
