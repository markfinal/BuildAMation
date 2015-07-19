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
namespace Test8
{
    [Bam.Core.V2.PlatformFilter(Bam.Core.EPlatform.Windows)]
    sealed class ApplicationTestV2 :
        C.V2.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer();
            source.AddFile("$(pkgroot)/source/main.c");

            var dynamicLib = Bam.Core.V2.Graph.Instance.FindReferencedModule<Test7.ExplicitDynamicLibraryV2>();
            this.Requires(dynamicLib);
            source.UsePublicPatches(dynamicLib);

            if (this.Linker is VisualC.V2.LinkerBase)
            {
                this.CompileAndLinkAgainst<WindowsSDK.WindowsSDKV2>(source);

                this.PrivatePatch(settings =>
                    {
                        var linker = settings as C.V2.ICommonLinkerOptions;
                        linker.Libraries.Add("dbghelp.lib");
                    });
            }
        }
    }

    [Bam.Core.V2.PlatformFilter(Bam.Core.EPlatform.Windows)]
    sealed class RuntimePackage :
        Publisher.V2.Package
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.Include<ApplicationTestV2>(C.V2.ConsoleApplication.Key, ".");
            this.Include<Test7.ExplicitDynamicLibraryV2>(C.V2.DynamicLibrary.Key, ".");
        }
    }

    // Define module classes here

    [Bam.Core.ModuleTargets(Platform=Bam.Core.EPlatform.Windows)]
    class ApplicationTest :
        C.Application
    {
        public
        ApplicationTest()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.sourceFile.Include(sourceDir, "main.c");
        }

        [Bam.Core.SourceFiles]
        C.ObjectFile sourceFile = new C.ObjectFile();

        [Bam.Core.RequiredModules]
        Bam.Core.TypeArray requiredModules = new Bam.Core.TypeArray(
            typeof(Test7.ExplicitDynamicLibrary)
        );

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray winVCDependents = new Bam.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray libraries = new Bam.Core.StringArray(
            "KERNEL32.lib",
            "dbghelp.lib"
        );

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations]
        Bam.Core.Array<Publisher.PublishDependency> publishKeys = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.Application.OutputFile));
#endif
    }

#if D_PACKAGE_PUBLISHER_DEV
    [Bam.Core.ModuleTargets(Platform=Bam.Core.EPlatform.Windows)]
    class Publish :
        Publisher.ProductModule
    {
        [Publisher.PrimaryTarget]
        System.Type primary = typeof(ApplicationTest);
    }
#endif
}
