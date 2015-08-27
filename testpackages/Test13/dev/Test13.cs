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
using Bam.Core.V2; // for EPlatform.PlatformExtensions
using QtCommon.V2.MocExtension;
namespace Test13
{
    sealed class QtApplicationV2 :
        C.V2.ConsoleApplication
    {
        public QtApplicationV2()
        {
            this.BitDepth = C.V2.EBit.ThirtyTwo;
        }

        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            var headers = this.CreateHeaderContainer();
            var myobjectHeader = headers.AddFile("$(pkgroot)/source/myobject.h");
            var myobject2Header = headers.AddFile("$(pkgroot)/source/myobject2.h");

            var source = this.CreateCxxSourceContainer();
            source.AddFile("$(pkgroot)/source/main.cpp");
            source.AddFile("$(pkgroot)/source/myobject.cpp");
            source.AddFile("$(pkgroot)/source/myobject2.cpp");

            /*var myObjectMocTuple = */source.MocHeader(myobjectHeader);
            /*var myObject2MocTuple = */source.MocHeader(myobject2Header);

            this.PrivatePatch(settings =>
            {
                var gccLinker = settings as GccCommon.V2.ICommonLinkerOptions;
                if (gccLinker != null)
                {
                    gccLinker.CanUseOrigin = true;
                    gccLinker.RPath.Add("$ORIGIN");
                }
            });

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                this.CompileAndLinkAgainst<Qt.V2.CoreFramework>(source);
                this.CompileAndLinkAgainst<Qt.V2.GuiFramework>(source);
            }
            else
            {
                this.CompileAndLinkAgainst<Qt.V2.Core>(source);
                this.CompileAndLinkAgainst<Qt.V2.Gui>(source);
            }

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDKV2>();
            }
        }
    }

    sealed class RuntimePackage :
        Publisher.V2.Package
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var app = this.Include<QtApplicationV2>(C.V2.ConsoleApplication.Key, EPublishingType.WindowedApplication);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
            }
            else
            {
                this.Include<Qt.V2.Core>(C.V2.DynamicLibrary.Key, ".", app);
                this.Include<Qt.V2.Gui>(C.V2.DynamicLibrary.Key, ".", app);
            }
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
