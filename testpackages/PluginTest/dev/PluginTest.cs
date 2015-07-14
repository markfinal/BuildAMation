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
namespace PluginTest
{
    public sealed class ApplicationV2 :
        C.Cxx.V2.ConsoleApplication
    {
        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCxxSourceContainer();
            source.AddFile("$(pkgroot)/source/application/main.cpp");

            var plugin = Bam.Core.V2.Graph.Instance.FindReferencedModule<PluginV2>();
            this.Requires(plugin);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDKV2>();
            }
        }
    }

    public sealed class PluginV2 :
        C.Cxx.V2.DynamicLibrary
    {
        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCxxSourceContainer();
            source.AddFile("$(pkgroot)/source/plugin/pluginmain.cpp");

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
        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.Include<ApplicationV2>(C.V2.ConsoleApplication.Key, ".");
            this.Include<PluginV2>(C.V2.DynamicLibrary.Key, ".");
        }
    }

    class Application : C.Application
    {
        class Source : C.Cxx.ObjectFileCollection
        {
            public Source()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                var appDir = sourceDir.SubDirectory("application");
                this.Include(appDir, "*.cpp");
            }
        }

        [Bam.Core.SourceFiles]
        Source source = new Source();

        [Bam.Core.RequiredModulesAttribute]
        Bam.Core.TypeArray requiredDependents = new Bam.Core.TypeArray(
            typeof(Plugin)
            );

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations]
        Bam.Core.Array<Publisher.PublishDependency> publishKeys = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.Application.OutputFile));
#endif
    }

    class Plugin : C.DynamicLibrary
    {
        public Plugin()
        {
#if D_PACKAGE_PUBLISHER_DEV
            // TODO: can this be automated?
            if (Bam.Core.OSUtilities.IsUnixHosting)
            {
                this.publishKeys.AddUnique(new Publisher.PublishDependency(C.PosixSharedLibrarySymlinks.MajorVersionSymlink));
                this.publishKeys.AddUnique(new Publisher.PublishDependency(C.PosixSharedLibrarySymlinks.MinorVersionSymlink));
                this.publishKeys.AddUnique(new Publisher.PublishDependency(C.PosixSharedLibrarySymlinks.LinkerSymlink));
            }
#endif
        }

        class Source : C.Cxx.ObjectFileCollection
        {
            public Source()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                var pluginDir = sourceDir.SubDirectory("plugin");
                this.Include(pluginDir, "*.cpp");
            }
        }

        [Bam.Core.SourceFiles]
        Source source = new Source();

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations]
        Bam.Core.Array<Publisher.PublishDependency> publishKeys = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.DynamicLibrary.OutputFile));
#endif
    }

#if D_PACKAGE_PUBLISHER_DEV
    class Publish : Publisher.ProductModule
    {
        [Publisher.PrimaryTarget]
        System.Type primary = typeof(Application);
    }
#endif
}
