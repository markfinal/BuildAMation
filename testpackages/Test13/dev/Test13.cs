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
using QtCommon.V2.MocExtension;
namespace Test13
{
    sealed class QtApplicationV2 :
        C.V2.ConsoleApplication
    {
        public QtApplicationV2()
        {
            var source = this.CreateCxxSourceContainer();
            source.AddFile("$(pkgroot)/source/main.cpp");
            source.AddFile("$(pkgroot)/source/myobject.cpp");
            source.AddFile("$(pkgroot)/source/myobject2.cpp");

            //var myObjectMoc = source.MocHeader(Bam.Core.V2.TokenizedString.Create("$(pkgroot)/source/myobject.h", this));
            //var myObject2Moc = source.MocHeader(Bam.Core.V2.TokenizedString.Create("$(pkgroot)/source/myobject2.h", this));

            this.PrivatePatch(settings =>
            {
                var gccLinker = settings as GccCommon.V2.ICommonLinkerOptions;
                if (gccLinker != null)
                {
                    gccLinker.CanUseOrigin = true;
                    gccLinker.RPath.Add("$ORIGIN");
                }
            });

            var qtCore = this.LinkAgainst<Qt.V2.Core>();
            source.UsePublicPatches(qtCore);

            var qtGui = this.LinkAgainst<Qt.V2.Gui>();
            source.UsePublicPatches(qtGui);
        }
    }

    class QtApplication :
        C.Application
    {
        public
        QtApplication(
            Bam.Core.Target target)
        {
            this.UpdateOptions += delegate(Bam.Core.IModule module, Bam.Core.Target delTarget) {
                var gccLink = module.Options as GccCommon.ILinkerOptions;
                if (null != gccLink)
                {
                    gccLink.CanUseOrigin = true;
                    gccLink.RPath.Add("$ORIGIN");
                }
            };
        }

        [Bam.Core.ModuleTargets(Platform=Bam.Core.EPlatform.Windows)]
        class Win32ResourceFile :
            C.Win32Resource
        {
            public
            Win32ResourceFile()
            {
                var resourcesDir = this.PackageLocation.SubDirectory("resources");
                this.Include(resourcesDir, "QtApplication.rc");
            }
        }

        class SourceFiles :
            C.Cxx.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "*.cpp");

                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
            }

            void
            SourceFiles_UpdateOptions(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                if (module.Options is MingwCommon.ICCompilerOptions)
                {
                    (module.Options as MingwCommon.ICCompilerOptions).Pedantic = false;
                }
                else if (module.Options is GccCommon.ICCompilerOptions)
                {
                    (module.Options as GccCommon.ICCompilerOptions).Pedantic = false;
                }
            }

#if false
            class MyMocFile :
                QtCommon.MocFile
            {
                public
                MyMocFile()
                {
                    var sourceDir = this.PackageLocation.SubDirectory("source");
                    this.Include(sourceDir, "myobject.h");
                }
            }

            [Bam.Core.DependentModules]
            Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(typeof(SourceFiles.MyMocFile));
#else
            class MyMocFiles :
                QtCommon.MocFileCollection
            {
                public
                MyMocFiles()
                {
                    var sourceDir = this.PackageLocation.SubDirectory("source");
                    this.Include(sourceDir, "*.h");

                    this.RegisterUpdateOptions(new Bam.Core.UpdateOptionCollectionDelegateArray(mocFile_UpdateOptions),
                                               sourceDir,
                                               "myobject2.h");
                }

                void
                mocFile_UpdateOptions(
                    Bam.Core.IModule module,
                    Bam.Core.Target target)
                {
                    var options = module.Options as QtCommon.IMocOptions;
                    if (null != options)
                    {
                        options.Defines.Add("CUSTOM_MOC_DEFINE_FOR_MYOBJECTS2");
                    }
                }
            }

            [Bam.Core.DependentModules]
            Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(typeof(SourceFiles.MyMocFiles));
#endif
        }

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(
            typeof(Qt.Core),
            typeof(Qt.Gui)
            );

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray winVCDependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray winVCLibraries = new Bam.Core.StringArray("KERNEL32.lib");

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows)]
        Bam.Core.TypeArray resourceFiles = new Bam.Core.TypeArray(
            typeof(Win32ResourceFile)
            );

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations]
        protected Bam.Core.Array<Publisher.PublishDependency> publishKeys = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.Application.OutputFile));
#endif
    }

    [Bam.Core.ModuleTargets(Platform=Bam.Core.EPlatform.OSX)]
    class AppInfoPList :
        XmlUtilities.OSXPlistModule
    {
        public
        AppInfoPList()
        {
            this.UpdateOptions += delegate(Bam.Core.IModule module, Bam.Core.Target target) {
                var options = module.Options as XmlUtilities.IOSXPlistOptions;
                options.CFBundleName = "QtApplication";
                options.CFBundleDisplayName = "QtApplication";
                options.CFBundleIdentifier = "QtApplication";
                options.CFBundleVersion = "1.0.0";
            };
        }

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(
            typeof(QtApplication)
        );

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations]
        Publisher.PublishDependency publishKey = new Publisher.PublishDependency(XmlUtilities.OSXPlistModule.OutputFile);
#endif
    }

#if D_PACKAGE_PUBLISHER_DEV
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
        System.Type primary = typeof(QtApplication);

        [Publisher.OSXInfoPList(Platform=Bam.Core.EPlatform.OSX)]
        System.Type plistType = typeof(AppInfoPList);
    }
#endif
}
