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
namespace C
{
    // TODO: this does not implement any options interface
    public class PosixSharedLibrarySymlinksOptionCollection :
        Bam.Core.BaseOptionCollection,
        CommandLineProcessor.ICommandLineSupport
    {
        protected override void
         SetDelegates(
            Bam.Core.DependencyNode node)
        {}

        public
        PosixSharedLibrarySymlinksOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode owningNode)
        {}

        protected override void
        SetNodeSpecificData(
            Bam.Core.DependencyNode node)
        {
            var locationMap = node.Module.Locations;

            var moduleWithPostAction = node.ExternalDependents[0].Module;

            (node.Module as PosixSharedLibrarySymlinks).RealSharedLibraryFileLocation = moduleWithPostAction.Locations[C.DynamicLibrary.OutputFile];

            var outputFileDir = locationMap[C.PosixSharedLibrarySymlinks.OutputDir] as Bam.Core.ScaffoldLocation;
            if (!outputFileDir.IsValid)
            {
                outputFileDir.SetReference(moduleWithPostAction.Locations[C.DynamicLibrary.OutputDir]);
            }
        }

        private static string
        GetMajorVersionSymlinkLeafname(
            Bam.Core.Target target,
            string realSharedLibraryLeafname,
            C.ILinkerOptions linkerOptions)
        {
            var splitName = realSharedLibraryLeafname.Split('.');
            // On Linux
            // 0 = filename
            // 1 = 'so'
            // 2 = major version
            // 3 = minor version
            // 4 = patch version
            // On OSX
            // 0 = filename
            // 1 = major version
            // 2 = minor version
            // 3 = patch version
            // 4 = 'dylib'
            if (target.HasPlatform(Bam.Core.EPlatform.Linux))
            {
                var majorSymlinkLeafname = System.String.Format("{0}.{1}.{2}", splitName[0], splitName[1], linkerOptions.MajorVersion);
                return majorSymlinkLeafname;
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.OSX))
            {
                var majorSymlinkLeafname = System.String.Format("{0}.{1}.{2}", splitName[0], linkerOptions.MajorVersion, splitName[4]);
                return majorSymlinkLeafname;
            }

            throw new Bam.Core.Exception("Unsupported platform for Posix shared library symlink generation");
        }

        private static string
        GetMinorVersionSymlinkLeafname(
            Bam.Core.Target target,
            string realSharedLibraryLeafname,
            C.ILinkerOptions linkerOptions)
        {
            var splitName = realSharedLibraryLeafname.Split('.');
            // On Linux
            // 0 = filename
            // 1 = 'so'
            // 2 = major version
            // 3 = minor version
            // 4 = patch version
            // On OSX
            // 0 = filename
            // 1 = major version
            // 2 = minor version
            // 3 = patch version
            // 4 = 'dylib'
            if (target.HasPlatform(Bam.Core.EPlatform.Linux))
            {
                var majorSymlinkLeafname = System.String.Format("{0}.{1}.{2}.{3}", splitName[0], splitName[1], linkerOptions.MajorVersion, linkerOptions.MinorVersion);
                return majorSymlinkLeafname;
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.OSX))
            {
                var majorSymlinkLeafname = System.String.Format("{0}.{1}.{2}.{3}", splitName[0], linkerOptions.MajorVersion, linkerOptions.MinorVersion, splitName[4]);
                return majorSymlinkLeafname;
            }

            throw new Bam.Core.Exception("Unsupported platform for Posix shared library symlink generation");
        }

        private static string
        GetLinkerSymlinkLeafname(
            Bam.Core.Target target,
            string realSharedLibraryLeafname,
            C.ILinkerOptions linkerOptions)
        {
            var splitName = realSharedLibraryLeafname.Split('.');
            // On Linux
            // 0 = filename
            // 1 = 'so'
            // 2 = major version
            // 3 = minor version
            // 4 = patch version
            // On OSX
            // 0 = filename
            // 1 = major version
            // 2 = minor version
            // 3 = patch version
            // 4 = 'dylib'
            if (target.HasPlatform(Bam.Core.EPlatform.Linux))
            {
                var majorSymlinkLeafname = System.String.Format("{0}.{1}", splitName[0], splitName[1]);
                return majorSymlinkLeafname;
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.OSX))
            {
                var majorSymlinkLeafname = System.String.Format("{0}.{1}", splitName[0], splitName[4]);
                return majorSymlinkLeafname;
            }

            throw new Bam.Core.Exception("Unsupported platform for Posix shared library symlink generation");
        }

        public override void
        FinalizeOptions(
            Bam.Core.DependencyNode node)
        {
            var locationMap = node.Module.Locations;

            var moduleWithPostAction = node.ExternalDependents[0].Module;
            var linkerOptions = moduleWithPostAction.Options as C.ILinkerOptions;

            var realSharedLibraryPath = (node.Module as PosixSharedLibrarySymlinks).RealSharedLibraryFileLocation.GetSingleRawPath();
            var realSharedLibraryLeafName = System.IO.Path.GetFileName(realSharedLibraryPath);

            // major version symlink
            var majorSymlinkFile = locationMap[C.PosixSharedLibrarySymlinks.MajorVersionSymlink] as Bam.Core.ScaffoldLocation;
            if (!majorSymlinkFile.IsValid)
            {
                var majorSymlinkLeafname = GetMajorVersionSymlinkLeafname(node.Target, realSharedLibraryLeafName, linkerOptions);
                majorSymlinkFile.SpecifyStub(locationMap[C.PosixSharedLibrarySymlinks.OutputDir], majorSymlinkLeafname, Bam.Core.Location.EExists.WillExist);

                // append this location to the invoking module
                var copiedSymlink = new Bam.Core.ScaffoldLocation(Bam.Core.ScaffoldLocation.ETypeHint.Symlink);
                copiedSymlink.SetReference(majorSymlinkFile);
                moduleWithPostAction.Locations[C.PosixSharedLibrarySymlinks.MajorVersionSymlink] = copiedSymlink;
            }

            // minor version symlink
            var minorSymlinkFile = locationMap[C.PosixSharedLibrarySymlinks.MinorVersionSymlink] as Bam.Core.ScaffoldLocation;
            if (!minorSymlinkFile.IsValid)
            {
                var minorSymlinkLeafname = GetMinorVersionSymlinkLeafname(node.Target, realSharedLibraryLeafName, linkerOptions);
                minorSymlinkFile.SpecifyStub(locationMap[C.PosixSharedLibrarySymlinks.OutputDir], minorSymlinkLeafname, Bam.Core.Location.EExists.WillExist);

                // append this location to the invoking module
                var copiedSymlink = new Bam.Core.ScaffoldLocation(Bam.Core.ScaffoldLocation.ETypeHint.Symlink);
                copiedSymlink.SetReference(minorSymlinkFile);
                moduleWithPostAction.Locations[C.PosixSharedLibrarySymlinks.MinorVersionSymlink] = copiedSymlink;
            }

            // linker symlink
            var linkerSymlinkFile = locationMap[C.PosixSharedLibrarySymlinks.LinkerSymlink] as Bam.Core.ScaffoldLocation;
            if (!linkerSymlinkFile.IsValid)
            {
                var linkerSymlinkLeafname = GetLinkerSymlinkLeafname(node.Target, realSharedLibraryLeafName, linkerOptions);
                linkerSymlinkFile.SpecifyStub(locationMap[C.PosixSharedLibrarySymlinks.OutputDir], linkerSymlinkLeafname, Bam.Core.Location.EExists.WillExist);

                // append this location to the invoking module
                var copiedSymlink = new Bam.Core.ScaffoldLocation(Bam.Core.ScaffoldLocation.ETypeHint.Symlink);
                copiedSymlink.SetReference(linkerSymlinkFile);
                moduleWithPostAction.Locations[C.PosixSharedLibrarySymlinks.LinkerSymlink] = copiedSymlink;
            }
        }

        void
        CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(
            Bam.Core.StringArray commandLineBuilder,
            Bam.Core.Target target,
            Bam.Core.StringArray excludedOptionNames)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target, excludedOptionNames);
        }
    }
}
