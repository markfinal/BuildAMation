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
namespace Test5
{
    sealed class MyDynamicLibTestAppV2 :
        C.V2.ConsoleApplication
    {
        public MyDynamicLibTestAppV2()
        {
            this.PrivatePatch(settings =>
                {
                    var gccCommon = settings as GccCommon.V2.ICommonLinkerOptions;
                    if (null != gccCommon)
                    {
                        gccCommon.CanUseOrigin = true;
                        gccCommon.RPath.AddUnique("$ORIGIN");
                    }
                });

            var source = this.CreateCSourceContainer();
            source.AddFile("$(pkgroot)/source/dynamicmain.c");

            var staticLib = this.LinkAgainst<Test4.MyStaticLibV2>();
            var dynamicLib = this.LinkAgainst<Test4.MyDynamicLibV2>();

            source.UsePublicPatches(staticLib.Source[0]);
            source.UsePublicPatches(dynamicLib);

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

    sealed class RuntimePackage :
        Publisher.V2.Package
    {
        public RuntimePackage()
        {
            this.Include<MyDynamicLibTestAppV2>(C.V2.ConsoleApplication.Key, ".");
            this.Include<Test4.MyDynamicLibV2>(C.V2.DynamicLibrary.Key, ".");
        }
    }

    sealed class SDKPackage :
        Publisher.V2.Package
    {
        public SDKPackage()
        {
            if (Bam.Core.OSUtilities.IsWindowsHosting)
            {
                this.Include<Test4.MyDynamicLibV2>(C.V2.DynamicLibrary.ImportLibraryKey, "lib");
            }
            this.Include<Test4.MyDynamicLibV2>(C.V2.DynamicLibrary.Key, "bin");

            this.IncludeFiles<Test4.MyDynamicLibV2>("$(pkgroot)/include/dynamiclibrary.h", "include");
        }
    }

    [Bam.Core.V2.PlatformFilter(Bam.Core.EPlatform.Windows)]
    sealed class Installer :
        Publisher.V2.InnoSetupInstaller
    {
        public Installer()
        {
            this.Include<MyDynamicLibTestAppV2>(C.V2.ConsoleApplication.Key);
        }
    }

    // Define module classes here
    class MyDynamicLibTestApp :
        C.Application
    {
        public
        MyDynamicLibTestApp()
        {
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

        class SourceFiles :
            C.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "dynamicmain.c");
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(
            typeof(Test4.MyDynamicLib),
            typeof(Test4.MyStaticLib)
        );

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray winVCDependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray libraries = new Bam.Core.StringArray("KERNEL32.lib");

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations]
        Bam.Core.Array<Publisher.PublishDependency> publish = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.Application.OutputFile)
            );
#endif
    }

#if D_PACKAGE_PUBLISHER_DEV
    class Publish :
        Publisher.ProductModule
    {
        [Publisher.PrimaryTarget]
        System.Type primary = typeof(MyDynamicLibTestApp);
    }
#endif
}
