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
namespace Test10
{
    class MyStaticLibrary :
        C.StaticLibrary
    {
        public
        MyStaticLibrary()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.sourceFile.Include(sourceDir, "stlib.c");
        }

        [Bam.Core.SourceFiles]
        C.ObjectFile sourceFile = new C.ObjectFile();
    }

    class MyDynamicLibrary :
        C.DynamicLibrary
    {
        public
        MyDynamicLibrary(
            Bam.Core.Target target)
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.sourceFile.Include(sourceDir, "dylib.c");

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

        [Bam.Core.SourceFiles]
        C.ObjectFile sourceFile = new C.ObjectFile();

        [Bam.Core.DependentModules(Platform=Bam.Core.EPlatform.Windows, ToolsetTypes=new[]{typeof(VisualC.Toolset)})]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray libraries = new Bam.Core.StringArray("KERNEL32.lib");

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations]
        Bam.Core.Array<Publisher.PublishDependency> publishKeys = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.DynamicLibrary.OutputFile));
#endif
    }

    class MyStandaloneApp :
        C.Application
    {
        public
        MyStandaloneApp()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.sourceFile.Include(sourceDir, "standaloneapp.c");
        }

        [Bam.Core.SourceFiles]
        C.ObjectFile sourceFile = new C.ObjectFile();

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(typeof(MyStaticLibrary));

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray windowsDependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray libraries = new Bam.Core.StringArray("KERNEL32.lib");
    }

    class DllDependentApp :
        C.Application
    {
        public
        DllDependentApp()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.sourceFile.Include(sourceDir, "dlldependentapp.c");

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

        [Bam.Core.SourceFiles]
        C.ObjectFile sourceFile = new C.ObjectFile();

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(typeof(MyDynamicLibrary));

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray windowsDependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray libraries = new Bam.Core.StringArray("KERNEL32.lib");

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
        System.Type primary = typeof(DllDependentApp);
    }
#endif
}
