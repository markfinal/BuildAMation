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
namespace CocoaTest1
{
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
