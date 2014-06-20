// <copyright file="PosixSharedLibrarySymlinksOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    // TODO: this does not implement any options interface
    public class PosixSharedLibrarySymlinksOptionCollection :
        Opus.Core.BaseOptionCollection,
        CommandLineProcessor.ICommandLineSupport
    {
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            // do nothing yet
        }

        public PosixSharedLibrarySymlinksOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        protected override void SetDefaultOptionValues(Opus.Core.DependencyNode owningNode)
        {
            // do nothing
        }

        protected override void SetNodeSpecificData(Opus.Core.DependencyNode node)
        {
            var locationMap = node.Module.Locations;

            var moduleWithPostAction = node.ExternalDependents[0].Module;

            (node.Module as PosixSharedLibrarySymlinks).RealSharedLibraryFileLocation = moduleWithPostAction.Locations[C.DynamicLibrary.OutputFile];

            var outputFileDir = locationMap[C.PosixSharedLibrarySymlinks.OutputDir] as Opus.Core.ScaffoldLocation;
            if (!outputFileDir.IsValid)
            {
                outputFileDir.SetReference(moduleWithPostAction.Locations[C.DynamicLibrary.OutputDir]);
            }
        }

        private static string
        GetMajorVersionSymlinkLeafname(
            Opus.Core.Target target,
            string realSharedLibraryLeafname,
            C.ILinkerOptions linkerOptions)
        {
            var splitName = realSharedLibraryLeafname.Split('.');
            // On Unix
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
            if (target.HasPlatform(Opus.Core.EPlatform.Unix))
            {
                var majorSymlinkLeafname = System.String.Format("{0}.{1}.{2}", splitName[0], splitName[1], linkerOptions.MajorVersion);
                return majorSymlinkLeafname;
            }
            else if (target.HasPlatform(Opus.Core.EPlatform.OSX))
            {
                var majorSymlinkLeafname = System.String.Format("{0}.{1}.{2}", splitName[0], linkerOptions.MajorVersion, splitName[4]);
                return majorSymlinkLeafname;
            }

            throw new Opus.Core.Exception("Unsupported platform for Posix shared library symlink generation");
        }

        private static string
        GetMinorVersionSymlinkLeafname(
            Opus.Core.Target target,
            string realSharedLibraryLeafname,
            C.ILinkerOptions linkerOptions)
        {
            var splitName = realSharedLibraryLeafname.Split('.');
            // On Unix
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
            if (target.HasPlatform(Opus.Core.EPlatform.Unix))
            {
                var majorSymlinkLeafname = System.String.Format("{0}.{1}.{2}.{3}", splitName[0], splitName[1], linkerOptions.MajorVersion, linkerOptions.MinorVersion);
                return majorSymlinkLeafname;
            }
            else if (target.HasPlatform(Opus.Core.EPlatform.OSX))
            {
                var majorSymlinkLeafname = System.String.Format("{0}.{1}.{2}.{3}", splitName[0], linkerOptions.MajorVersion, linkerOptions.MinorVersion, splitName[4]);
                return majorSymlinkLeafname;
            }

            throw new Opus.Core.Exception("Unsupported platform for Posix shared library symlink generation");
        }

        private static string
        GetLinkerSymlinkLeafname(
            Opus.Core.Target target,
            string realSharedLibraryLeafname,
            C.ILinkerOptions linkerOptions)
        {
            var splitName = realSharedLibraryLeafname.Split('.');
            // On Unix
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
            if (target.HasPlatform(Opus.Core.EPlatform.Unix))
            {
                var majorSymlinkLeafname = System.String.Format("{0}.{1}", splitName[0], splitName[1]);
                return majorSymlinkLeafname;
            }
            else if (target.HasPlatform(Opus.Core.EPlatform.OSX))
            {
                var majorSymlinkLeafname = System.String.Format("{0}.{1}", splitName[0], splitName[4]);
                return majorSymlinkLeafname;
            }

            throw new Opus.Core.Exception("Unsupported platform for Posix shared library symlink generation");
        }

        public override void FinalizeOptions (Opus.Core.DependencyNode node)
        {
            var locationMap = node.Module.Locations;

            var moduleWithPostAction = node.ExternalDependents[0].Module;
            var linkerOptions = moduleWithPostAction.Options as C.ILinkerOptions;

            var realSharedLibraryPath = (node.Module as PosixSharedLibrarySymlinks).RealSharedLibraryFileLocation.GetSingleRawPath();

            // major version symlink
            var majorSymlinkFile = locationMap[C.PosixSharedLibrarySymlinks.MajorVersionSymlink] as Opus.Core.ScaffoldLocation;
            if (!majorSymlinkFile.IsValid)
            {
                var majorSymlinkLeafname = GetMajorVersionSymlinkLeafname(node.Target, realSharedLibraryPath, linkerOptions);
                majorSymlinkFile.SpecifyStub(locationMap[C.PosixSharedLibrarySymlinks.OutputDir], majorSymlinkLeafname, Opus.Core.Location.EExists.WillExist);

                // append this location to the invoking module
                var copiedSymlink = new Opus.Core.ScaffoldLocation(Opus.Core.ScaffoldLocation.ETypeHint.File);
                copiedSymlink.SetReference(majorSymlinkFile);
                moduleWithPostAction.Locations[C.PosixSharedLibrarySymlinks.MajorVersionSymlink] = copiedSymlink;
            }

            // minor version symlink
            var minorSymlinkFile = locationMap[C.PosixSharedLibrarySymlinks.MinorVersionSymlink] as Opus.Core.ScaffoldLocation;
            if (!minorSymlinkFile.IsValid)
            {
                var minorSymlinkLeafname = GetMinorVersionSymlinkLeafname(node.Target, realSharedLibraryPath, linkerOptions);
                minorSymlinkFile.SpecifyStub(locationMap[C.PosixSharedLibrarySymlinks.OutputDir], minorSymlinkLeafname, Opus.Core.Location.EExists.WillExist);

                // append this location to the invoking module
                var copiedSymlink = new Opus.Core.ScaffoldLocation(Opus.Core.ScaffoldLocation.ETypeHint.File);
                copiedSymlink.SetReference(minorSymlinkFile);
                moduleWithPostAction.Locations[C.PosixSharedLibrarySymlinks.MinorVersionSymlink] = copiedSymlink;
            }

            // linker symlink
            var linkerSymlinkFile = locationMap[C.PosixSharedLibrarySymlinks.LinkerSymlink] as Opus.Core.ScaffoldLocation;
            if (!linkerSymlinkFile.IsValid)
            {
                var linkerSymlinkLeafname = GetLinkerSymlinkLeafname(node.Target, realSharedLibraryPath, linkerOptions);
                linkerSymlinkFile.SpecifyStub(locationMap[C.PosixSharedLibrarySymlinks.OutputDir], linkerSymlinkLeafname, Opus.Core.Location.EExists.WillExist);

                // append this location to the invoking module
                var copiedSymlink = new Opus.Core.ScaffoldLocation(Opus.Core.ScaffoldLocation.ETypeHint.File);
                copiedSymlink.SetReference(linkerSymlinkFile);
                moduleWithPostAction.Locations[C.PosixSharedLibrarySymlinks.LinkerSymlink] = copiedSymlink;
            }
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target, Opus.Core.StringArray excludedOptionNames)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target, excludedOptionNames);
        }
    }
}
