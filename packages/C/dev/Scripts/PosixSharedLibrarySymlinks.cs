// <copyright file="PosixSharedLibrarySymlinks.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    /// <summary>
    /// Posix shared library symlink creation
    /// </summary>
    [Opus.Core.ModuleToolAssignment(typeof(IPosixSharedLibrarySymlinksTool))]
    public class PosixSharedLibrarySymlinks : Opus.Core.BaseModule
    {
        public static readonly Opus.Core.LocationKey OutputDir = new Opus.Core.LocationKey("PosixSOSymlinkOutputDirectory", Opus.Core.ScaffoldLocation.ETypeHint.Directory);
        public static readonly Opus.Core.LocationKey LinkerSymlink = new Opus.Core.LocationKey("PosixSOSymlinkLinkerSymlink", Opus.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Opus.Core.LocationKey MajorVersionSymlink = new Opus.Core.LocationKey("PosixSOSymlinkMajorVersionSymlink", Opus.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Opus.Core.LocationKey MinorVersionSymlink = new Opus.Core.LocationKey("PosixSOSymlinkMinorVersionSymlink", Opus.Core.ScaffoldLocation.ETypeHint.File);

        public Opus.Core.Location RealSharedLibraryFileLocation
        {
            get;
            set;
        }
    }
}