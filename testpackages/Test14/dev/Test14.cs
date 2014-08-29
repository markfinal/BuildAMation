#region License
// Copyright 2010-2014 Mark Final
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
namespace Test14
{
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
