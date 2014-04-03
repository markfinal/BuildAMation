// Automatically generated by Opus v
namespace BundlingTest
{
    /// <summary>
    /// A simple command line application
    /// </summary>
    class Application : C.Application
    {
        class SourceFiles : C.Cxx.ObjectFileCollection
        {
            public SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                var appDir = sourceDir.SubDirectory("app");
                this.Include(appDir, "*.cpp");
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles source = new SourceFiles();

        [Opus.Core.DependentModules]
        Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(
            typeof(DynamicLibrary)
            );
    }

    /// <summary>
    /// A windowed application. On OSX, this is an application bundle
    /// </summary>
    class WindowedApplication : C.WindowsApplication
    {
        public WindowedApplication()
        {
            this.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target target)
            {
                var osxOptions = module.Options as C.ILinkerOptionsOSX;
                if (null != osxOptions)
                {
                    osxOptions.ApplicationBundle = true;
                }
            };
        }

        class SourceFiles : C.Cxx.ObjectFileCollection
        {
            public SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                var appDir = sourceDir.SubDirectory("app");
                this.Include(appDir, "*.cpp");
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles source = new SourceFiles();

        [Opus.Core.DependentModules]
        Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(
            typeof(DynamicLibrary)
            );
    }

    class DynamicLibrary : C.DynamicLibrary
    {
        public DynamicLibrary()
        {
            var includeDir = this.PackageLocation.SubDirectory("include");
            var dynLibDir = includeDir.SubDirectory("dynlib");
            this.headers.Include(dynLibDir, "*.h");
        }

        class SourceFiles : C.Cxx.ObjectFileCollection
        {
            public SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                var dynLibDir = sourceDir.SubDirectory("dynlib");
                this.Include(dynLibDir, "*.cpp");

                this.UpdateOptions += IncludePaths;
            }
        }

        [C.ExportCompilerOptionsDelegate]
        public static void IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as C.ICCompilerOptions;
            if (null != options)
            {
                options.IncludePaths.Include((module as Opus.Core.BaseModule).PackageLocation.SubDirectory("include"));
            }
        }

        [C.HeaderFiles]
        Opus.Core.FileCollection headers = new Opus.Core.FileCollection();

        [Opus.Core.SourceFiles]
        SourceFiles source = new SourceFiles();
    }

    class BundlingApplicationForDebugger : FileUtilities.CopyFileCollection
    {
        public BundlingApplicationForDebugger(Opus.Core.Target target)
        {
            this.Include(target, C.OutputFileFlags.Executable, typeof(DynamicLibrary));
        }

        // TODO: would be nice to have a TypeArray here
        [FileUtilities.BesideModule(C.OutputFileFlags.Executable)]
        System.Type nextTo = typeof(Application);
    }

    class BundlingWindowedApplicationForDebugger : FileUtilities.CopyFileCollection
    {
        public BundlingWindowedApplicationForDebugger(Opus.Core.Target target)
        {
            this.Include(target, C.OutputFileFlags.Executable, typeof(DynamicLibrary));
        }

        [FileUtilities.BesideModule(C.OutputFileFlags.Executable)]
        System.Type nextTo = typeof(WindowedApplication);
    }

    class BundleApplicationToFolder : FileUtilities.CopyFileCollection
    {
        public BundleApplicationToFolder(Opus.Core.Target target)
        {
            this.Include(target, C.OutputFileFlags.Executable, typeof(DynamicLibrary));
            this.Include(target, C.OutputFileFlags.Executable, typeof(Application));
            this.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target delegateTarget) {
                var options = module.Options as FileUtilities.ICopyFileOptions;
                if (null != options)
                {
                    var baseTarget = (Opus.Core.BaseTarget)delegateTarget;
                    var bundleDir = Opus.Core.State.BuildRootLocation.SubDirectory("Bundle" + baseTarget.ConfigurationName('='), Opus.Core.Location.EExists.WillExist);
                    options.DestinationDirectory = bundleDir.AbsolutePath;
                }
            };
        }
    }

    [Opus.Core.ModuleTargets(Platform=Opus.Core.EPlatform.NotOSX)]
    class BundleWindowedApplicationToFolder : FileUtilities.CopyFileCollection
    {
        public BundleWindowedApplicationToFolder(Opus.Core.Target target)
        {
            this.Include(target, C.OutputFileFlags.Executable, typeof(DynamicLibrary));
            this.Include(target, C.OutputFileFlags.Executable, typeof(WindowedApplication));
            this.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target delegateTarget) {
                var options = module.Options as FileUtilities.ICopyFileOptions;
                if (null != options)
                {
                    var baseTarget = (Opus.Core.BaseTarget)delegateTarget;
                    var bundleDir = Opus.Core.State.BuildRootLocation.SubDirectory("Bundle" + baseTarget.ConfigurationName('='), Opus.Core.Location.EExists.WillExist);
                    options.DestinationDirectory = bundleDir.AbsolutePath;
                }
            };
        }
    }

    [Opus.Core.ModuleTargets(Platform=Opus.Core.EPlatform.OSX)]
    class BundleWindowedApplicationToFolderOSX : FileUtilities.CopyDirectory
    {
        public BundleWindowedApplicationToFolderOSX(Opus.Core.Target target)
        {
            // TODO: this does not compile yet
            this.Include(target, C.OutputFileFlags.OSXBundle, typeof(WindowedApplication));
            this.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target delegateTarget) {
                var options = module.Options as FileUtilities.ICopyFileOptions;
                if (null != options)
                {
                    var baseTarget = (Opus.Core.BaseTarget)delegateTarget;
                    var bundleDir = Opus.Core.State.BuildRootLocation.SubDirectory("Bundle" + baseTarget.ConfigurationName('='), Opus.Core.Location.EExists.WillExist);
                    options.DestinationDirectory = bundleDir.AbsolutePath;
                }
            };
        }
    }
}
