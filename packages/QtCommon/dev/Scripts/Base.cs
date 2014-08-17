#region License
// Copyright 2010-2014 Mark Final
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
#endregion
namespace QtCommon
{
    public abstract class Base :
        ThirdPartyModule
    {
        protected
        Base()
        {
            this.QtToolset = Bam.Core.State.Get("Toolset", "Qt") as Toolset;

#if OPUSPACKAGE_PUBLISHER_DEV
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
        AddLibraryPath(
            C.ILinkerOptions options,
            Bam.Core.Target target)
        {
            var libraryPath = this.QtToolset.GetLibraryPath((Bam.Core.BaseTarget)target);
            if (!string.IsNullOrEmpty(libraryPath))
            {
                options.LibraryPaths.Add(libraryPath);
            }
        }

        protected void
        AddModuleLibrary(
            C.ILinkerOptions options,
            Bam.Core.Target target,
            string moduleName)
        {
            if (target.HasPlatform(Bam.Core.EPlatform.Windows))
            {
                if (target.HasConfiguration(Bam.Core.EConfiguration.Debug))
                {
                    options.Libraries.Add(System.String.Format("{0}d4.lib", moduleName));
                }
                else
                {
                    options.Libraries.Add(System.String.Format("{0}4.lib", moduleName));
                }
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.Unix))
            {
                options.Libraries.Add(System.String.Format("-l{0}", moduleName));
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.OSX))
            {
                var osxLinkerOptions = options as C.ILinkerOptionsOSX;
                osxLinkerOptions.Frameworks.Add(moduleName);
            }
            else
            {
                // TODO: framework
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
                    dynamicLibraryName = System.String.Format("{0}d4.dll", moduleName);
                }
                else
                {
                    dynamicLibraryName = System.String.Format("{0}4.dll", moduleName);
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

#if OPUSPACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations(Platform=Bam.Core.EPlatform.NotOSX)]
        protected Bam.Core.Array<Publisher.PublishDependency> publishKeys = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.DynamicLibrary.OutputFile));
#endif
    }
}
