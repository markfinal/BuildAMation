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
