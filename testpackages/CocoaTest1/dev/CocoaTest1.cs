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
namespace CocoaTest1
{
    [Bam.Core.V2.PlatformFilter(Bam.Core.EPlatform.OSX)]
    sealed class CLibraryV2 :
        C.V2.StaticLibrary
    {
        protected override void Init (Bam.Core.V2.Module parent)
        {
            base.Init (parent);

            var source = this.CreateCSourceContainer();
            source.AddFile("$(pkgroot)/source/library.c");
        }
    }

    [Bam.Core.V2.PlatformFilter(Bam.Core.EPlatform.OSX)]
    sealed class CocoaTestV2 :
        C.V2.GUIApplication
    {
        protected override void Init (Bam.Core.V2.Module parent)
        {
            base.Init (parent);

            var source = this.CreateObjectiveCSourceContainer();
            source.AddFile("$(pkgroot)/source/main.m");

            this.LinkAgainst<CLibraryV2>();

            this.PrivatePatch(settings =>
                {
                    var linker = settings as C.V2.ILinkerOptionsOSX;
                    if (null != linker)
                    {
                        linker.Frameworks.Add(Bam.Core.V2.TokenizedString.Create("Cocoa", null, verbatim:true));
                    }
                });
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

            this.Include<CocoaTestV2>(C.V2.ConsoleApplication.Key, EPublishingType.WindowedApplication);
        }
    }

    [Bam.Core.ModuleTargets(Platform=Bam.Core.EPlatform.OSX)]
    class CLibrary :
        C.StaticLibrary
    {
        class Source :
            C.ObjectFileCollection
        {
            public
            Source()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "*.c");
            }
        }

        [Bam.Core.SourceFiles]
        Source source = new Source();
    }

    [Bam.Core.ModuleTargets(Platform=Bam.Core.EPlatform.OSX)]
    class CocoaTest :
        C.WindowsApplication
    {
        public
        CocoaTest()
        {
            this.UpdateOptions += delegate(Bam.Core.IModule module, Bam.Core.Target target) {
                var link = module.Options as C.ILinkerOptionsOSX;
                link.Frameworks.Add("Cocoa");
            };
        }

        class Source :
            C.ObjC.ObjectFileCollection
        {
            public
            Source()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "*.m");
            }
        }

        [Bam.Core.SourceFiles]
        Source source = new Source();

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(
            typeof(CLibrary)
        );

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations]
        Bam.Core.Array<Publisher.PublishDependency> publishKeys = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.Application.OutputFile));
#endif
    }

    [Bam.Core.ModuleTargets(Platform=Bam.Core.EPlatform.OSX)]
    class CocoaTestPlist :
        XmlUtilities.OSXPlistModule
    {
        public
        CocoaTestPlist()
        {
            this.UpdateOptions += delegate(Bam.Core.IModule module, Bam.Core.Target target) {
                var options = module.Options as XmlUtilities.IOSXPlistOptions;
                options.CFBundleName = "CocoaTest1";
                options.CFBundleDisplayName = "CocoaTest1";
                options.CFBundleIdentifier = "CocoaTest1";
                options.CFBundleVersion = "1.0.0";
            };
        }

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(
            typeof(CocoaTest)
            );

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations]
        Publisher.PublishDependency publishKey = new Publisher.PublishDependency(XmlUtilities.OSXPlistModule.OutputFile);
#endif
    }

#if D_PACKAGE_PUBLISHER_DEV
    [Bam.Core.ModuleTargets(Platform=Bam.Core.EPlatform.OSX)]
    class Publish :
        Publisher.ProductModule
    {
        public
        Publish()
        {
            this.UpdateOptions += delegate(Bam.Core.IModule module, Bam.Core.Target target)
            {
                var options = module.Options as Publisher.IPublishOptions;
                if (null != options)
                {
                    options.OSXApplicationBundle = true;
                }
            };
        }

        [Publisher.PrimaryTarget]
        System.Type primary = typeof(CocoaTest);

        [Publisher.OSXInfoPList]
        System.Type plistType = typeof(CocoaTestPlist);
    }
#endif
}
