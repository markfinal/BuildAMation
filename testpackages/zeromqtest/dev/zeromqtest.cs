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
namespace zeromqtest
{
    public sealed class TestV2 :
        C.Cxx.V2.ConsoleApplication
    {
        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCxxSourceContainer();
            source.AddFile("$(pkgroot)/source/main.cpp");

            this.CompileAndLinkAgainst<zeromq.ZMQSharedLibraryV2>(source);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.CompileAndLinkAgainst<WindowsSDK.WindowsSDKV2>(source);
            }

            this.PrivatePatch(settings =>
                {
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Unix))
                    {
                        var gccLinker = settings as GccCommon.V2.ICommonLinkerOptions;
                        if (null != gccLinker)
                        {
                            gccLinker.CanUseOrigin = true;
                            gccLinker.RPath.AddUnique("$ORIGIN");
                        }
                    }
                });
        }
    }

    public sealed class RuntimePackage :
        Publisher.V2.Package
    {
        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.Include<TestV2>(C.V2.ConsoleApplication.Key, ".");
            this.Include<zeromq.ZMQSharedLibraryV2>(C.V2.DynamicLibrary.Key, ".");
        }
    }


    class Test :
        C.Application
    {
        class SourceFiles :
            C.Cxx.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "*.cpp");
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles source = new SourceFiles();

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(
            typeof(zeromq.ZMQSharedLibrary)
            );

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
        System.Type primary = typeof(Test);
    }
#endif
}
