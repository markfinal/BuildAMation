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
namespace C
{
    public interface ILinkerOptions
    {
        /// <summary>
        /// Specify the output type of the linked binary
        /// </summary>
        C.ELinkerOutput OutputType
        {
            get;
            set;
        }

        /// <summary>
        /// Exclude standard libraries from the linking phase
        /// </summary>
        bool DoNotAutoIncludeStandardLibraries
        {
            get;
            set;
        }

        /// <summary>
        /// Generate debug symbols for the linked binary
        /// </summary>
        bool DebugSymbols
        {
            get;
            set;
        }

        /// <summary>
        /// Specify the subsystem for the linked binary
        /// </summary>
        C.ESubsystem SubSystem
        {
            get;
            set;
        }

        /// <summary>
        /// Specify search paths for libraries
        /// </summary>
        Bam.Core.DirectoryCollection LibraryPaths
        {
            get;
            set;
        }

        /// <summary>
        /// Specify standard libraries to link against
        /// </summary>
        Bam.Core.FileCollection StandardLibraries
        {
            get;
            set;
        }

        /// <summary>
        /// Specify user libraries to link against
        /// </summary>
        Bam.Core.FileCollection Libraries
        {
            get;
            set;
        }

        /// <summary>
        /// The link step generates a map file for the binary
        /// </summary>
        bool GenerateMapFile
        {
            get;
            set;
        }

        /// <summary>
        /// Additional options passed to the linker
        /// </summary>
        string AdditionalOptions
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the major version. Used by Posix shared libraries.
        /// </summary>
        /// <value>
        /// The major version.
        /// </value>
        [Bam.Core.ValueOnlyOption]
        int MajorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minor version. Used by Posix shared libraries.
        /// </summary>
        /// <value>
        /// The minor version.
        /// </value>
        [Bam.Core.ValueOnlyOption]
        int MinorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the patch version. Used by Posix shared libraries.
        /// </summary>
        /// <value>
        /// The patch version.
        /// </value>
        [Bam.Core.ValueOnlyOption]
        int PatchVersion
        {
            get;
            set;
        }
    }
}
