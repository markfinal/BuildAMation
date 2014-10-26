#region License
// <copyright>
//  Mark Final
// </copyright>
// <author>Mark Final</author>
#endregion // License
namespace QtCommon
{
    public abstract class Base :
        ThirdPartyModule
    {
        protected
        Base()
        {
            this.QtToolset = Bam.Core.State.Get("Toolset", "Qt") as Toolset;
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

        private Toolset QtToolset
        {
            get;
            set;
        }

        protected void
        AddIncludePath(
            C.ICCompilerOptions options,
            Bam.Core.Target target,
            string moduleName)
        {
            var includePath = this.QtToolset.GetIncludePath((Bam.Core.BaseTarget)target);
            if (!string.IsNullOrEmpty(includePath))
            {
                options.IncludePaths.Add(includePath);
                if (this.QtToolset.IncludePathIncludesQtModuleName)
                {
                    includePath = System.IO.Path.Combine(includePath, moduleName);
                    options.IncludePaths.Add(includePath);
                }
            }
        }

        protected void
        AddFrameworkIncludePath(
            C.ICCompilerOptionsOSX options,
            Bam.Core.Target target)
        {
            var libraryPath = this.QtToolset.GetLibraryPath((Bam.Core.BaseTarget)target);
            if (!string.IsNullOrEmpty(libraryPath))
            {
                options.FrameworkSearchDirectories.Add(libraryPath);
            }
        }

        protected void
        AddLibraryPath(
            C.ILinkerOptions options,
            Bam.Core.Target target)
        {
            var libraryPath = this.QtToolset.GetLibraryPath((Bam.Core.BaseTarget)target);
            if (!string.IsNullOrEmpty(libraryPath))
            {
                if (target.HasPlatform(Bam.Core.EPlatform.OSX))
                {
                    var osxOptions = options as C.ILinkerOptionsOSX;
                    osxOptions.FrameworkSearchDirectories.Add(libraryPath);
                }
                else
                {
                    options.LibraryPaths.Add(libraryPath);
                }
            }
        }

        protected void
        AddModuleLibrary(
            C.ILinkerOptions options,
            Bam.Core.Target target,
            bool hasQtPrefix,
            string moduleName)
        {
            if (target.HasPlatform(Bam.Core.EPlatform.Windows))
            {
                if (target.HasConfiguration(Bam.Core.EConfiguration.Debug))
                {
                    options.Libraries.Add(System.String.Format("{0}{1}d.lib", hasQtPrefix ? "Qt5" : string.Empty, moduleName));
                }
                else
                {
                    options.Libraries.Add(System.String.Format("{0}{1}.lib", hasQtPrefix ? "Qt5" : string.Empty, moduleName));
                }
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.Unix))
            {
                options.Libraries.Add(System.String.Format("-l{0}{1}", hasQtPrefix ? "Qt5" : string.Empty, moduleName));
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.OSX))
            {
                var osxLinkerOptions = options as C.ILinkerOptionsOSX;
                osxLinkerOptions.Frameworks.Add(System.String.Format("{0}{1}", hasQtPrefix ? "Qt" : string.Empty, moduleName));
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        protected void
        GetModuleDynamicLibrary(
            Bam.Core.Target target,
            string moduleName)
        {
            if (target.HasPlatform(Bam.Core.EPlatform.Windows))
            {
                var binPath = (this.QtToolset as Bam.Core.IToolset).BinPath((Bam.Core.BaseTarget)target);
                string dynamicLibraryName = null;
                if (target.HasConfiguration(Bam.Core.EConfiguration.Debug))
                {
                    dynamicLibraryName = System.String.Format("{0}d.dll", moduleName);
                }
                else
                {
                    dynamicLibraryName = System.String.Format("{0}.dll", moduleName);
                }
                var dynamicLibraryPath = System.IO.Path.Combine(binPath, dynamicLibraryName);
                this.Locations[C.DynamicLibrary.OutputFile] = Bam.Core.FileLocation.Get(dynamicLibraryPath);
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.Unix))
            {
                var libPath = this.QtToolset.GetLibraryPath((Bam.Core.BaseTarget)target);
                var version = (this.QtToolset as Bam.Core.IToolset).Version((Bam.Core.BaseTarget)target);
                var versionSplit = version.Split('.');
                var majorVersion = versionSplit[0];
                var minorVersion = versionSplit[1];
                var patchVersion = versionSplit[2];

                // real library name
                var realDynamicLibraryLeafname = System.String.Format("lib{0}.so.{1}.{2}.{3}", moduleName, majorVersion, minorVersion, patchVersion);
                var realDynamicLibraryPath = System.IO.Path.Combine(libPath, realDynamicLibraryLeafname);
                this.Locations[C.DynamicLibrary.OutputFile] = Bam.Core.FileLocation.Get(realDynamicLibraryPath);

                // so library name (major version)
                var soNameDynamicLibraryLeafname = System.String.Format("lib{0}.so.{1}", moduleName, majorVersion);
                var soNameDynamicLibraryPath = System.IO.Path.Combine(libPath, soNameDynamicLibraryLeafname);
                this.Locations[C.PosixSharedLibrarySymlinks.MajorVersionSymlink] = Bam.Core.SymlinkLocation.Get(soNameDynamicLibraryPath);

                // minor version library name
                var minorVersionDynamicLibraryLeafname = System.String.Format("lib{0}.so.{1}.{2}", moduleName, majorVersion, minorVersion);
                var minorVersionDynamicLibraryPath = System.IO.Path.Combine(libPath, minorVersionDynamicLibraryLeafname);
                this.Locations[C.PosixSharedLibrarySymlinks.MinorVersionSymlink] = Bam.Core.SymlinkLocation.Get(minorVersionDynamicLibraryPath);

                // linker library name
                var linkerDynamicLibraryLeafname = System.String.Format("lib{0}.so", moduleName);
                var linkerDynamicLibraryPath = System.IO.Path.Combine(libPath, linkerDynamicLibraryLeafname);
                this.Locations[C.PosixSharedLibrarySymlinks.LinkerSymlink] = Bam.Core.SymlinkLocation.Get(linkerDynamicLibraryPath);
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.OSX))
            {
                // TODO: this needs some rework with publishing, as it ought to be a framework
#if false
                return Bam.Core.FileLocation.Get(moduleName);
#endif
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations(Platform = Bam.Core.EPlatform.NotOSX)]
        protected Bam.Core.Array<Publisher.PublishDependency> publishKeys = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.DynamicLibrary.OutputFile));
#endif
    }
}
