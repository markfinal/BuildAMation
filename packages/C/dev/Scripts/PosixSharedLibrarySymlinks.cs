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
    [Bam.Core.ModuleToolAssignment(typeof(IPosixSharedLibrarySymlinksTool))]
    public class PosixSharedLibrarySymlinks :
        Bam.Core.BaseModule
    {
        public static readonly Bam.Core.LocationKey OutputDir = new Bam.Core.LocationKey("PosixSOSymlinkOutputDirectory", Bam.Core.ScaffoldLocation.ETypeHint.Directory);
        public static readonly Bam.Core.LocationKey LinkerSymlink = new Bam.Core.LocationKey("PosixSOSymlinkLinkerSymlink", Bam.Core.ScaffoldLocation.ETypeHint.Symlink);
        public static readonly Bam.Core.LocationKey MajorVersionSymlink = new Bam.Core.LocationKey("PosixSOSymlinkMajorVersionSymlink", Bam.Core.ScaffoldLocation.ETypeHint.Symlink);
        public static readonly Bam.Core.LocationKey MinorVersionSymlink = new Bam.Core.LocationKey("PosixSOSymlinkMinorVersionSymlink", Bam.Core.ScaffoldLocation.ETypeHint.Symlink);

        public Bam.Core.Location RealSharedLibraryFileLocation
        {
            get;
            set;
        }
    }
}
