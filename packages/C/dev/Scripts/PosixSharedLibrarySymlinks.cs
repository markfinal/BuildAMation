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
#endregion // License
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
