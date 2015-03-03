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
namespace ICU
{
    abstract class ICUBase :
        C.ThirdPartyModule
    {
        protected
        ICUBase()
        {
#if D_PACKAGE_PUBLISHER_DEV
            // TODO: can this be automated?
            if (Bam.Core.OSUtilities.IsUnixHosting)
            {
                this.publishKeys.AddUnique(new Publisher.PublishDependency(C.PosixSharedLibrarySymlinks.MajorVersionSymlink));
                this.publishKeys.AddUnique(new Publisher.PublishDependency(C.PosixSharedLibrarySymlinks.LinkerSymlink));
            }
#endif
        }

        protected Bam.Core.Location
        GetWindowsDLLPath(
            string filename)
        {
            var prebuiltDir = this.PackageLocation.SubDirectory("bin");
            var platformDir = prebuiltDir.SubDirectory("win32-msvc10");
            var binDir = platformDir.SubDirectory("bin");
            var dll = Bam.Core.FileLocation.Get(binDir, filename);
            return dll;
        }

        protected Bam.Core.Location
        GetLinuxOSDirectory()
        {
            var prebuiltDir = this.PackageLocation.SubDirectory("bin");
            var platformDir = prebuiltDir.SubDirectory("linux64-gcc44");
            var libDir = platformDir.SubDirectory("lib");
            return libDir;
        }

        protected Bam.Core.Location
        GetLinuxSOPath(
            string filename,
            bool isSymLink)
        {
            var prebuiltDir = this.PackageLocation.SubDirectory("bin");
            var platformDir = prebuiltDir.SubDirectory("linux64-gcc44");
            var libDir = platformDir.SubDirectory("lib");
            if (isSymLink)
            {
                var so = Bam.Core.SymlinkLocation.Get(libDir, filename);
                return so;
            }
            else
            {
                var so = Bam.Core.FileLocation.Get(libDir, filename);
                return so;
            }
        }

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations(Platform = Bam.Core.EPlatform.NotOSX)]
        protected Bam.Core.Array<Publisher.PublishDependency> publishKeys = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.DynamicLibrary.OutputFile));
#endif
    }

    sealed class ICUIN :
        ICUBase
    {
        public
        ICUIN(
            Bam.Core.Target target)
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(ICUIN_LinkerOptions);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        ICUIN_LinkerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            if (target.HasPlatform(Bam.Core.EPlatform.Unix))
            {
                var options = module.Options as C.ILinkerOptions;
                if (null != options)
                {
                    options.LibraryPaths.Add(this.GetLinuxOSDirectory().GetSinglePath());
                    options.Libraries.Add("-licui18n");
                }
            }
        }

        public override void
        RegisterOutputFiles(
            Bam.Core.BaseOptionCollection options,
            Bam.Core.Target target,
            string modulePath)
        {
            if (target.HasPlatform(Bam.Core.EPlatform.Windows))
            {
                this.Locations[C.DynamicLibrary.OutputFile] = this.GetWindowsDLLPath("ICUIN51.dll");
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.Unix))
            {
                this.Locations[C.DynamicLibrary.OutputFile] = this.GetLinuxSOPath("libicui18n.so.51.2", false);
                this.Locations[C.PosixSharedLibrarySymlinks.MajorVersionSymlink] = this.GetLinuxSOPath("libicui18n.so.51", true);
                this.Locations[C.PosixSharedLibrarySymlinks.LinkerSymlink] = this.GetLinuxSOPath("libicui18n.so", true);
            }
            base.RegisterOutputFiles(options, target, modulePath);
        }
    }

    sealed class ICUUC :
        ICUBase
    {
        public
        ICUUC(
            Bam.Core.Target target)
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(ICUUC_LinkerOptions);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        ICUUC_LinkerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            if (target.HasPlatform(Bam.Core.EPlatform.Unix))
            {
                var options = module.Options as C.ILinkerOptions;
                if (null != options)
                {
                    options.LibraryPaths.Add(this.GetLinuxOSDirectory().GetSinglePath());
                    options.Libraries.Add("-licuuc");
                }
            }
        }

        public override void
        RegisterOutputFiles(
            Bam.Core.BaseOptionCollection options,
            Bam.Core.Target target,
            string modulePath)
        {
            if (target.HasPlatform(Bam.Core.EPlatform.Windows))
            {
                this.Locations[C.DynamicLibrary.OutputFile] = this.GetWindowsDLLPath("ICUUC51.dll");
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.Unix))
            {
                this.Locations[C.DynamicLibrary.OutputFile] = this.GetLinuxSOPath("libicuuc.so.51.2", false);
                this.Locations[C.PosixSharedLibrarySymlinks.MajorVersionSymlink] = this.GetLinuxSOPath("libicuuc.so.51", true);
                this.Locations[C.PosixSharedLibrarySymlinks.LinkerSymlink] = this.GetLinuxSOPath("libicuuc.so", true);
            }
            base.RegisterOutputFiles(options, target, modulePath);
        }
    }

    sealed class ICUDT :
        ICUBase
    {
        public
        ICUDT(
            Bam.Core.Target target)
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(ICUDT_LinkerOptions);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        ICUDT_LinkerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            if (target.HasPlatform(Bam.Core.EPlatform.Unix))
            {
                var options = module.Options as C.ILinkerOptions;
                if (null != options)
                {
                    options.LibraryPaths.Add(this.GetLinuxOSDirectory().GetSinglePath());
                    options.Libraries.Add("-licudata");
                }
            }
        }

        public override void
        RegisterOutputFiles(
            Bam.Core.BaseOptionCollection options,
            Bam.Core.Target target,
            string modulePath)
        {
            if (target.HasPlatform(Bam.Core.EPlatform.Windows))
            {
                this.Locations[C.DynamicLibrary.OutputFile] = this.GetWindowsDLLPath("ICUDT51.dll");
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.Unix))
            {
                this.Locations[C.DynamicLibrary.OutputFile] = this.GetLinuxSOPath("libicudata.so.51.2", false);
                this.Locations[C.PosixSharedLibrarySymlinks.MajorVersionSymlink] = this.GetLinuxSOPath("libicudata.so.51", true);
                this.Locations[C.PosixSharedLibrarySymlinks.LinkerSymlink] = this.GetLinuxSOPath("libicudata.so", true);
            }
            base.RegisterOutputFiles(options, target, modulePath);
        }
    }
}
